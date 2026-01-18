namespace Admin.Models
{
    public class AgendamentoViewModel
    {
        public int Id { get; set; }
        public string Atividade { get; set; }
        public string Safra { get; set; }
        public DateTime Data { get; set; }
        public TimeSpan Hora { get; set; }
        public int VagasTotais { get; set; }
        public int VagasDisponiveis { get; set; }
    }
}
