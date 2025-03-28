using FreteManager.Models;

namespace FreteManager.Repositories
{
    public interface IClienteRepository
    {
        Task<Cliente> ObterPorIdAsync(int id);
        Task<IEnumerable<Cliente>> ListarTodosAsync();
        Task<Cliente> AdicionarAsync(Cliente cliente);
        Task AtualizarAsync(Cliente cliente);
        Task ExcluirAsync(int id);
        Task<Cliente> ObterPorEmailAsync(string email);
    }
}