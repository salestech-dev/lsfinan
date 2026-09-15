
using Microsoft.AspNetCore.Identity;
namespace lsfinan.Models
{
    public class Usuario : IdentityUser<int>
    {
        public string Nome { get; set; } = string.Empty;
        public ICollection<Conta> Contas { get; set; } = new List<Conta>();

        public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();

        public ICollection<Entrada> Entradas { get; set; } = new List<Entrada>();

        public ICollection<Saida> Saidas { get; set; } = new List<Saida>();
    }
}