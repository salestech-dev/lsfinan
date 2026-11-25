namespace lsfinan.ViewModel
{
    /// <summary>
    /// Dados do painel "de onde sai o dinheiro": escolher outra conta ou
    /// dividir o valor entre duas ou mais. Usado ao cadastrar uma saída sem
    /// saldo suficiente e ao pagar uma conta a pagar.
    /// </summary>
    public class DivisaoPagamentoViewModel
    {
        public decimal Valor { get; set; }

        public List<ContaListagemViewModel> Contas { get; set; } = new();

        /// <summary>Valores já preenchidos em cada conta (sugestão ou escolha anterior).</summary>
        public List<AlocacaoPagamentoViewModel> Alocacoes { get; set; } = new();

        /// <summary>Mensagem de erro/aviso mostrada no topo do painel.</summary>
        public string? Mensagem { get; set; }

        // Contexto do "saldo insuficiente" (opcional)
        public string? NomeContaEscolhida { get; set; }
        public decimal SaldoContaEscolhida { get; set; }

        public decimal SaldoTotalDisponivel =>
            Contas.Sum(c => Math.Max(0m, c.SaldoAtual));

        public bool ContasCobremValor => SaldoTotalDisponivel >= Valor;
    }
}
