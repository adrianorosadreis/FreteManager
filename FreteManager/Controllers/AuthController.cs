using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        // POST: api/Auth/Register
        [HttpPost("register")]
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

        // POST: api/Auth/Login
        [HttpPost("login")]
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