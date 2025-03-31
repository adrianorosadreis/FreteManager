using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using FreteManager.Data;
using FreteManager.Models;
using FreteManager.Repositories;
using FreteManager.Exceptions;

namespace FreteManager.Tests.Repositories
{
    public class ClienteRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClienteRepository _repository;
        private readonly Mock<ILogger<ClienteRepository>> _mockLogger;

        public ClienteRepositoryTests()
        {
            // Configurar banco de dados em memória
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<ClienteRepository>>();
            _repository = new ClienteRepository(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task AdicionarAsync_NovoCliente_DeveInserirComSucesso()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "João Silva",
                Email = "joao.silva@teste.com",
                Telefone = "1234567890",
                Endereco = "Rua Teste, 123"
            };

            // Act
            var clienteInserido = await _repository.AdicionarAsync(cliente);

            // Assert
            Assert.NotNull(clienteInserido);
            Assert.True(clienteInserido.Id > 0);
            Assert.Equal(cliente.Nome, clienteInserido.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_ClienteExistente_DeveRetornarCliente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Maria Souza",
                Email = "maria.souza@teste.com",
                Telefone = "0987654321",
                Endereco = "Av. Teste, 456"
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            // Act
            var clienteEncontrado = await _repository.ObterPorIdAsync(cliente.Id);

            // Assert
            Assert.NotNull(clienteEncontrado);
            Assert.Equal(cliente.Nome, clienteEncontrado.Nome);
        }

        [Fact]
        public async Task ObterPorIdAsync_ClienteInexistente_DeveLancarExcecao()
        {
            // Act & Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _repository.ObterPorIdAsync(999)
            );
        }

        [Fact]
        public async Task AtualizarAsync_ClienteExistente_DeveAtualizarCorretamente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Cliente Original",
                Email = "original@teste.com",
                Telefone = "1111111111",
                Endereco = "Rua Original, 100"
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            // Act
            cliente.Nome = "Cliente Atualizado";
            cliente.Telefone = "2222222222";
            await _repository.AtualizarAsync(cliente);

            // Recarregar do banco para verificar
            var clienteAtualizado = await _repository.ObterPorIdAsync(cliente.Id);

            // Assert
            Assert.Equal("Cliente Atualizado", clienteAtualizado.Nome);
            Assert.Equal("2222222222", clienteAtualizado.Telefone);
        }

        [Fact]
        public async Task ExcluirAsync_ClienteExistente_DeveRemoverCliente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Cliente para Exclusão",
                Email = "exclusao@teste.com",
                Telefone = "3333333333",
                Endereco = "Rua Exclusão, 200"
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            // Act
            await _repository.ExcluirAsync(cliente.Id);

            // Assert
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _repository.ObterPorIdAsync(cliente.Id)
            );
        }

        [Fact]
        public async Task ObterPorEmailAsync_EmailExistente_DeveRetornarCliente()
        {
            // Arrange
            var cliente = new Cliente
            {
                Nome = "Cliente Email",
                Email = "email.teste@teste.com",
                Telefone = "4444444444",
                Endereco = "Rua Email, 300"
            };
            await _context.Clientes.AddAsync(cliente);
            await _context.SaveChangesAsync();

            // Act
            var clienteEncontrado = await _repository.ObterPorEmailAsync("email.teste@teste.com");

            // Assert
            Assert.NotNull(clienteEncontrado);
            Assert.Equal(cliente.Nome, clienteEncontrado.Nome);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}