using FreteManager.Data;
using FreteManager.Exceptions;
using FreteManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FreteManager.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClienteRepository> _logger;

        public ClienteRepository(ApplicationDbContext context, ILogger<ClienteRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Cliente> ObterPorIdAsync(int id)
        {
            _logger.LogDebug($"Buscando cliente com ID {id}");

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                _logger.LogWarning($"Cliente com ID {id} não encontrado");
                throw new EntityNotFoundException("Cliente", id);
            }

            _logger.LogDebug($"Cliente ID {id} recuperado com sucesso");
            return cliente;
        }

        public async Task<IEnumerable<Cliente>> ListarTodosAsync()
        {
            _logger.LogDebug("Buscando todos os clientes");

            try
            {
                var clientes = await _context.Clientes.ToListAsync();
                _logger.LogInformation($"Recuperados {clientes.Count} clientes");
                return clientes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todos os clientes");
                throw;
            }
        }

        public async Task<Cliente> AdicionarAsync(Cliente cliente)
        {
            _logger.LogDebug($"Iniciando adição de novo cliente: {cliente.Nome}");

            try
            {
                await _context.Clientes.AddAsync(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente {cliente.Nome} adicionado com sucesso. ID gerado: {cliente.Id}");
                return cliente;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao adicionar cliente: {cliente.Nome}");

                if (ex.InnerException?.Message.Contains("IX_Clientes_Email") == true)
                {
                    throw new DataIntegrityException("Este e-mail já está sendo usado por outro cliente", ex);
                }

                throw new DataIntegrityException("Erro ao salvar o cliente no banco de dados", ex);
            }
        }

        public async Task AtualizarAsync(Cliente cliente)
        {
            _logger.LogDebug($"Iniciando atualização do cliente ID {cliente.Id}");

            var clienteExistente = await _context.Clientes.FindAsync(cliente.Id);

            if (clienteExistente == null)
            {
                _logger.LogWarning($"Tentativa de atualizar cliente inexistente. ID: {cliente.Id}");
                throw new EntityNotFoundException("Cliente", cliente.Id);
            }

            clienteExistente.Nome = cliente.Nome;
            clienteExistente.Email = cliente.Email;
            clienteExistente.Endereco = cliente.Endereco;
            clienteExistente.Telefone = cliente.Telefone;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cliente ID {cliente.Id} atualizado com sucesso");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar cliente ID {cliente.Id}");

                if (ex.InnerException?.Message.Contains("IX_Clientes_Email") == true)
                {
                    throw new DataIntegrityException("Este e-mail já está sendo usado por outro cliente", ex);
                }

                throw new DataIntegrityException("Erro ao salvar as alterações do cliente", ex);
            }
        }

        public async Task ExcluirAsync(int id)
        {
            _logger.LogDebug($"Iniciando exclusão do cliente ID {id}");

            var cliente = await ObterPorIdAsync(id);
            // ObterPorIdAsync já lança EntityNotFoundException se não encontrar

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Cliente ID {id} excluído com sucesso");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Erro ao excluir cliente ID {id}");

                if (ex.InnerException?.Message.Contains("FK_") == true)
                {
                    throw new DataIntegrityException(
                        $"Não é possível excluir o cliente ID {id} pois existem pedidos associados a ele", ex);
                }

                throw new DataIntegrityException($"Não foi possível excluir o cliente ID {id}", ex);
            }
        }

        public async Task<Cliente> ObterPorEmailAsync(string email)
        {
            _logger.LogDebug($"Buscando cliente pelo email: {email}");

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

            if (cliente != null)
            {
                _logger.LogDebug($"Cliente encontrado com o email {email}. ID: {cliente.Id}");
            }
            else
            {
                _logger.LogDebug($"Nenhum cliente encontrado com o email: {email}");
            }

            return cliente;
        }
    }
}