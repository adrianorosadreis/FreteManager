using FreteManager.Models;

namespace FreteManager.Services
{
    public interface IPedidoService
    {
        Task<Pedido> ObterPorIdAsync(int id);
        Task<IEnumerable<Pedido>> ListarTodosAsync();
        Task<Pedido> CriarAsync(Pedido pedido);
        Task AtualizarAsync(Pedido pedido);
        Task ExcluirAsync(int id);
        Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId);
        Task<decimal> CalcularFreteAsync(string origem, string destino);
        Task<Pedido> AtualizarStatusAsync(int id, StatusPedido novoStatus);
    }
}