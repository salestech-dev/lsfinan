namespace lsfinan.ViewModel
{
    /// <summary>
    /// Dados de uma saída a ser registrada. Usado tanto pelo cadastro
    /// de saída quanto pelo pagamento de uma conta a pagar.
    /// </summary>
    public class NovaSaidaDados
    {
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public decimal Valor { get; set; }

        public int CategoriaId { get; set; }

        /// <summary>Conta única escolhida no formulário (caminho simples).</summary>
        public int ContaId { get; set; }

        /// <summary>Quando preenchida, ignora ContaId e usa estas contas.</summary>
        public List<AlocacaoPagamentoViewModel> Alocacoes { get; set; } = new();

        public int? ContaPagarId { get; set; }

        /// <summary>Novas saídas exigem categoria ativa; quitar conta antiga não.</summary>
        public bool ExigirCategoriaAtiva { get; set; } = true;
    }
}
