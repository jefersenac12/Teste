using System.Text.Json.Serialization;

namespace Teste.Models
{
    public class Pagamento
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("reservaId")]
        public int ReservaId { get; set; }

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("metodo")]
        public string Metodo { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("dataPagamento")]
        public DateTime? DataPagamento { get; set; }

        [JsonPropertyName("chavePix")]
        public string? ChavePix { get; set; }

        [JsonPropertyName("qrCode")]
        public string? QrCode { get; set; }

        // Propriedades de navegação (opcional)
        [JsonIgnore]
        public Reserva? Reserva { get; set; }
    }
}
