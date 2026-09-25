
using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<Categoria>> ListarCategorias(Usuario usuario);
        Task<Categoria> CadastrarCategoria(CategoriaViewModel categoria, Usuario usuario);
    }
}