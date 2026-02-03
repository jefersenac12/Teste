using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Admin.Models
{
    public class AtividadeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da atividade é obrigatório")]
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        // Propriedade de exibição (para compatibilidade)
        public string ValorFormatado => Valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
    }
}