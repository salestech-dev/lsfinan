using lsfinan.Data;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class ContaService : IContaService
    {

        public ContaService(AppDbContext context)
        {
            _context = context;
        }

        private readonly AppDbContext _context;


        public async Task<Conta> CadastrarConta(ContaViewModel contaViewModel, Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(usuario.Id);
            if (usuarioExistente == null)
            {
                throw new Exception("Usuario não encontrado.");
            }

            var novaConta = new Conta
            {
                NomeConta = contaViewModel.NomeConta,
                TipoConta = contaViewModel.TipoConta,
                SaldoInicial = contaViewModel.SaldoInicial,
                UsuarioId = usuario.Id
            };

            await _context.AddAsync(novaConta);
            await _context.SaveChangesAsync();
            return novaConta;

        }

        public Task<List<Conta>> ListarContas(Usuario usuario)
        {
            var contas = _context.Contas
            .Where(x => x.UsuarioId == usuario.Id)
            .ToListAsync();

            return contas;

        }
    }
}