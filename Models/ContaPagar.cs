namespace lsfinan.Models
{
    /// <summary>
    /// Conta a pagar (boleto, fatura, mensalidade...). Ela só vira dinheiro
    /// que saiu de verdade (Saida) quando o usuário confirma o pagamento.
    /// </summary>
    public class ContaPagar
    {
        public int Id { get; set; }

        public string NomeContaPagar { get; set; } = string.Empty;

        public string DescricaoContaPagar { get; set; } = string.Empty;

        public decimal ValorContaPagar { get; set; }

        public DateTime DataVencimento { get; set; }

        public int CategoriaId { get; set; }

        public bool Pago { get; set; }

        public DateTime? DataPagamento { get; set; }

        public DateTime DataCadastro { get; set; }

        public int UsuarioId { get; set; }
    }
}
