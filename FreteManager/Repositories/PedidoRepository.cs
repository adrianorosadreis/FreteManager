using FreteManager.Data;
using FreteManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> ObterPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> ListarTodosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .ToListAsync();
        }

        public async Task<Pedido> AdicionarAsync(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task AtualizarAsync(Pedido pedido)
        {
            // Obter o pedido existente que está sendo rastreado
            var pedidoExistente = await _context.Pedidos.FindAsync(pedido.Id);

            if (pedidoExistente == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {pedido.Id} não encontrado.");
            }

            // Atualizar as propriedades manualmente
            pedidoExistente.ClienteId = pedido.ClienteId;
            pedidoExistente.Origem = pedido.Origem;
            pedidoExistente.Destino = pedido.Destino;
            pedidoExistente.Status = pedido.Status;
            pedidoExistente.ValorFrete = pedido.ValorFrete;
            
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirAsync(int id)
        {
            var pedido = await ObterPorIdAsync(id);
            if (pedido != null)
            {
                _context.Pedidos.Remove(pedido);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }
    }
}