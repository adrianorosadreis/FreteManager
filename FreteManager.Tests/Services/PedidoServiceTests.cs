using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using FreteManager.Services;
using FreteManager.Repositories;
using FreteManager.Models;
using FreteManager.DTOs;
using FreteManager.Exceptions;
using FreteManager.Helpers;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Tests.Services
{
    /// <summary>
    /// Testes unitários para o serviço de pedidos
    /// </summary>
    public class PedidoServiceTests
    {
        private readonly Mock<IPedidoRepository> _mockPedidoRepository;
        private readonly Mock<IClienteService> _mockClienteService;
        private readonly Mock<IFreteService> _mockFreteService;
        private readonly Mock<ILogger<PedidoService>> _mockLogger;
        private readonly PedidoService _pedidoService;

        public PedidoServiceTests()
        {
            // Configuração inicial para cada teste
            _mockPedidoRepository = new Mock<IPedidoRepository>();
            _mockClienteService = new Mock<IClienteService>();
            _mockFreteService = new Mock<IFreteService>();
            _mockLogger = new Mock<ILogger<PedidoService>>();

            _pedidoService = new PedidoService(
                _mockPedidoRepository.Object,
                _mockClienteService.Object,
                _mockFreteService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CriarAsync_PedidoValido_DeveCriarComSucesso()
        {
            // Arrange
            var clienteId = 1;
            var criarPedidoDto = new CriarPedidoDTO
            {
                ClienteId = clienteId,
                Origem = "12345678",
                Destino = "87654321",
                Pacotes = new List<PacoteDTO>
                {
                    new PacoteDTO
                    {
                        Altura = 10,
                        Largura = 15,
                        Comprimento = 20,
                        Peso = 1.5m,
                        Quantidade = 1
                    }
                }
            };

            // Configurar mock do cliente service
            _mockClienteService
                .Setup(service => service.ClienteExisteAsync(clienteId))
                .ReturnsAsync(true);

            // Configurar mock do frete service
            _mockFreteService
                .Setup(service => service.CalcularFreteDetalhadoAsync(It.IsAny<ParametrosFrete>()))
                .ReturnsAsync(50.00m);

            // Configurar mock do repositório
            _mockPedidoRepository
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Pedido>()))
                .Returns<Pedido>(pedido =>
                {
                    pedido.Id = 1; // Simular atribuição de ID
                    return Task.FromResult(pedido);
                });

            // Act
            var pedidoResposta = await _pedidoService.CriarAsync(criarPedidoDto);

            // Assert
            Assert.NotNull(pedidoResposta);
            Assert.Equal(1, pedidoResposta.Id);
            Assert.Equal(50.00m, pedidoResposta.ValorFrete);
            Assert.Equal(StatusPedido.EmProcessamento, pedidoResposta.Status);

            // Verificar chamadas
            _mockClienteService.Verify(service => service.ClienteExisteAsync(clienteId), Times.Once);
            _mockFreteService.Verify(service => service.CalcularFreteDetalhadoAsync(It.IsAny<ParametrosFrete>()), Times.Once);
            _mockPedidoRepository.Verify(repo => repo.AdicionarAsync(It.IsAny<Pedido>()), Times.Once);
        }

        public async Task AtualizarStatusAsync_TransicaoValidaEntrePedidosPossiveis_DeveAtualizarStatus()
        {
            // Arrange
            var pedidoId = 1;
            var pedidoExistente = new Pedido
            {
                Id = pedidoId,
                Status = StatusPedido.EmProcessamento
            };

            // Configurar mock do repositório para retornar o pedido existente
            _mockPedidoRepository
                .Setup(repo => repo.ObterPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoExistente);

            // Configurar mock do repositório para atualizar
            _mockPedidoRepository
                .Setup(repo => repo.AtualizarAsync(It.IsAny<Pedido>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _pedidoService.AtualizarStatusAsync(pedidoId, StatusPedido.Enviado);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(StatusPedido.Enviado, resultado.Status);
            _mockPedidoRepository.Verify(repo => repo.AtualizarAsync(It.IsAny<Pedido>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarStatusAsync_TransicaoInvalida_DeveLancarExcecao()
        {
            // Arrange
            var pedidoId = 1;
            var pedidoExistente = new Pedido
            {
                Id = pedidoId,
                Status = StatusPedido.Entregue
            };

            // Configurar mock do repositório para retornar o pedido existente
            _mockPedidoRepository
                .Setup(repo => repo.ObterPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoExistente);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleViolationException>(
                () => _pedidoService.AtualizarStatusAsync(pedidoId, StatusPedido.Enviado)
            );
        }

        [Fact]
        public async Task CalcularFreteParaPedidoAsync_PedidoSemPacotes_DeveUsarPacotePadrao()
        {
            // Arrange
            var pedido = new Pedido
            {
                Id = 1,
                Origem = "12345678",
                Destino = "87654321",
                Pacotes = new List<Pacote>() // Lista vazia
            };

            // Configurar mock do frete service
            _mockFreteService
                .Setup(service => service.CalcularFreteDetalhadoAsync(It.IsAny<ParametrosFrete>()))
                .ReturnsAsync(45.00m);

            // Act
            var valorFrete = await _pedidoService.CalcularFreteParaPedidoAsync(pedido);

            // Assert
            Assert.Equal(45.00m, valorFrete);
            _mockFreteService.Verify(
                service => service.CalcularFreteDetalhadoAsync(It.Is<ParametrosFrete>(
                    p => p.Pacotes.Count == 1 &&
                         p.Pacotes[0].Altura == 10 &&
                         p.Pacotes[0].Largura == 15 &&
                         p.Pacotes[0].Comprimento == 20
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task ExcluirAsync_PedidoExistente_DeveExcluirComSucesso()
        {
            // Arrange
            var pedidoId = 1;
            var pedidoExistente = new Pedido
            {
                Id = pedidoId,
                Status = StatusPedido.EmProcessamento
            };

            // Configurar mock do repositório para retornar o pedido existente
            _mockPedidoRepository
                .Setup(repo => repo.ObterPorIdAsync(pedidoId))
                .ReturnsAsync(pedidoExistente);

            // Configurar mock do repositório para excluir
            _mockPedidoRepository
                .Setup(repo => repo.ExcluirAsync(pedidoId))
                .Returns(Task.CompletedTask);

            // Act
            await _pedidoService.ExcluirAsync(pedidoId);

            // Assert
            _mockPedidoRepository.Verify(repo => repo.ExcluirAsync(pedidoId), Times.Once);
        }
    }
}