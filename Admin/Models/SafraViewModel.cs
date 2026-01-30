using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class SafraViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }
    }
}