using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Admin.Models
{
    public class ReservaViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string Safra { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DataReserva { get; set; } = DateTime.Today;

        [Range(0, int.MaxValue)]
        public int QtdInteira { get; set; }

        [Range(0, int.MaxValue)]
        public int QtdMeia { get; set; }

        [DataType(DataType.Currency)]
        public decimal ValorTotal { get; set; }

        // Propriedade auxiliar para exibir "2 (1 inteira, 1 meia)"
        public string QuantidadeDescricao
        {
            get
            {
                var total = QtdInteira + QtdMeia;
                if (total == 0) return "0";
                if (QtdMeia > 0)
                    return $"{total} ({QtdInteira} inteira, {QtdMeia} meia)";
                return $"{total} ({QtdInteira} inteira)";
            }
        }

        // Formatação de moeda pt-BR
        public string ValorTotalFormatado => ValorTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
    }
}
