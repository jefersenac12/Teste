using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Admin.Models
{
    public class AtividadeViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da atividade é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        // Propriedades de exibição (mantidas para compatibilidade)
        public decimal ValorPorHa { get => Valor; set => Valor = value; }
        public string ValorFormatado => Valor.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        public string ValorPorHaFormatado => ValorFormatado;
    }
}