using lsfinan.Data;
using lsfinan.Exceptions;
using lsfinan.Interfaces;
using lsfinan.Models;
using lsfinan.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace lsfinan.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly AppDbContext _context;

        public CategoriaService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<Categoria> CadastrarCategoria(CategoriaViewModel categoria, Usuario usuario)
        {
            var nome = categoria.NomeDaCategoria.Trim();

            var jaExiste = await _context.Categorias.AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.NomeCategoria.ToLower() == nome.ToLower());

            if (jaExiste)
            {
                throw new RegraDeNegocioException($"A categoria \"{nome}\" já existe.");
            }

            var novaCategoria = new Categoria
            {
                Ativa = categoria.Ativa,
                NomeCategoria = nome,
                UsuarioId = usuario.Id
            };

            _context.Categorias.Add(novaCategoria);
            await _context.SaveChangesAsync();

            return novaCategoria;
        }

        public async Task<Categoria> AtualizarCategoria(int id, CategoriaViewModel categoriaViewModel, Usuario usuario)
        {
            var categoriaExistente = await _context.Categorias
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (categoriaExistente == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            var nome = categoriaViewModel.NomeDaCategoria.Trim();

            var nomeEmUso = await _context.Categorias.AnyAsync(x =>
                x.UsuarioId == usuario.Id &&
                x.Id != id &&
                x.NomeCategoria.ToLower() == nome.ToLower());

            if (nomeEmUso)
            {
                throw new RegraDeNegocioException($"A categoria \"{nome}\" já existe.");
            }

            categoriaExistente.NomeCategoria = nome;
            categoriaExistente.Ativa = categoriaViewModel.Ativa;

            await _context.SaveChangesAsync();

            return categoriaExistente;
        }

        public async Task<CategoriaViewModel> ObterParaEdicao(int id, Usuario usuario)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (categoria == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            return new CategoriaViewModel
            {
                Id = categoria.Id,
                NomeDaCategoria = categoria.NomeCategoria,
                Ativa = categoria.Ativa
            };
        }

        public async Task Excluir(int id, Usuario usuario)
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuario.Id);

            if (categoria == null)
            {
                throw new RegraDeNegocioException("Categoria não encontrada.");
            }

            var emUso =
                await _context.Entradas.AnyAsync(x => x.UsuarioId == usuario.Id && x.CategoriaId == id) ||
                await _context.Saidas.AnyAsync(x => x.UsuarioId == usuario.Id && x.CategoriaId == id) ||
                await _context.ContasPagar.AnyAsync(x => x.UsuarioId == usuario.Id && x.CategoriaId == id);

            if (emUso)
            {
                throw new RegraDeNegocioException(
                    $"A categoria \"{categoria.NomeCategoria}\" já tem movimentações ou contas a pagar " +
                    "e não pode ser excluída. Edite-a e deixe como inativa para ela " +
                    "não aparecer mais nos novos cadastros.");
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }

        public Task<List<Categoria>> ListarCategorias(Usuario usuario)
        {
            return _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id)
                .OrderBy(x => x.NomeCategoria)
                .ToListAsync();
        }

        public Task<List<Categoria>> ListarCategoriasAtivas(Usuario usuario)
        {
            return _context.Categorias
                .Where(x => x.UsuarioId == usuario.Id && x.Ativa)
                .OrderBy(x => x.NomeCategoria)
                .ToListAsync();
        }
    }
}
