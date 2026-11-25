using lsfinan.Models;

namespace lsfinan.ViewModel
{
    public class TodasSaidasViewModel
    {
        public List<Saida> Saidas { get; set; } = new();

        public int MesSelecionado { get; set; }

        public int AnoSelecionado { get; set; }

        public List<DateTime> MesesDisponiveis { get; set; } = new();

        public int PaginaAtual { get; set; }

        public int TotalPaginas { get; set; }
    }
}