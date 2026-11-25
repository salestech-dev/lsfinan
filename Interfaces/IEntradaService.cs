using lsfinan.Models;

namespace lsfinan.Interfaces
{
    public interface IEntradaService
    {
        Task<List<Entrada>> ListarEntradas(Usuario Usuario);
        Task<Entrada> CadastrarEntrada(EntradaViewModel entrada, Usuario usuario);
        Task<List<Entrada>> ListarTodasEntradas(Usuario usuario);
        Task<EntradaViewModel> ObterParaEdicao(int id, Usuario usuario);
        Task EditarEntrada(int id, EntradaViewModel entrada, Usuario usuario);
        Task ExcluirEntrada(int id, Usuario usuario);
    }
}
