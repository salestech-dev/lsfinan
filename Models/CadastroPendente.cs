
using System.ComponentModel.DataAnnotations;

namespace lsfinan.Models
{
    public class CadastroPendente
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        public string CodigoHash { get; set; } = string.Empty;

        public DateTime CodigoExpiraEm { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoEnvioEm { get; set; }

        public int Tentativas { get; set; }
    }
}
