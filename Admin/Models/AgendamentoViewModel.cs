using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class AgendamentoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A safra é obrigatória")]
        public int SafraId { get; set; }

        [Required(ErrorMessage = "A atividade é obrigatória")]
        public int AtividadeId { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "O número de vagas totais é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Deve haver pelo menos 1 vaga")]
        public int VagasTotais { get; set; }

        [Required(ErrorMessage = "O número de vagas disponíveis é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "Vagas disponíveis não podem ser negativas")]
        public int VagasDisponiveis { get; set; }

        // Propriedades de navegação/exibição
        public string? SafraNome { get; set; }
        public string? AtividadeNome { get; set; }

        // Propriedades calculadas
        public string DataFormatada => DataHora.ToString("dd/MM/yyyy");
        public string HoraFormatada => DataHora.ToString("HH:mm");
        public string DataHoraFormatada => DataHora.ToString("dd/MM/yyyy HH:mm");
        public bool Disponivel => VagasDisponiveis > 0;
        public double PercentualOcupado => VagasTotais > 0 ? (double)(VagasTotais - VagasDisponiveis) / VagasTotais * 100 : 0;
        public string Status => VagasDisponiveis == 0 ? "Esgotado" : VagasDisponiveis < VagasTotais * 0.2 ? "Poucas vagas" : "Disponível";
    }
}
