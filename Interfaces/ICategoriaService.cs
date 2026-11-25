using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ListarCategorias(Usuario usuario);
        Task<Categoria> CadastrarCategoria(CategoriaViewModel categoria, Usuario usuario);
        Task<List<Categoria>> ListarCategoriasAtivas(Usuario usuario);
        Task<Categoria> AtualizarCategoria(int id, CategoriaViewModel categoria, Usuario usuario);
        Task<CategoriaViewModel> ObterParaEdicao(int id, Usuario usuario);
        Task Excluir(int id, Usuario usuario);
    }
}
