using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface IContaPagarService
    {
        Task<ContaPagarListagemViewModel> Listar(Usuario usuario, string filtro);

        /// <summary>Contas atrasadas e que vencem nos próximos dias.</summary>
        Task<AlertasContasPagarViewModel> ObterAlertas(int usuarioId);

        Task<ContaPagarViewModel> NovoFormulario(Usuario usuario);
        Task PreencherCategorias(ContaPagarViewModel viewModel, Usuario usuario);
        Task<ContaPagarViewModel> ObterParaEdicao(int id, Usuario usuario);

        Task<ContaPagar> Cadastrar(ContaPagarViewModel viewModel, Usuario usuario);
        Task Editar(int id, ContaPagarViewModel viewModel, Usuario usuario);
        Task Excluir(int id, Usuario usuario);

        Task<PagarContaPagarViewModel> ObterParaPagamento(int id, Usuario usuario);

        /// <summary>Paga com uma conta ou dividindo entre várias (gera as saídas).</summary>
        Task Pagar(int id, List<AlocacaoPagamentoViewModel> alocacoes, Usuario usuario);

        Task DesfazerPagamento(int id, Usuario usuario);
    }
}
