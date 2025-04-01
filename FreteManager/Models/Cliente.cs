using System.ComponentModel.DataAnnotations;

namespace FreteManager.Models
{
    /// <summary>
    /// Modelo que representa um cliente no sistema
    /// </summary>
    public class Cliente
    {
        /// <summary>
        /// Identificador único do cliente
        /// </summary>
        /// <example>1</example>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo ou razão social do cliente
        /// </summary>
        /// <example>Empresa ABC Ltda</example>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; }

        /// <summary>
        /// Endereço completo do cliente
        /// </summary>
        /// <example>Av. Paulista, 1000, São Paulo, SP</example>
        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(200, ErrorMessage = "Endereço muito longo")]
        public string Endereco { get; set; }

        /// <summary>
        /// Telefone de contato do cliente
        /// </summary>
        /// <example>(11) 3456-7890</example>
        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; }

        /// <summary>
        /// Email principal do cliente
        /// </summary>
        /// <example>contato@empresaabc.com</example>
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }
}