using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de clientes
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<ClientesController> _logger;

        /// <summary>
        /// Construtor do ClientesController
        /// </summary>
        /// <param name="clienteService">Serviço de clientes</param>
        /// <param name="logger">Serviço de log</param>
        public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os clientes cadastrados
        /// </summary>
        /// <returns>Lista de clientes</returns>
        /// <response code="200">Retorna a lista de clientes</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            _logger.LogInformation("Requisição GET para listar todos os clientes");

            var clientes = await _clienteService.ListarTodosAsync();

            _logger.LogInformation($"Retornando {clientes.Count()} clientes");
            return Ok(clientes);
        }

        /// <summary>
        /// Obtém um cliente específico pelo ID
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <returns>Dados do cliente</returns>
        /// <response code="200">Retorna os dados do cliente</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o cliente não é encontrado</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            _logger.LogInformation($"Requisição GET para obter cliente ID {id}");

            var cliente = await _clienteService.ObterPorIdAsync(id);

            _logger.LogInformation($"Retornando dados do cliente ID {id}: {cliente.Nome}");
            return Ok(cliente);
        }

        /// <summary>
        /// Cadastra um novo cliente
        /// </summary>
        /// <param name="cliente">Dados do cliente a ser cadastrado</param>
        /// <returns>Dados do cliente cadastrado</returns>
        /// <response code="201">Retorna quando o cliente é criado com sucesso</response>
        /// <response code="400">Retorna quando o modelo é inválido</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        /// <summary>
        /// Atualiza os dados de um cliente existente
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <param name="cliente">Novos dados do cliente</param>
        /// <returns>Sem conteúdo em caso de sucesso</returns>
        /// <response code="204">Retorna quando o cliente é atualizado com sucesso</response>
        /// <response code="400">Retorna quando o modelo é inválido ou o ID não corresponde</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o cliente não é encontrado</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Exclui um cliente
        /// </summary>
        /// <param name="id">ID do cliente a ser excluído</param>
        /// <returns>Sem conteúdo em caso de sucesso</returns>
        /// <response code="204">Retorna quando o cliente é excluído com sucesso</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o cliente não é encontrado</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            _logger.LogInformation($"Requisição DELETE para excluir cliente ID {id}");

            await _clienteService.ExcluirAsync(id);

            _logger.LogInformation($"Cliente ID {id} excluído com sucesso");
            return NoContent();
        }
    }
}