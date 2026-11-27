
using System.Security.Cryptography;
using lsfinan.Data;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class CadastroPendenteService : ICadastroPendenteService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public CadastroPendenteService(
            AppDbContext context,
            IEmailService emailService,
            UserManager<Usuario> userManager,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
        }

        public async Task<IdentityResult> IniciarCadastroAsync(
            UsuarioCadastroViewModel cadastro)
        {
            var email = cadastro.Email.Trim();

            var usuarioExistente =
                await _userManager.FindByEmailAsync(email);

            if (usuarioExistente != null)
            {
                return Falha("Este e-mail já está cadastrado.");
            }

            var usuario = new Usuario
            {
                UserName = email,
                Email = email,
                Nome = cadastro.Nome.Trim()
            };

            // Valida a senha usando as regras configuradas no Identity.
            foreach (var validador in _userManager.PasswordValidators)
            {
                var resultadoSenha = await validador.ValidateAsync(
                    _userManager,
                    usuario,
                    cadastro.Senha);

                if (!resultadoSenha.Succeeded)
                {
                    return IdentityResult.Failed(resultadoSenha.Errors.ToArray());
                }
            }

            var agora = DateTime.UtcNow;

            var pendente = await _context.CadastrosPendentes
                .FirstOrDefaultAsync(x =>
                    x.Email.ToUpper() == email.ToUpper());

            if (pendente != null &&
                pendente.UltimoEnvioEm.HasValue &&
                agora < pendente.UltimoEnvioEm.Value.AddSeconds(60))
            {
                return Falha(
                    "Aguarde 60 segundos antes de solicitar outro código.");
            }

            var codigo = GerarCodigo();

            if (pendente == null)
            {
                pendente = new CadastroPendente
                {
                    Email = email,
                    CriadoEm = agora
                };

                _context.CadastrosPendentes.Add(pendente);
            }

            // A senha original nunca é armazenada no banco.
            pendente.Nome = usuario.Nome;
            pendente.SenhaHash =
                _passwordHasher.HashPassword(usuario, cadastro.Senha);

            // O código também é armazenado somente como hash.
            pendente.CodigoHash =
                _passwordHasher.HashPassword(usuario, codigo);

            pendente.CodigoExpiraEm = agora.AddMinutes(10);
            pendente.UltimoEnvioEm = agora;
            pendente.Tentativas = 0;

            await _context.SaveChangesAsync();

            await _emailService.EnviarCodigoConfirmacaoAsync(
                email,
                codigo);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ConfirmarCadastroAsync(
            string email,
            string codigo)
        {
            email = email.Trim();

            var pendente = await _context.CadastrosPendentes
                .FirstOrDefaultAsync(x =>
                    x.Email.ToUpper() == email.ToUpper());

            if (pendente == null)
            {
                return Falha(
                    "Cadastro pendente não encontrado. Faça o cadastro novamente.");
            }

            var agora = DateTime.UtcNow;

            if (agora > pendente.CodigoExpiraEm)
            {
                _context.CadastrosPendentes.Remove(pendente);
                await _context.SaveChangesAsync();

                return Falha(
                    "O código expirou. Faça o cadastro novamente.");
            }

            if (pendente.Tentativas >= 5)
            {
                return Falha(
                    "Limite de tentativas atingido. Solicite um novo código.");
            }

            var usuario = new Usuario
            {
                UserName = pendente.Email,
                Email = pendente.Email,
                Nome = pendente.Nome
            };

            var verificacao = _passwordHasher.VerifyHashedPassword(
                usuario,
                pendente.CodigoHash,
                codigo);

            if (verificacao == PasswordVerificationResult.Failed)
            {
                pendente.Tentativas++;
                await _context.SaveChangesAsync();

                return Falha(
                    "Código incorreto. Verifique e tente novamente.");
            }

            var usuarioExistente =
                await _userManager.FindByEmailAsync(pendente.Email);

            if (usuarioExistente != null)
            {
                _context.CadastrosPendentes.Remove(pendente);
                await _context.SaveChangesAsync();

                return Falha("Este e-mail já está cadastrado.");
            }

            // Recupera o hash da senha original.
            usuario.PasswordHash = pendente.SenhaHash;
            usuario.EmailConfirmed = true;
            usuario.DataCadastro = agora;

            var resultado = await _userManager.CreateAsync(usuario);

            if (!resultado.Succeeded)
            {
                return resultado;
            }

            _context.CadastrosPendentes.Remove(pendente);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task ReenviarCodigoAsync(string email)
        {
            email = email.Trim();

            var pendente = await _context.CadastrosPendentes
                .FirstOrDefaultAsync(x =>
                    x.Email.ToUpper() == email.ToUpper());

            if (pendente == null)
            {
                throw new InvalidOperationException(
                    "Cadastro pendente não encontrado.");
            }

            var agora = DateTime.UtcNow;

            if (pendente.UltimoEnvioEm.HasValue &&
                agora < pendente.UltimoEnvioEm.Value.AddSeconds(60))
            {
                throw new InvalidOperationException(
                    "Aguarde 60 segundos antes de solicitar outro código.");
            }

            var codigo = GerarCodigo();

            var usuario = new Usuario
            {
                UserName = pendente.Email,
                Email = pendente.Email,
                Nome = pendente.Nome
            };

            pendente.CodigoHash =
                _passwordHasher.HashPassword(usuario, codigo);

            pendente.CodigoExpiraEm = agora.AddMinutes(10);
            pendente.UltimoEnvioEm = agora;
            pendente.Tentativas = 0;

            await _context.SaveChangesAsync();

            await _emailService.EnviarCodigoConfirmacaoAsync(
                pendente.Email,
                codigo);
        }

        private static string GerarCodigo()
        {
            return RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");
        }

        private static IdentityResult Falha(string mensagem)
        {
            return IdentityResult.Failed(
                new IdentityError
                {
                    Description = mensagem
                });
        }
    }
}
