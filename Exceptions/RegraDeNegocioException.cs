namespace lsfinan.Exceptions
{
    /// <summary>
    /// Erro previsto pelas regras do sistema. A mensagem já é amigável
    /// e pode ser mostrada direto para o usuário.
    /// </summary>
    public class RegraDeNegocioException : Exception
    {
        public RegraDeNegocioException(string mensagem) : base(mensagem)
        {
        }
    }
}
