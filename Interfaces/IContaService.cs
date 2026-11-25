using lsfinan.Models;
using lsfinan.ViewModel;

namespace lsfinan.Interfaces
{
    public interface IContaService
    {
        Task<List<Conta>> ListarContas(Usuario usuario);

        /// <summary>Só as contas ativas (as únicas que aceitam movimentações novas).</summary>
        Task<List<Conta>> ListarContasAtivas(Usuario usuario);

        Task<List<ContaListagemViewModel>> ListarContasComSaldo(Usuario usuario);

        Task<List<ContaListagemViewModel>> ListarContasAtivasComSaldo(Usuario usuario);

        Task<decimal> ObterSaldoAtual(int contaId, int usuarioId);

        Task<Conta> CadastrarConta(ContaViewModel contaViewModel, Usuario usuario);

        Task<ContaViewModel> ObterParaEdicao(int id, Usuario usuario);

        Task EditarConta(int id, ContaViewModel conta, Usuario usuario);
    }
}
