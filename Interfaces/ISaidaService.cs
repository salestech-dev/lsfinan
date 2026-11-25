using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface ISaidaService
    {
        /// <summary>
        /// Registra a saída. Devolve uma saída por conta usada
        /// (mais de uma quando o valor foi dividido entre contas).
        /// </summary>
        Task<List<Saida>> CadastrarSaida(Usuario usuario, SaidaViewModel saidaViewModel);

        /// <summary>
        /// Valida as regras e adiciona as saídas ao contexto SEM salvar,
        /// para que quem chama salve tudo junto (ex.: pagar conta a pagar).
        /// </summary>
        Task<List<Saida>> AdicionarSaidasAsync(Usuario usuario, NovaSaidaDados dados);

        Task<List<Saida>> ListarSaida(Usuario usuario);
        Task<List<Saida>> ListarSaidasDeHoje(Usuario usuario);
        Task<TodasSaidasViewModel> ListarTodasSaidas(Usuario usuario, int mes, int ano, int pagina = 1);
        Task<List<DateTime>> ListarMesesDisponiveis(Usuario usuario);

        Task<SaidaViewModel> ObterParaEdicao(int id, Usuario usuario);
        Task EditarSaida(int id, SaidaViewModel saida, Usuario usuario);
        Task ExcluirSaida(int id, Usuario usuario);
    }
}
