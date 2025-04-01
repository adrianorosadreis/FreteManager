using System.ComponentModel.DataAnnotations;

namespace FreteManager.Models
{
    /// <summary>
    /// Modelo que representa um usuário no sistema
    /// </summary>
    public class Usuario
    {
        /// <summary>
        /// Identificador único do usuário
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        /// <example>João Silva</example>
        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        /// <summary>
        /// Email do usuário (usado para login)
        /// </summary>
        /// <example>joao.silva@exemplo.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário (armazenada como hash)
        /// </summary>
        [Required]
        public string Senha { get; set; }

        /// <summary>
        /// Papel/função do usuário no sistema
        /// </summary>
        /// <example>Usuario</example>
        public string Role { get; set; } = "Usuario"; // Perfil padrão
    }

    /// <summary>
    /// Modelo para requisição de login
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email do usuário
        /// </summary>
        /// <example>usuario@exemplo.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário
        /// </summary>
        /// <example>Senha123!</example>
        [Required]
        public string Senha { get; set; }
    }

    /// <summary>
    /// Modelo para resposta de login bem-sucedido
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Token JWT para autenticação
        /// </summary>
        /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
        public string Token { get; set; }

        /// <summary>
        /// Nome do usuário
        /// </summary>
        /// <example>João Silva</example>
        public string Nome { get; set; }

        /// <summary>
        /// Email do usuário
        /// </summary>
        /// <example>joao.silva@exemplo.com</example>
        public string Email { get; set; }

        /// <summary>
        /// Data e hora de expiração do token
        /// </summary>
        /// <example>2025-04-01T14:30:00</example>
        public DateTime Expiracao { get; set; }
    }

    /// <summary>
    /// Modelo para requisição de registro de novo usuário
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        /// <example>Maria Souza</example>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Nome { get; set; }

        /// <summary>
        /// Email do usuário (será usado para login)
        /// </summary>
        /// <example>maria.souza@exemplo.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Senha do usuário
        /// </summary>
        /// <example>Senha123!</example>
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Senha { get; set; }

        /// <summary>
        /// Confirmação da senha (deve ser igual à senha)
        /// </summary>
        /// <example>Senha123!</example>
        [Compare("Senha", ErrorMessage = "As senhas não coincidem.")]
        public string ConfirmacaoSenha { get; set; }
    }
}