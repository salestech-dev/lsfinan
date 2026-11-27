
namespace lsfinan.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCodigoConfirmacaoAsync(string email, string codigo);
    }
}
