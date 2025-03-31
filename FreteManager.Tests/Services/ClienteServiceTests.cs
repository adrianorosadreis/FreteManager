using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FreteManager.Services;
using FreteManager.Repositories;
using FreteManager.Models;
using FreteManager.Exceptions;

namespace FreteManager.Tests.Services
{
    /// <summary>
    /// Testes unitários para o serviço de clientes
    /// </summary>
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _mockClienteRepository;
        private readonly Mock<ILogger<ClienteService>> _mockLogger;
        private readonly ClienteService _clienteService;

        public ClienteServiceTests()
        {
            // Configuração inicial para cada teste
            _mockClienteRepository = new Mock<IClienteRepository>();
            _mockLogger = new Mock<ILogger<ClienteService>>();
            _clienteService = new ClienteService(_mockClienteRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CadastrarAsync_NovoCliente_DeveSerCadastradoComSucesso()
        {
            // Arrange
            var novoCliente = new Cliente
            {
                Nome = "Cliente Teste",
                Email = "cliente@teste.com",
                Telefone = "1234567890",
                Endereco = "Rua Teste, 123"
            };

            // Configurar o mock para não encontrar um cliente com o mesmo email
            _mockClienteRepository
                .Setup(repo => repo.ObterPorEmailAsync(novoCliente.Email))
                .ReturnsAsync((Cliente)null);

            // Configurar o mock para adicionar o cliente
            _mockClienteRepository
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync(novoCliente);

            // Act
            var clienteCadastrado = await _clienteService.CadastrarAsync(novoCliente);

            // Assert
            Assert.NotNull(clienteCadastrado);
            Assert.Equal(novoCliente.Nome, clienteCadastrado.Nome);
            Assert.Equal(novoCliente.Email, clienteCadastrado.Email);

            // Verificar que o método de adicionar foi chamado
            _mockClienteRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task CadastrarAsync_EmailJaExistente_DeveLancarExcecao()
        {
            // Arrange
            var clienteExistente = new Cliente
            {
                Id = 1,
                Nome = "Cliente Existente",
                Email = "existente@teste.com"
            };

            var novoCliente = new Cliente
            {
                Nome = "Novo Cliente",
                Email = "existente@teste.com"
            };

            // Configurar o mock para encontrar um cliente com o mesmo email
            _mockClienteRepository
                .Setup(repo => repo.ObterPorEmailAsync(novoCliente.Email))
                .ReturnsAsync(clienteExistente);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _clienteService.CadastrarAsync(novoCliente)
            );

            // Verificar que o método de adicionar NÃO foi chamado
            _mockClienteRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_ClienteValido_DeveAtualizarComSucesso()
        {
            // Arrange
            var clienteExistente = new Cliente
            {
                Id = 1,
                Nome = "Cliente Original",
                Email = "original@teste.com",
                Telefone = "1111111111",
                Endereco = "Rua Original, 100"
            };

            var clienteAtualizado = new Cliente
            {
                Id = 1,
                Nome = "Cliente Atualizado",
                Email = "original@teste.com",
                Telefone = "2222222222",
                Endereco = "Rua Atualizada, 200"
            };

            // Configurar o mock para encontrar o cliente pelo email
            _mockClienteRepository
                .Setup(repo => repo.ObterPorEmailAsync(clienteAtualizado.Email))
                .ReturnsAsync(clienteExistente);

            // Configurar o mock para não lançar exceção na atualização
            _mockClienteRepository
                .Setup(repo => repo.AtualizarAsync(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);

            // Act
            await _clienteService.AtualizarAsync(clienteAtualizado);

            // Assert
            _mockClienteRepository.Verify(repo => repo.AtualizarAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_EmailJaUtilizadoPorOutroCliente_DeveLancarExcecao()
        {
            // Arrange
            var clienteExistente = new Cliente
            {
                Id = 1,
                Nome = "Cliente Existente",
                Email = "existente@teste.com"
            };

            var clienteParaAtualizar = new Cliente
            {
                Id = 2,
                Nome = "Novo Cliente",
                Email = "existente@teste.com"
            };

            // Configurar o mock para encontrar um cliente com o mesmo email
            _mockClienteRepository
                .Setup(repo => repo.ObterPorEmailAsync(clienteParaAtualizar.Email))
                .ReturnsAsync(clienteExistente);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _clienteService.AtualizarAsync(clienteParaAtualizar)
            );

            // Verificar que o método de atualizar NÃO foi chamado
            _mockClienteRepository.Verify(repo => repo.AtualizarAsync(It.IsAny<Cliente>()), Times.Never);
        }
    }
}
