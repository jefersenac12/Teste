using System.ComponentModel.DataAnnotations;

namespace Admin.Models
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O telefone é obrigatório")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        public string Telefone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Formato de e-mail inválido")]
        public string? Email { get; set; }

        public string? CNPJ { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
        public byte Tipo { get; set; } // 1=Família, 2=Agência

        // Propriedades de exibição
        public string TipoFormatado => Tipo == 1 ? "Família" : "Agência";
        public string Identificador => string.IsNullOrWhiteSpace(CNPJ) ? Telefone : CNPJ;
    }
}
