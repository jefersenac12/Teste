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

        // Propriedades de navegação (para compatibilidade com views existentes)
        public string? SafraNome { get; set; }
        public string? AtividadeNome { get; set; }
        public string DataFormatada => DataHora.ToString("dd/MM/yyyy");
        public string HoraFormatada => DataHora.ToString("HH:mm");
        public string DataHoraFormatada => DataHora.ToString("dd/MM/yyyy HH:mm");
    }
}
