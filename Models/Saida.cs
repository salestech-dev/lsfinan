using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public bool PagarDepois { get; set; }
        public bool Pago { get; set; }
        public int ContaId { get; set; }
        public DateTime? DataPagamento { get; set; }
        public int UsuarioId { get; set; }

    }
}