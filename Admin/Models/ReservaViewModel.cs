using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class ReservaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A agenda é obrigatória")]
        public int AgendaId { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "A data da reserva é obrigatória")]
        public DateTime DataReserva { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser pelo menos 1")]
        public int Quantidade { get; set; }

        public int NPEntrada { get; set; }
        public int MeiaEntrada { get; set; }
        public int InteiraEntrada { get; set; }

        [Required(ErrorMessage = "O valor total é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser maior que zero")]
        public decimal ValorTotal { get; set; }

        // Propriedades de navegação/exibição
        public string? AgendaDescricao { get; set; }
        public string? UsuarioNome { get; set; }
        public string? UsuarioTipo { get; set; }

        // Propriedades calculadas
        public string DataReservaFormatada => DataReserva.ToString("dd/MM/yyyy");
        public string ValorTotalFormatado => ValorTotal.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        public string QuantidadeDescricao
        {
            get
            {
                if (Quantidade <= 0) return "0";
                if (MeiaEntrada > 0 && InteiraEntrada > 0)
                    return $"{Quantidade} ({InteiraEntrada} inteira, {MeiaEntrada} meia)";
                if (MeiaEntrada > 0)
                    return $"{Quantidade} ({MeiaEntrada} meia)";
                return $"{Quantidade} ({InteiraEntrada} inteira)";
            }
        }
    }
}
