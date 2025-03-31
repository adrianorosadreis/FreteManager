using FreteManager.Data;
using FreteManager.Exceptions;
using FreteManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace FreteManager.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PedidoRepository> _logger;

        public PedidoRepository(ApplicationDbContext context, ILogger<PedidoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Pedido> ObterPorIdAsync(int id)
        {
            _logger.LogDebug($"Buscando pedido com ID {id} no banco de dados");

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Pacotes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                _logger.LogWarning($"Pedido com ID {id} não encontrado");
                throw new EntityNotFoundException("Pedido", id);
            }

            _logger.LogDebug($"Pedido com ID {id} recuperado com sucesso");
            return pedido;
        }

        public async Task<IEnumerable<Pedido>> ListarTodosAsync()
        {
            _logger.LogDebug("Buscando todos os pedidos no banco de dados");

            try
            {
                // Usar uma projeção explícita para controlar os campos carregados
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Pacotes)
                    .ToListAsync();

                _logger.LogInformation($"Recuperados {pedidos.Count} pedidos do banco de dados");
                return pedidos;
            }
            catch (SqlNullValueException ex)
            {
                _logger.LogError(ex, "Erro ao converter valores nulos na listagem de pedidos");
                throw new DataIntegrityException("Erro ao processar dados de pedidos do banco de dados", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todos os pedidos");
                throw;
            }
        }

        public async Task<Pedido> AdicionarAsync(Pedido pedido)
        {
            _logger.LogDebug($"Iniciando adição de novo pedido para cliente ID {pedido.ClienteId}");

            try
            {
                await _context.Pedidos.AddAsync(pedido);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Pedido adicionado com sucesso. ID gerado: {pedido.Id}");
                return pedido;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao adicionar pedido para cliente ID {pedido.ClienteId}");
                throw new DataIntegrityException("Erro ao salvar o pedido no banco de dados", ex);
            }
        }

        public async Task AtualizarAsync(Pedido pedido)
        {
            _logger.LogDebug($"Iniciando atualização do pedido ID {pedido.Id}");

            // Obter o pedido existente que está sendo rastreado
            var pedidoExistente = await _context.Pedidos.FindAsync(pedido.Id);

            if (pedidoExistente == null)
            {
                _logger.LogWarning($"Tentativa de atualizar pedido inexistente. ID: {pedido.Id}");
                throw new EntityNotFoundException("Pedido", pedido.Id);
            }

            // Atualizar as propriedades manualmente
            pedidoExistente.ClienteId = pedido.ClienteId;
            pedidoExistente.Origem = pedido.Origem;
            pedidoExistente.Destino = pedido.Destino;
            pedidoExistente.Status = pedido.Status;
            pedidoExistente.ValorFrete = pedido.ValorFrete;
            pedidoExistente.ValorDeclarado = pedido.ValorDeclarado;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Pedido ID {pedido.Id} atualizado com sucesso");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar pedido ID {pedido.Id}");
                throw new DataIntegrityException("Erro ao salvar as alterações do pedido", ex);
            }
        }

        public async Task ExcluirAsync(int id)
        {
            _logger.LogDebug($"Iniciando exclusão do pedido ID {id}");

            var pedido = await ObterPorIdAsync(id);
            // ObterPorIdAsync já lança EntityNotFoundException se não encontrar

            try
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Pedido ID {id} excluído com sucesso");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao excluir pedido ID {id}");
                throw new DataIntegrityException($"Não foi possível excluir o pedido com ID {id}. " +
                    "Pode haver registros dependentes.", ex);
            }
        }

        public async Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId)
        {
            _logger.LogDebug($"Buscando pedidos do cliente ID {clienteId}");

            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.Pacotes)
                    .Where(p => p.ClienteId == clienteId)
                    .ToListAsync();

                _logger.LogInformation($"Recuperados {pedidos.Count} pedidos do cliente ID {clienteId}");
                return pedidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao listar pedidos do cliente ID {clienteId}");
                throw;
            }
        }
    }
}