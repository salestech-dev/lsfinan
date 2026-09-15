using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Categoria
{
    public int Id { get; set; }

    public string NomeCategoria { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public int UsuarioId { get; set; }
}