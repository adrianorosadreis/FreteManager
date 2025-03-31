using FreteManager.DTOs;
using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }

        // GET: api/pedidos
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDTO>>> GetPedidos()
        {
            _logger.LogInformation("Requisição GET para listar todos os pedidos");

            // Não precisamos de try-catch pois o middleware global tratará as exceções
            var pedidos = await _pedidoService.ListarTodosAsync();

            _logger.LogInformation($"Retornando {pedidos.Count()} pedidos");
            return Ok(pedidos);
        }

        // GET: api/pedidos/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PedidoRespostaDTO>> GetPedido(int id)
        {
            _logger.LogInformation($"Requisição GET para obter pedido ID {id}");

            var pedido = await _pedidoService.ObterPorIdAsync(id);

            _logger.LogInformation($"Retornando dados do pedido ID {id}");
            return Ok(pedido);
        }

        // GET: api/pedidos/cliente/5
        [HttpGet("cliente/{clienteId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PedidoRespostaDTO>>> GetPedidosPorCliente(int clienteId)
        {
            _logger.LogInformation($"Requisição GET para listar pedidos do cliente ID {clienteId}");

            var pedidos = await _pedidoService.ListarPorClienteAsync(clienteId);

            _logger.LogInformation($"Retornando {pedidos.Count()} pedidos do cliente ID {clienteId}");
            return Ok(pedidos);
        }

        // POST: api/pedidos
        [HttpPost]
        [Authorize]
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

        // PUT: api/pedidos/5
        [HttpPut("{id}")]
        [Authorize]
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

        // DELETE: api/pedidos/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePedido(int id)
        {
            _logger.LogInformation($"Requisição DELETE para excluir pedido ID {id}");

            await _pedidoService.ExcluirAsync(id);

            _logger.LogInformation($"Pedido ID {id} excluído com sucesso");
            return NoContent();
        }

        // PATCH: api/pedidos/5/status
        [HttpPatch("{id}/status")]
        [Authorize]
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
        public StatusPedido NovoStatus { get; set; }
    }
}