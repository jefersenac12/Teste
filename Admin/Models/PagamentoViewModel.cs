using System.ComponentModel.DataAnnotations;

using System.Globalization;



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

        public string Metodo { get; set; } = string.Empty;



        [Required(ErrorMessage = "O status é obrigatório")]

        public string Status { get; set; } = string.Empty;



        public DateTime? DataPagamento { get; set; }

        public string? ChavePix { get; set; }

        public string? QrCode { get; set; }



        // Propriedades de navegação (para compatibilidade com views existentes)

        public string? ReservaDescricao { get; set; }

        public string? ClienteNome { get; set; }

        public string DataPagamentoFormatada => DataPagamento.HasValue ? DataPagamento.Value.ToString("dd/MM/yyyy HH:mm") : "-";

        public string ValorFormatado => Valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));

        public string StatusFormatado => Status?.ToUpper() ?? "";

        // Propriedades de status para compatibilidade
        public bool EstaPago => Status?.ToLower() == "pago";

        public bool EstaPendente => Status?.ToLower() == "pendente";

        public bool EstaCancelado => Status?.ToLower() == "cancelado";

    }

}
