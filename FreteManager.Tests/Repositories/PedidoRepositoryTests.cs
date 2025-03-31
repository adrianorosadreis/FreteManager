using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FreteManager.Data;
using FreteManager.Models;
using FreteManager.Repositories;
using FreteManager.Exceptions;

namespace FreteManager.Tests.Repositories
{
    /// <summary>
    /// Testes de repositório para operações de Pedido
    /// Utiliza InMemory Database para isolar testes de banco de dados
    /// </summary>
    public class PedidoRepositoryTests : IDisposable
    {
        // Contexto de banco de dados em memória
        private readonly ApplicationDbContext _context;

        // Repositório de pedidos sendo testado
        private readonly PedidoRepository _repository;

        // Mock de logger para capturar logs durante os testes
        private readonly Mock<ILogger<PedidoRepository>> _mockLogger;

        /// <summary>
        /// Configuração inicial para cada teste
        /// Cria um banco de dados em memória único para cada execução de teste
        /// </summary>
        public PedidoRepositoryTests()
        {
            // Configurar opções para banco de dados em memória
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Criar contexto de banco de dados
            _context = new ApplicationDbContext(options);

            // Criar mock de logger
            _mockLogger = new Mock<ILogger<PedidoRepository>>();

            // Criar repositório com o contexto e logger mockado
            _repository = new PedidoRepository(_context, _mockLogger.Object);

            // Popular dados iniciais para testes
            SeedData();
        }

        /// <summary>
        /// Método para popular dados iniciais no banco de dados de teste
        /// </summary>
        private void SeedData()
        {
            // Criar cliente de teste
            var cliente = new Cliente
            {
                Nome = "Cliente Teste",
                Email = "cliente@teste.com",
                Telefone = "1234567890",
                Endereco = "Rua Teste, 123"
            };
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AdicionarAsync_NovoPedido_DeveInserirComSucesso()
        {
            // Arrange: Preparar dados de teste
            var cliente = _context.Clientes.First();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Origem = "01000-000",
                Destino = "02000-000",
                DataCriacao = DateTime.Now,
                Status = StatusPedido.EmProcessamento,
                ValorFrete = 50.00m,
                ValorDeclarado = 1000.00m,
                Pacotes = new List<Pacote>
                {
                    new Pacote
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 1
                    }
                }
            };

            // Act: Adicionar pedido
            var pedidoInserido = await _repository.AdicionarAsync(pedido);

            // Assert: Verificar se o pedido foi inserido corretamente
            Assert.NotNull(pedidoInserido);
            Assert.True(pedidoInserido.Id > 0);
            Assert.Equal(pedido.Origem, pedidoInserido.Origem);
            Assert.Single(pedidoInserido.Pacotes);
        }

        [Fact]
        public async Task ObterPorIdAsync_PedidoExistente_DeveRetornarPedido()
        {
            // Arrange: Criar e salvar um pedido
            var cliente = _context.Clientes.First();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Origem = "03000-000",
                Destino = "04000-000",
                DataCriacao = DateTime.Now,
                Status = StatusPedido.EmProcessamento
            };
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            // Act: Obter o pedido por ID
            var pedidoEncontrado = await _repository.ObterPorIdAsync(pedido.Id);

            // Assert: Verificar se o pedido foi encontrado corretamente
            Assert.NotNull(pedidoEncontrado);
            Assert.Equal(pedido.Id, pedidoEncontrado.Id);
            Assert.Equal(pedido.Origem, pedidoEncontrado.Origem);
        }

        [Fact]
        public async Task ObterPorIdAsync_PedidoInexistente_DeveLancarExcecao()
        {
            // Act & Assert: Verificar se lança exceção para ID inexistente
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _repository.ObterPorIdAsync(999999)
            );
        }

        [Fact]
        public async Task AtualizarAsync_PedidoExistente_DeveAtualizarCorretamente()
        {
            // Arrange: Criar e salvar um pedido
            var cliente = _context.Clientes.First();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Origem = "05000-000",
                Destino = "06000-000",
                DataCriacao = DateTime.Now,
                Status = StatusPedido.EmProcessamento
            };
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            // Act: Atualizar o pedido
            pedido.Status = StatusPedido.Enviado;
            pedido.ValorFrete = 75.00m;
            await _repository.AtualizarAsync(pedido);

            // Recarregar do banco para verificar
            var pedidoAtualizado = await _repository.ObterPorIdAsync(pedido.Id);

            // Assert: Verificar se as alterações foram salvas
            Assert.Equal(StatusPedido.Enviado, pedidoAtualizado.Status);
            Assert.Equal(75.00m, pedidoAtualizado.ValorFrete);
        }

        [Fact]
        public async Task ExcluirAsync_PedidoExistente_DeveRemoverPedido()
        {
            // Arrange: Criar e salvar um pedido
            var cliente = _context.Clientes.First();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Origem = "07000-000",
                Destino = "08000-000",
                DataCriacao = DateTime.Now,
                Status = StatusPedido.EmProcessamento
            };
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            // Act: Excluir o pedido
            await _repository.ExcluirAsync(pedido.Id);

            // Assert: Verificar se o pedido foi removido
            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _repository.ObterPorIdAsync(pedido.Id)
            );
        }

        [Fact]
        public async Task ListarTodosAsync_DeveRetornarTodosPedidos()
        {
            // Arrange: Criar múltiplos pedidos
            var cliente = _context.Clientes.First();
            var pedidos = new[]
            {
                new Pedido
                {
                    ClienteId = cliente.Id,
                    Origem = "09000-000",
                    Destino = "10000-000",
                    DataCriacao = DateTime.Now,
                    Status = StatusPedido.EmProcessamento
                },
                new Pedido
                {
                    ClienteId = cliente.Id,
                    Origem = "11000-000",
                    Destino = "12000-000",
                    DataCriacao = DateTime.Now,
                    Status = StatusPedido.Enviado
                }
            };
            _context.Pedidos.AddRange(pedidos);
            _context.SaveChanges();

            // Act: Listar todos os pedidos
            var todosPedidos = await _repository.ListarTodosAsync();

            // Assert: Verificar se todos os pedidos foram retornados
            Assert.NotEmpty(todosPedidos);
            Assert.True(todosPedidos.Count() >= 2);
        }

        [Fact]
        public async Task ListarPorClienteAsync_DeveFiltrarPedidosPorCliente()
        {
            // Arrange: Criar pedidos para um cliente específico
            var cliente = _context.Clientes.First();
            var pedidos = new[]
            {
                new Pedido
                {
                    ClienteId = cliente.Id,
                    Origem = "13000-000",
                    Destino = "14000-000",
                    DataCriacao = DateTime.Now,
                    Status = StatusPedido.EmProcessamento
                },
                new Pedido
                {
                    ClienteId = cliente.Id,
                    Origem = "15000-000",
                    Destino = "16000-000",
                    DataCriacao = DateTime.Now,
                    Status = StatusPedido.Enviado
                }
            };
            _context.Pedidos.AddRange(pedidos);
            _context.SaveChanges();

            // Act: Listar pedidos do cliente
            var pedidosCliente = await _repository.ListarPorClienteAsync(cliente.Id);

            // Assert: Verificar se apenas os pedidos do cliente foram retornados
            Assert.NotEmpty(pedidosCliente);
            Assert.All(pedidosCliente, p => Assert.Equal(cliente.Id, p.ClienteId));
            Assert.Equal(2, pedidosCliente.Count());
        }

        [Fact]
        public async Task AdicionarAsync_PedidoComPacotes_DeveInserirComSucesso()
        {
            // Arrange: Criar pedido com múltiplos pacotes
            var cliente = _context.Clientes.First();
            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                Origem = "17000-000",
                Destino = "18000-000",
                DataCriacao = DateTime.Now,
                Status = StatusPedido.EmProcessamento,
                Pacotes = new List<Pacote>
                {
                    new Pacote
                    {
                        Altura = 10,
                        Largura = 20,
                        Comprimento = 30,
                        Peso = 5.0m,
                        Quantidade = 2
                    },
                    new Pacote
                    {
                        Altura = 15,
                        Largura = 25,
                        Comprimento = 35,
                        Peso = 3.0m,
                        Quantidade = 1
                    }
                }
            };

            // Act: Adicionar pedido com pacotes
            var pedidoInserido = await _repository.AdicionarAsync(pedido);

            // Assert: Verificar se pedido e pacotes foram inseridos corretamente
            Assert.NotNull(pedidoInserido);
            Assert.True(pedidoInserido.Id > 0);
            Assert.Equal(2, pedidoInserido.Pacotes.Count);
        }

        /// <summary>
        /// Método para limpeza de recursos após os testes
        /// </summary>
        public void Dispose()
        {
            // Limpar o contexto de banco de dados
            _context.Dispose();
        }
    }
}