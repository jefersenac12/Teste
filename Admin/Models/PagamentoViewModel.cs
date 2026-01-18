namespace Admin.Models
{
    public class PagamentoViewModel
    {
        public int Id { get; set; }
        public string IdReserva { get; set; }
        public string Cliente { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } // Pago, Pendente, Cancelado
        public DateTime Data { get; set; }
        public string NumeroTransacao { get; set; }
    }
}
