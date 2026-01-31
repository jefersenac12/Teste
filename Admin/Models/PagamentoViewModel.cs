using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class PagamentoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A reserva é obrigatória")]
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "O método de pagamento é obrigatório")]
        public string Metodo { get; set; } = string.Empty; // "Pix"

        [Required(ErrorMessage = "O status é obrigatório")]
        public string Status { get; set; } = string.Empty; // "Pago", "Pendente", "Cancelado"

        public DateTime? DataPagamento { get; set; }

        public string? ChavePix { get; set; }
        public string? QrCode { get; set; }

        // Propriedades de navegação/exibição
        public string? ReservaDescricao { get; set; }
        public string? ClienteNome { get; set; }

        // Propriedades calculadas
        public string DataPagamentoFormatada => DataPagamento.HasValue ? DataPagamento.Value.ToString("dd/MM/yyyy HH:mm") : "-";
        public string ValorFormatado => Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        public string StatusFormatado => Status?.ToUpper() ?? "";
        public bool EstaPago => Status?.ToLower() == "pago";
        public bool EstaPendente => Status?.ToLower() == "pendente";
        public bool EstaCancelado => Status?.ToLower() == "cancelado";
    }
}
