using System.Text.Json.Serialization;

namespace Teste.Models
{
    public class Agenda
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("safraId")]
        public int SafraId { get; set; }

        [JsonPropertyName("atividadeId")]
        public int AtividadeId { get; set; }

        [JsonPropertyName("dataHora")]
        public DateTime DataHora { get; set; }

        [JsonPropertyName("vagasTotais")]
        public int VagasTotais { get; set; }

        [JsonPropertyName("vagasDisponiveis")]
        public int VagasDisponiveis { get; set; }

        // Propriedades de navegação (opcional)
        [JsonIgnore]
        public Safra? Safra { get; set; }

        [JsonIgnore]
        public Atividade? Atividade { get; set; }
    }
}
