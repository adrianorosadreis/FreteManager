using FreteManager.DTOs;
using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de pedidos
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        /// <summary>
        /// Construtor do PedidosController
        /// </summary>
        /// <param name="pedidoService">Serviço de pedidos</param>
        /// <param name="logger">Serviço de log</param>
        public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os pedidos cadastrados
        /// </summary>
        /// <returns>Lista de pedidos</returns>
        /// <response code="200">Retorna a lista de pedidos</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDTO>>> GetPedidos()
        {
            _logger.LogInformation("Requisição GET para listar todos os pedidos");

            var pedidos = await _pedidoService.ListarTodosAsync();

            _logger.LogInformation($"Retornando {pedidos.Count()} pedidos");
            return Ok(pedidos);
        }

        /// <summary>
        /// Obtém um pedido específico pelo ID
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <returns>Dados do pedido</returns>
        /// <response code="200">Retorna os dados do pedido</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o pedido não é encontrado</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoRespostaDTO>> GetPedido(int id)
        {
            _logger.LogInformation($"Requisição GET para obter pedido ID {id}");

            var pedido = await _pedidoService.ObterPorIdAsync(id);

            _logger.LogInformation($"Retornando dados do pedido ID {id}");
            return Ok(pedido);
        }

        /// <summary>
        /// Obtém todos os pedidos de um cliente específico
        /// </summary>
        /// <param name="clienteId">ID do cliente</param>
        /// <returns>Lista de pedidos do cliente</returns>
        /// <response code="200">Retorna a lista de pedidos do cliente</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o cliente não é encontrado</response>
        [HttpGet("cliente/{clienteId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDTO>>> GetPedidosPorCliente(int clienteId)
        {
            _logger.LogInformation($"Requisição GET para listar pedidos do cliente ID {clienteId}");

            var pedidos = await _pedidoService.ListarPorClienteAsync(clienteId);

            _logger.LogInformation($"Retornando {pedidos.Count()} pedidos do cliente ID {clienteId}");
            return Ok(pedidos);
        }

        /// <summary>
        /// Cria um novo pedido
        /// </summary>
        /// <param name="pedidoDTO">Dados do pedido a ser criado</param>
        /// <returns>Dados do pedido criado</returns>
        /// <response code="201">Retorna quando o pedido é criado com sucesso</response>
        /// <response code="400">Retorna quando o modelo é inválido</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o cliente não é encontrado</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoRespostaDTO>> PostPedido(CriarPedidoDTO pedidoDTO)
        {
            _logger.LogInformation("Requisição POST para criar novo pedido");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido na criação de pedido");
                return BadRequest(ModelState);
            }

            var pedidoCriado = await _pedidoService.CriarAsync(pedidoDTO);

            _logger.LogInformation($"Pedido criado com ID {pedidoCriado.Id}");
            return CreatedAtAction(nameof(GetPedido), new { id = pedidoCriado.Id }, pedidoCriado);
        }

        /// <summary>
        /// Atualiza os dados de um pedido existente
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <param name="pedidoDTO">Novos dados do pedido</param>
        /// <returns>Sem conteúdo em caso de sucesso</returns>
        /// <response code="204">Retorna quando o pedido é atualizado com sucesso</response>
        /// <response code="400">Retorna quando o modelo é inválido ou o ID não corresponde</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o pedido ou cliente não é encontrado</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPedido(int id, AtualizarPedidoDTO pedidoDTO)
        {
            _logger.LogInformation($"Requisição PUT para atualizar pedido ID {id}");

            if (id != pedidoDTO.Id)
            {
                _logger.LogWarning($"ID da rota ({id}) não corresponde ao ID no corpo da requisição ({pedidoDTO.Id})");
                return BadRequest("ID do pedido na rota não corresponde ao ID no corpo da requisição");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido na atualização de pedido");
                return BadRequest(ModelState);
            }

            await _pedidoService.AtualizarAsync(pedidoDTO);

            _logger.LogInformation($"Pedido ID {id} atualizado com sucesso");
            return NoContent();
        }

        /// <summary>
        /// Exclui um pedido
        /// </summary>
        /// <param name="id">ID do pedido a ser excluído</param>
        /// <returns>Sem conteúdo em caso de sucesso</returns>
        /// <response code="204">Retorna quando o pedido é excluído com sucesso</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o pedido não é encontrado</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePedido(int id)
        {
            _logger.LogInformation($"Requisição DELETE para excluir pedido ID {id}");

            await _pedidoService.ExcluirAsync(id);

            _logger.LogInformation($"Pedido ID {id} excluído com sucesso");
            return NoContent();
        }

        /// <summary>
        /// Atualiza o status de um pedido
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <param name="model">Novo status do pedido</param>
        /// <returns>Dados atualizados do pedido</returns>
        /// <response code="200">Retorna os dados atualizados do pedido</response>
        /// <response code="400">Retorna quando o modelo é inválido ou a transição de status não é permitida</response>
        /// <response code="401">Retorna quando o usuário não está autenticado</response>
        /// <response code="404">Retorna quando o pedido não é encontrado</response>
        [HttpPatch("{id}/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PedidoRespostaDTO>> AtualizarStatus(int id, [FromBody] StatusUpdateModel model)
        {
            _logger.LogInformation($"Requisição PATCH para atualizar status do pedido ID {id} para {model.NovoStatus}");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Modelo inválido na atualização de status");
                return BadRequest(ModelState);
            }

            var pedidoAtualizado = await _pedidoService.AtualizarStatusAsync(id, model.NovoStatus);

            _logger.LogInformation($"Status do pedido ID {id} atualizado para {model.NovoStatus}");
            return Ok(pedidoAtualizado);
        }
    }

    // Modelo para atualização de status
    public class StatusUpdateModel
    {
        /// <summary>
        /// O novo status a ser aplicado ao pedido
        /// </summary>
        public StatusPedido NovoStatus { get; set; }
    }
}