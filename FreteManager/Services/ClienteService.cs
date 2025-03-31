using FreteManager.Exceptions;
using FreteManager.Models;
using FreteManager.Repositories;

namespace FreteManager.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(IClienteRepository clienteRepository, ILogger<ClienteService> logger)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<Cliente> ObterPorIdAsync(int id)
        {
            _logger.LogInformation($"Obtendo cliente com ID {id}");

            // O repositório já lançará EntityNotFoundException se não encontrar
            var cliente = await _clienteRepository.ObterPorIdAsync(id);

            _logger.LogDebug($"Cliente com ID {id} encontrado: {cliente.Nome}");
            return cliente;
        }

        public async Task<IEnumerable<Cliente>> ListarTodosAsync()
        {
            _logger.LogInformation("Listando todos os clientes");

            var clientes = await _clienteRepository.ListarTodosAsync();

            _logger.LogDebug($"Recuperados {clientes.Count()} clientes");
            return clientes;
        }

        public async Task<Cliente> CadastrarAsync(Cliente cliente)
        {
            _logger.LogInformation($"Iniciando cadastro de novo cliente: {cliente.Nome}");

            // Verificar se o e-mail já existe
            var emailExiste = await EmailExisteAsync(cliente.Email);
            if (emailExiste)
            {
                _logger.LogWarning($"Tentativa de cadastro com e-mail já existente: {cliente.Email}");
                throw new BusinessRuleViolationException("Este e-mail já está cadastrado.");
            }

            var clienteCadastrado = await _clienteRepository.AdicionarAsync(cliente);

            _logger.LogInformation($"Cliente cadastrado com sucesso. ID: {clienteCadastrado.Id}");
            return clienteCadastrado;
        }

        public async Task AtualizarAsync(Cliente cliente)
        {
            _logger.LogInformation($"Iniciando atualização do cliente ID {cliente.Id}");

            // Verificar se está tentando alterar para um e-mail que já existe
            var clienteComEmail = await _clienteRepository.ObterPorEmailAsync(cliente.Email);
            if (clienteComEmail != null && clienteComEmail.Id != cliente.Id)
            {
                _logger.LogWarning($"Tentativa de atualizar para e-mail já existente: {cliente.Email}");
                throw new BusinessRuleViolationException("Este e-mail já está sendo usado por outro cliente.");
            }

            // A verificação se o cliente existe já é feita no repositório
            await _clienteRepository.AtualizarAsync(cliente);

            _logger.LogInformation($"Cliente ID {cliente.Id} atualizado com sucesso");
        }

        public async Task ExcluirAsync(int id)
        {
            _logger.LogInformation($"Iniciando exclusão do cliente ID {id}");

            // A verificação se o cliente existe já é feita no repositório
            await _clienteRepository.ExcluirAsync(id);

            _logger.LogInformation($"Cliente ID {id} excluído com sucesso");
        }

        public async Task<bool> ClienteExisteAsync(int id)
        {
            _logger.LogDebug($"Verificando se cliente ID {id} existe");

            try
            {
                await _clienteRepository.ObterPorIdAsync(id);
                _logger.LogDebug($"Cliente ID {id} existe");
                return true;
            }
            catch (EntityNotFoundException)
            {
                _logger.LogDebug($"Cliente ID {id} não existe");
                return false;
            }
        }

        public async Task<bool> EmailExisteAsync(string email)
        {
            _logger.LogDebug($"Verificando se email {email} já está em uso");

            var cliente = await _clienteRepository.ObterPorEmailAsync(email);
            var existe = cliente != null;

            _logger.LogDebug(existe
                ? $"Email {email} já está em uso pelo cliente ID {cliente.Id}"
                : $"Email {email} não está em uso");

            return existe;
        }
    }
}