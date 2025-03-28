using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        // GET: api/clientes
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            try
            {
                var clientes = await _clienteService.ListarTodosAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar clientes");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            try
            {
                var cliente = await _clienteService.ObterPorIdAsync(id);

                if (cliente == null)
                {
                    _logger.LogWarning($"Cliente com ID {id} não encontrado");
                    return NotFound($"Cliente com ID {id} não encontrado");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter cliente com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // POST: api/clientes
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var clienteCriado = await _clienteService.CadastrarAsync(cliente);

                _logger.LogInformation($"Cliente criado com ID {clienteCriado.Id}");

                return CreatedAtAction(nameof(GetCliente), new { id = clienteCriado.Id }, clienteCriado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar cliente");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            try
            {
                if (id != cliente.Id)
                {
                    return BadRequest("ID do cliente na rota não corresponde ao ID no corpo da requisição");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _clienteService.AtualizarAsync(cliente);

                _logger.LogInformation($"Cliente com ID {id} atualizado");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Cliente com ID {id} não encontrado para atualização");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao atualizar cliente");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar cliente com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                await _clienteService.ExcluirAsync(id);

                _logger.LogInformation($"Cliente com ID {id} excluído");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Cliente com ID {id} não encontrado para exclusão");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao excluir cliente com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}