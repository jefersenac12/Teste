using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Admin.Models
{
    public class AtividadeViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal ValorPorHa { get; set; }

        // Helper para exibir valor formatado em pt-BR
        public string ValorPorHaFormatado => ValorPorHa.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
    }
}