
using lsfinan.Models;

namespace lsfinan.Interfaces
{
    public interface IEntradaService
    {
        Task<List<Entrada>> ListarEntradas(Usuario Usuario);
        Task<Entrada> CadastrarEntrada(EntradaViewModel entrada, Usuario usuario);
    }
}