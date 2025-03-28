using FreteManager.Models;

namespace FreteManager.Repositories
{
    public interface IPedidoRepository
    {
        Task<Pedido> ObterPorIdAsync(int id);
        Task<IEnumerable<Pedido>> ListarTodosAsync();
        Task<Pedido> AdicionarAsync(Pedido pedido);
        Task AtualizarAsync(Pedido pedido);
        Task ExcluirAsync(int id);
        Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId);
    }
}