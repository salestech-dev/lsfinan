
using Resend;
using lsfinan.Interfaces;

namespace lsfinan.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;

        public EmailService(IResend resend)
        {
            _resend = resend;
        }

        public async Task EnviarCodigoConfirmacaoAsync(string email, string codigo)
        {
            var mensagem = new EmailMessage
            {
                From = Environment.GetEnvironmentVariable("EMAIL_FROM")
    ?? throw new InvalidOperationException(
        "A variável EMAIL_FROM não foi configurada."),

                To = new EmailAddressList { email },

                Subject = "Código de confirmação - WiseWallet",

                HtmlBody = $@"
                    <div style='font-family: Arial; max-width: 500px; margin: auto;'>
                        <h2>Bem-vindo ao WiseWallet!</h2>
                        <p>Use o código abaixo para confirmar seu endereço de e-mail:</p>
                        <h1 style='letter-spacing: 8px;'>{codigo}</h1>
                        <p>Este código expira em 10 minutos.</p>
                        <p>Se você não solicitou este cadastro, ignore este e-mail.</p>
                    </div>"
            };

            await _resend.EmailSendAsync(mensagem);
        }
    }
}
