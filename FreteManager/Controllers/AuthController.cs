using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    /// <summary>
    /// Controller para gerenciar operações de autenticação e registro de usuários
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        
        /// <summary>
        /// Construtor do AuthController
        /// </summary>
        /// <param name="authService">Serviço de autenticação</param>
        /// <param name="logger">Serviço de log</param>
        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo usuário no sistema
        /// </summary>
        /// <param name="model">Dados de registro do usuário</param>
        /// <returns>Informações do usuário registrado</returns>
        /// <response code="200">Retorna quando o usuário é registrado com sucesso</response>
        /// <response code="400">Retorna quando o modelo é inválido ou o email já está em uso</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            _logger.LogInformation($"Requisição POST para registrar usuário: {model.Email}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido no registro de usuário");
                return BadRequest(ModelState);
            }

            var usuario = await _authService.RegisterAsync(model);

            _logger.LogInformation($"Usuário registrado com sucesso: {model.Email}, ID: {usuario.Id}");

            return Ok(new { message = "Usuário registrado com sucesso!", userId = usuario.Id });
        }

        /// <summary>
        /// Autentica um usuário e retorna um token JWT
        /// </summary>
        /// <param name="model">Credenciais de login do usuário</param>
        /// <returns>Token de autenticação e informações do usuário</returns>
        /// <response code="200">Retorna quando o login é bem-sucedido</response>
        /// <response code="400">Retorna quando o modelo é inválido</response>
        /// <response code="404">Retorna quando o usuário não é encontrado</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            _logger.LogInformation($"Requisição POST para login: {model.Email}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido no login");
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(model);

            _logger.LogInformation($"Login bem-sucedido: {model.Email}");

            return Ok(response);
        }
    }
}