using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lsfinan.Enum;


namespace lsfinan.Models
{
    public class Conta
    {
        public int Id { get; set; }

        public string NomeConta { get; set; } = string.Empty;

        public decimal SaldoInicial { get; set; }

        public EnumTipoConta TipoConta { get; set; }

        public int UsuarioId { get; set; }
    }
}
