using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.Data;
using lsfinan.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> ObterDashboardAsync(int usuarioId)
        {

            var userSaldoInicial = await _context.Contas
                .Where(x => x.UsuarioId == usuarioId)
                .Select(x => x.SaldoInicial)
                .SumAsync();

            var userEntradas = await _context.Entradas
                .Where(x => x.UsuarioId == usuarioId && x.Recebido)
                .SumAsync(x => x.ValorEntrada);

            var userSaidas = await _context.Saidas
                .Where(x => x.UsuarioId == usuarioId && x.Pago)
                .SumAsync(x => x.ValorSaida);

            var nomeUsuario = await _context.Usuarios
                .Where(x => x.Id == usuarioId)
                .Select(x => x.Nome)
                .FirstOrDefaultAsync() ?? "Usuário";

            var userSaldo = userSaldoInicial + userEntradas - userSaidas;

            var userResultado = userEntradas - userSaidas;

            return new DashboardViewModel
            {
                SaldoTotal = userSaldo,
                TotalEntradas = userEntradas,
                TotalSaidas = userSaidas,
                Resultado = userResultado,
                NomeUsuario = nomeUsuario
            };
        }
    }
}