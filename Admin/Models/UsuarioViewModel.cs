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
        public string Email { get; set; } = string.Empty;

        public string CNPJ { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
        public string Tipo { get; set; } = string.Empty; // "Agencia" ou "Familia"

        // Propriedades de exibição
        public string TipoFormatado => Tipo?.ToLower() == "agencia" ? "Agência" : "Família";
        public string Identificador => string.IsNullOrWhiteSpace(CNPJ) ? Telefone : CNPJ;
    }
}
