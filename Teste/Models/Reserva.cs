using System.Text.Json.Serialization;

namespace Teste.Models
{
    public class Reserva
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("agendaId")]
        public int AgendaId { get; set; }

        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }

        [JsonPropertyName("npEntrada")]
        public int NpEntrada { get; set; }

        [JsonPropertyName("meiaEntrada")]
        public int MeiaEntrada { get; set; }

        [JsonPropertyName("inteiraEntrada")]
        public int InteiraEntrada { get; set; }

        [JsonPropertyName("dataReserva")]
        public DateTime DataReserva { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // Propriedades de navegação (opcional)
        [JsonIgnore]
        public Agenda? Agenda { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}
