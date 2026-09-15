using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lsfinan.Models
{public class Entrada
{
    public int Id { get; set; }

    public string NomeEntrada { get; set; } = string.Empty;

    public decimal ValorEntrada { get; set; }

    public DateTime DataEntrada { get; set; }

    public int CategoriaId { get; set; }

    public int ContaId { get; set; }

    public string DescricaoEntrada { get; set; } = string.Empty;

    public bool ReceberDepois { get; set; }

    public bool Recebido { get; set; }

    public DateTime? DataRecebimento { get; set; }

    public int UsuarioId { get; set; }
}
}