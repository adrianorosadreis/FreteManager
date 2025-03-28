using System.ComponentModel.DataAnnotations;

namespace FreteManager.Models
{
    public class Cliente
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        // Campo Nome - Obrigatório
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; }

        // Endereço - Obrigatório
        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(200, ErrorMessage = "Endereço muito longo")]
        public string Endereco { get; set; }

        // Telefone - Obrigatório com validação de formato
        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; }

        // Email - Obrigatório com validação de formato
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }
}