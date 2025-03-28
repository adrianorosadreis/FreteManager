using FreteManager.Models;

namespace FreteManager.Services
{
    public interface IClienteService
    {
        Task<Cliente> ObterPorIdAsync(int id);
        Task<IEnumerable<Cliente>> ListarTodosAsync();
        Task<Cliente> CadastrarAsync(Cliente cliente);
        Task AtualizarAsync(Cliente cliente);
        Task ExcluirAsync(int id);
        Task<bool> ClienteExisteAsync(int id);
        Task<bool> EmailExisteAsync(string email);
    }
}