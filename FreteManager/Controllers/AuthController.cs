using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("api/[controller]")]
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
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = await _authService.RegisterAsync(model);

                _logger.LogInformation($"Usuário registrado com sucesso: {model.Email}");

                return Ok(new { message = "Usuário registrado com sucesso!", userId = usuario.Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Falha ao registrar usuário: {model.Email}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao registrar usuário: {model.Email}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // POST: api/Auth/Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.LoginAsync(model);

                _logger.LogInformation($"Login bem-sucedido: {model.Email}");

                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Tentativa de login com email não encontrado: {model.Email}");
                return BadRequest("Credenciais inválidas");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Tentativa de login com senha incorreta: {model.Email}");
                return BadRequest("Credenciais inválidas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao fazer login: {model.Email}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}