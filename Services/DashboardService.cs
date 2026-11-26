using lsfinan.Data;
using lsfinan.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class DashboardService : IDashboardService
    {
        private const int QuantidadeMovimentacoes = 8;

        private readonly AppDbContext _context;
        private readonly IContaPagarService _contaPagarService;

        public DashboardService(AppDbContext context, IContaPagarService contaPagarService)
        {
            _context = context;
            _contaPagarService = contaPagarService;
        }

        public async Task<DashboardViewModel> ObterDashboardAsync(int usuarioId)
        {
            var userSaldoInicial = await _context.Contas
                .Where(x => x.UsuarioId == usuarioId)
                .SumAsync(x => (decimal?)x.SaldoInicial) ?? 0m;

            var userEntradas = await _context.Entradas
                .Where(x => x.UsuarioId == usuarioId)
                .SumAsync(x => (decimal?)x.ValorEntrada) ?? 0m;

            var userSaidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuarioId)
                .SumAsync(x => (decimal?)x.ValorSaida) ?? 0m;

            var nomeUsuario = await _context.Usuarios
                .Where(x => x.Id == usuarioId)
                .Select(x => x.Nome)
                .FirstOrDefaultAsync() ?? "Usuário";

            var contas = await _context.Contas
                .Where(x => x.UsuarioId == usuarioId)
                .ToListAsync();

            string NomeDaConta(int contaId) =>
                contas.FirstOrDefault(c => c.Id == contaId)?.NomeConta ?? string.Empty;

            var ultimasEntradas = await _context.Entradas
                .Where(x => x.UsuarioId == usuarioId)
                .OrderByDescending(x => x.DataEntrada)
                .Take(QuantidadeMovimentacoes)
                .ToListAsync();

            var ultimasSaidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuarioId)
                .OrderByDescending(x => x.DataSaida)
                .Take(QuantidadeMovimentacoes)
                .ToListAsync();

            var movimentacoes = ultimasEntradas
                .Select(e => new MovimentacaoViewModel
                {
                    Descricao = string.IsNullOrWhiteSpace(e.DescricaoEntrada) ? e.NomeEntrada : e.DescricaoEntrada,
                    Valor = e.ValorEntrada,
                    Data = e.DataEntrada,
                    Tipo = "Entrada",
                    Conta = NomeDaConta(e.ContaId)
                })
                .Concat(ultimasSaidas.Select(s => new MovimentacaoViewModel
                {
                    Descricao = s.NomeSaida,
                    Valor = s.ValorSaida,
                    Data = s.DataSaida,
                    Tipo = "Saída",
                    Conta = NomeDaConta(s.ContaId)
                }))
                .OrderByDescending(m => m.Data)
                .Take(QuantidadeMovimentacoes)
                .ToList();

            return new DashboardViewModel
            {
                SaldoTotal = userSaldoInicial + userEntradas - userSaidas,
                TotalEntradas = userEntradas,
                TotalSaidas = userSaidas,
                Resultado = userEntradas - userSaidas,
                NomeUsuario = nomeUsuario,
                UltimasMovimentacoes = movimentacoes,
                Alertas = await _contaPagarService.ObterAlertas(usuarioId)
            };
        }
    }
}
