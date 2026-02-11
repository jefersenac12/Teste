using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Admin.Models
{
    public class ComunicadoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O conteúdo é obrigatório")]
        [JsonPropertyName("conteudo")]
        public string Conteudo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo é obrigatório")]
        [JsonPropertyName("tipo")]
        public TipoComunicado Tipo { get; set; }

        [JsonPropertyName("dataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [JsonPropertyName("dataInicio")]
        public DateTime? DataInicio { get; set; }

        [JsonPropertyName("dataFim")]
        public DateTime? DataFim { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; } = true;

        [JsonPropertyName("autor")]
        public string? Autor { get; set; }

        // Propriedades de exibição
        public string TipoDescricao => Tipo.GetDescription();
        public string DataCriacaoFormatada => DataCriacao.ToString("dd/MM/yyyy HH:mm");
        public string PeriodoExibicao
        {
            get
            {
                if (!DataInicio.HasValue && !DataFim.HasValue)
                    return "Sem restrição";
                
                var inicio = DataInicio?.ToString("dd/MM/yyyy") ?? "Início";
                var fim = DataFim?.ToString("dd/MM/yyyy") ?? "Indeterminado";
                
                return $"{inicio} - {fim}";
            }
        }
    }

    public enum TipoComunicado
    {
        [System.ComponentModel.Description("Comunicado")]
        Comunicado = 1,
        
        [System.ComponentModel.Description("Notícia")]
        Noticia = 2,
        
        [System.ComponentModel.Description("Aviso")]
        Aviso = 3,
        
        [System.ComponentModel.Description("Atualização")]
        Atualizacao = 4,
        
        [System.ComponentModel.Description("Evento")]
        Evento = 5
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (System.ComponentModel.DescriptionAttribute?)field?
                .GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                .FirstOrDefault();
            return attribute?.Description ?? value.ToString();
        }
    }
}
