using FreteManager.Models;
using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreteManager.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            try
            {
                var pedidos = await _pedidoService.ListarTodosAsync();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pedidos");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // GET: api/pedidos/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            try
            {
                var pedido = await _pedidoService.ObterPorIdAsync(id);
                return Ok(pedido);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Pedido com ID {id} não encontrado");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao obter pedido com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // GET: api/pedidos/cliente/5
        [HttpGet("cliente/{clienteId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidosPorCliente(int clienteId)
        {
            try
            {
                var pedidos = await _pedidoService.ListarPorClienteAsync(clienteId);
                return Ok(pedidos);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Cliente com ID {clienteId} não encontrado");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao listar pedidos do cliente com ID {clienteId}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // POST: api/pedidos
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pedidoCriado = await _pedidoService.CriarAsync(pedido);

                _logger.LogInformation($"Pedido criado com ID {pedidoCriado.Id}");

                return CreatedAtAction(nameof(GetPedido), new { id = pedidoCriado.Id }, pedidoCriado);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Cliente não encontrado ao criar pedido");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // PUT: api/pedidos/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            try
            {
                if (id != pedido.Id)
                {
                    return BadRequest("ID do pedido na rota não corresponde ao ID no corpo da requisição");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _pedidoService.AtualizarAsync(pedido);

                _logger.LogInformation($"Pedido com ID {id} atualizado");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Pedido ou Cliente não encontrado para atualização");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar pedido com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // DELETE: api/pedidos/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePedido(int id)
        {
            try
            {
                await _pedidoService.ExcluirAsync(id);

                _logger.LogInformation($"Pedido com ID {id} excluído");

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Pedido com ID {id} não encontrado para exclusão");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao excluir pedido com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // PATCH: api/pedidos/5/status
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<ActionResult<Pedido>> AtualizarStatus(int id, [FromBody] StatusUpdateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pedidoAtualizado = await _pedidoService.AtualizarStatusAsync(id, model.NovoStatus);

                _logger.LogInformation($"Status do pedido com ID {id} atualizado para {model.NovoStatus}");

                return Ok(pedidoAtualizado);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Pedido com ID {id} não encontrado para atualização de status");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Transição de status inválida para o pedido com ID {id}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar status do pedido com ID {id}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // GET: api/pedidos/calcular-frete
        [HttpGet("calcular-frete")]
        [Authorize]
        public async Task<ActionResult<decimal>> CalcularFrete([FromQuery] string origem, [FromQuery] string destino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(destino))
                {
                    return BadRequest("Origem e destino são obrigatórios");
                }

                var valorFrete = await _pedidoService.CalcularFreteAsync(origem, destino);

                _logger.LogInformation($"Frete calculado de {origem} para {destino}: {valorFrete:C}");

                return Ok(valorFrete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao calcular frete de {origem} para {destino}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }

    // Modelo para atualização de status
    public class StatusUpdateModel
    {
        public StatusPedido NovoStatus { get; set; }
    }
}