using FreteManager.DTOs;
using FreteManager.Models;

namespace FreteManager.Services
{
    public interface IPedidoService
    {
        Task<PedidoRespostaDTO> ObterPorIdAsync(int id);
        Task<IEnumerable<PedidoRespostaDTO>> ListarTodosAsync();
        Task<PedidoRespostaDTO> CriarAsync(CriarPedidoDTO pedidoDTO);
        Task<PedidoRespostaDTO> AtualizarAsync(AtualizarPedidoDTO pedidoDTO);
        Task ExcluirAsync(int id);
        Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId);
        Task<decimal> CalcularFreteParaPedidoAsync(Pedido pedido);
        Task<Pedido> AtualizarStatusAsync(int id, StatusPedido novoStatus);
    }
}