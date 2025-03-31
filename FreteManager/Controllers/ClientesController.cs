using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("v1/[controller]")]
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
            _logger.LogInformation("Requisição GET para listar todos os clientes");

            var clientes = await _clienteService.ListarTodosAsync();

            _logger.LogInformation($"Retornando {clientes.Count()} clientes");
            return Ok(clientes);
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            _logger.LogInformation($"Requisição GET para obter cliente ID {id}");

            var cliente = await _clienteService.ObterPorIdAsync(id);

            _logger.LogInformation($"Retornando dados do cliente ID {id}: {cliente.Nome}");
            return Ok(cliente);
        }

        // POST: api/clientes
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            _logger.LogInformation("Requisição POST para criar novo cliente");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido na criação de cliente");
                return BadRequest(ModelState);
            }

            var clienteCriado = await _clienteService.CadastrarAsync(cliente);

            _logger.LogInformation($"Cliente criado com ID {clienteCriado.Id}: {clienteCriado.Nome}");
            return CreatedAtAction(nameof(GetCliente), new { id = clienteCriado.Id }, clienteCriado);
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            _logger.LogInformation($"Requisição PUT para atualizar cliente ID {id}");

            if (id != cliente.Id)
            {
                _logger.LogWarning($"ID da rota ({id}) não corresponde ao ID no corpo da requisição ({cliente.Id})");
                return BadRequest("ID do cliente na rota não corresponde ao ID no corpo da requisição");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido na atualização de cliente");
                return BadRequest(ModelState);
            }

            await _clienteService.AtualizarAsync(cliente);

            _logger.LogInformation($"Cliente ID {id} atualizado com sucesso");
            return NoContent();
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            _logger.LogInformation($"Requisição DELETE para excluir cliente ID {id}");

            await _clienteService.ExcluirAsync(id);

            _logger.LogInformation($"Cliente ID {id} excluído com sucesso");
            return NoContent();
        }
    }
}