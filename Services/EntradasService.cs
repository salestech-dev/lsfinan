
using lsfinan.Data;
using lsfinan.Interfaces;
using lsfinan.Models;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class EntradasService : IEntradaService
    {
        private readonly AppDbContext _context;

        public EntradasService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<Entrada>> ListarEntradas(Usuario usuario)
        {
            var entradas = await _context.Entradas
            .Where(e => e.UsuarioId == usuario.Id)
            .ToListAsync();
            return entradas;
        }

        public async Task<Entrada> CadastrarEntrada(
            EntradaViewModel entrada,
            Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(usuario.Id);

            if (usuarioExistente == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            var novaEntrada = new Entrada
            {
                DescricaoEntrada = entrada.DescricaoEntrada,
                ValorEntrada = entrada.ValorEntrada,
                DataEntrada = entrada.DataEntrada,

                CategoriaId = entrada.CategoriaId,
                ContaId = entrada.ContaId,

                ReceberDepois = entrada.ReceberDepois,

                // Se NÃO vai receber depois, já recebeu
                Recebido = !entrada.ReceberDepois,

                DataRecebimento = entrada.ReceberDepois
                    ? null
                    : DateTime.Now,

                UsuarioId = usuario.Id
            };

            _context.Entradas.Add(novaEntrada);

            await _context.SaveChangesAsync();

            return novaEntrada;
        }
    }
}