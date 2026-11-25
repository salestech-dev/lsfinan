using lsfinan.Data;
using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class ContaService : IContaService
    {
        private readonly AppDbContext _context;

        public ContaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Conta> CadastrarConta(ContaViewModel contaViewModel, Usuario usuario)
        {
            var nome = contaViewModel.NomeConta.Trim();

            var jaExiste = await _context.Contas.AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.NomeConta.ToLower() == nome.ToLower());

            if (jaExiste)
            {
                throw new RegraDeNegocioException("Você já tem uma conta com esse nome.");
            }

            var novaConta = new Conta
            {
                NomeConta = nome,
                TipoConta = contaViewModel.TipoConta,
                SaldoInicial = contaViewModel.SaldoInicial,
                Ativa = contaViewModel.Ativa,
                UsuarioId = usuario.Id
            };

            await _context.Contas.AddAsync(novaConta);
            await _context.SaveChangesAsync();
            return novaConta;
        }

        public async Task<List<ContaListagemViewModel>> ListarContasComSaldo(Usuario usuario)
        {
            var contas = await _context.Contas
                .Where(x => x.UsuarioId == usuario.Id)
                .OrderBy(x => x.NomeConta)
                .ToListAsync();

            var entradas = await _context.Entradas
                .Where(x => x.UsuarioId == usuario.Id)
                .GroupBy(x => x.ContaId)
                .Select(g => new { ContaId = g.Key, Total = g.Sum(x => x.ValorEntrada) })
                .ToListAsync();

            var saidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuario.Id)
                .GroupBy(x => x.ContaId)
                .Select(g => new { ContaId = g.Key, Total = g.Sum(x => x.ValorSaida) })
                .ToListAsync();

            return contas
                .Select(conta => new ContaListagemViewModel
                {
                    Id = conta.Id,
                    NomeConta = conta.NomeConta,
                    TipoConta = conta.TipoConta,
                    SaldoInicial = conta.SaldoInicial,
                    Ativa = conta.Ativa,
                    SaldoAtual = conta.SaldoInicial
                        + entradas.Where(x => x.ContaId == conta.Id).Sum(x => x.Total)
                        - saidas.Where(x => x.ContaId == conta.Id).Sum(x => x.Total)
                })
                .ToList();
        }

        public async Task<List<ContaListagemViewModel>> ListarContasAtivasComSaldo(Usuario usuario)
        {
            var contas = await ListarContasComSaldo(usuario);
            return contas.Where(c => c.Ativa).ToList();
        }

        public async Task<decimal> ObterSaldoAtual(int contaId, int usuarioId)
        {
            var conta = await _context.Contas
                .FirstOrDefaultAsync(x => x.Id == contaId && x.UsuarioId == usuarioId);

            if (conta == null)
            {
                throw new RegraDeNegocioException("Conta não encontrada.");
            }

            var entradas = await _context.Entradas
                .Where(x => x.UsuarioId == usuarioId && x.ContaId == contaId)
                .SumAsync(x => (decimal?)x.ValorEntrada) ?? 0m;

            var saidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuarioId && x.ContaId == contaId)
                .SumAsync(x => (decimal?)x.ValorSaida) ?? 0m;

            return conta.SaldoInicial + entradas - saidas;
        }

        public Task<List<Conta>> ListarContas(Usuario usuario)
        {
            return _context.Contas
                .Where(x => x.UsuarioId == usuario.Id)
                .OrderBy(x => x.NomeConta)
                .ToListAsync();
        }

        public Task<List<Conta>> ListarContasAtivas(Usuario usuario)
        {
            return _context.Contas
                .Where(x => x.UsuarioId == usuario.Id && x.Ativa)
                .OrderBy(x => x.NomeConta)
                .ToListAsync();
        }

        public async Task<ContaViewModel> ObterParaEdicao(int id, Usuario usuario)
        {
            var conta = await _context.Contas
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (conta == null)
            {
                throw new RegraDeNegocioException("Conta não encontrada.");
            }

            return new ContaViewModel
            {
                Id = conta.Id,
                NomeConta = conta.NomeConta,
                SaldoInicial = conta.SaldoInicial,
                TipoConta = conta.TipoConta,
                Ativa = conta.Ativa
            };
        }

        public async Task EditarConta(int id, ContaViewModel conta, Usuario usuario)
        {
            var contaExistente = await _context.Contas
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (contaExistente == null)
            {
                throw new RegraDeNegocioException("Conta não encontrada.");
            }

            var nome = conta.NomeConta.Trim();

            var nomeEmUso = await _context.Contas.AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.Id != id &&
                x.NomeConta.ToLower() == nome.ToLower());

            if (nomeEmUso)
            {
                throw new RegraDeNegocioException("Você já tem outra conta com esse nome.");
            }

            contaExistente.NomeConta = nome;
            contaExistente.TipoConta = conta.TipoConta;
            contaExistente.SaldoInicial = conta.SaldoInicial;
            contaExistente.Ativa = conta.Ativa;

            await _context.SaveChangesAsync();
        }
    }
}
