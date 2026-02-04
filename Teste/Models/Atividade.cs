using System.Text.Json.Serialization;

namespace Teste.Models
{
    public class Atividade
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("agendas")]
        public List<object>? Agendas { get; set; }
    }
}
