using System.Text.Json.Serialization;

namespace Teste.Models
{
    public class Safra
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("dataInicio")]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("dataFim")]
        public DateTime DataFim { get; set; }

        [JsonPropertyName("descricao")]
        public string? Descricao { get; set; }
    }
}
