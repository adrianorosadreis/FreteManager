using FreteManager.Models;
using FreteManager.Repositories;

namespace FreteManager.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<Cliente> ObterPorIdAsync(int id)
        {
            return await _clienteRepository.ObterPorIdAsync(id);
        }

        public async Task<IEnumerable<Cliente>> ListarTodosAsync()
        {
            return await _clienteRepository.ListarTodosAsync();
        }

        public async Task<Cliente> CadastrarAsync(Cliente cliente)
        {
            // Verificar se o e-mail já existe
            var clienteExistente = await _clienteRepository.ObterPorEmailAsync(cliente.Email);
            if (clienteExistente != null)
            {
                throw new InvalidOperationException("Este e-mail já está cadastrado.");
            }

            return await _clienteRepository.AdicionarAsync(cliente);
        }

        public async Task AtualizarAsync(Cliente cliente)
        {
            // Verificar se o cliente existe
            var clienteExistente = await _clienteRepository.ObterPorIdAsync(cliente.Id);
            if (clienteExistente == null)
            {
                throw new KeyNotFoundException("Cliente não encontrado.");
            }

            // Verificar se está tentando alterar para um e-mail que já existe
            var clienteComEmail = await _clienteRepository.ObterPorEmailAsync(cliente.Email);
            if (clienteComEmail != null && clienteComEmail.Id != cliente.Id)
            {
                throw new InvalidOperationException("Este e-mail já está sendo usado por outro cliente.");
            }

            await _clienteRepository.AtualizarAsync(cliente);
        }

        public async Task ExcluirAsync(int id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            if (cliente == null)
            {
                throw new KeyNotFoundException("Cliente não encontrado.");
            }

            await _clienteRepository.ExcluirAsync(id);
        }

        public async Task<bool> ClienteExisteAsync(int id)
        {
            var cliente = await _clienteRepository.ObterPorIdAsync(id);
            return cliente != null;
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            var cliente = await _clienteRepository.ObterPorEmailAsync(email);
            return cliente != null;
        }
    }
}