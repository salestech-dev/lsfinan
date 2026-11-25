using System.ComponentModel.DataAnnotations.Schema;

namespace lsfinan.Models
{
    public class Saida
    {
        public int Id { get; set; }
        public string NomeSaida { get; set; } = string.Empty;
        public decimal ValorSaida { get; set; }
        public DateTime DataSaida { get; set; }
        public int CategoriaId { get; set; }
        public string DescricaoSaida { get; set; } = string.Empty;
        public bool Pago { get; set; }
        public int ContaId { get; set; }
        public DateTime? DataPagamento { get; set; }
        public int UsuarioId { get; set; }

        /// <summary>
        /// Preenchido quando a saída foi gerada pelo pagamento de uma
        /// conta a pagar. Essas saídas só são desfeitas pela própria conta a pagar.
        /// </summary>
        public int? ContaPagarId { get; set; }

        // ---- Campos apenas de exibição (não vão para o banco) ----

        /// <summary>Saldo da conta logo depois desta saída.</summary>
        [NotMapped]
        public decimal? SaldoRestante { get; set; }

        [NotMapped]
        public string NomeConta { get; set; } = string.Empty;

        [NotMapped]
        public string NomeCategoria { get; set; } = string.Empty;
    }
}
