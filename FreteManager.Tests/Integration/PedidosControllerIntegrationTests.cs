using FreteManager.DTOs;
using FreteManager.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Testes de integração para PedidosController
    /// </summary>
    public class PedidosControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token;
        private int _clienteId;

        public PedidosControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Obter token de autenticação
            _token = AuthenticationHelper
                .ObterTokenAutenticacao(_client)
                .GetAwaiter()
                .GetResult();

            // Adicionar token aos cabeçalhos
            AuthenticationHelper.AdicionarTokenAutenticacao(_client, _token);

            // Obter ID de um cliente existente
            _clienteId = ObterIdClienteExistente().GetAwaiter().GetResult();
        }

        private async Task<int> ObterIdClienteExistente()
        {
            var resposta = await _client.GetAsync("/v1/Clientes");
            resposta.EnsureSuccessStatusCode();

            var clientes = JsonConvert.DeserializeObject<List<Cliente>>(
                await resposta.Content.ReadAsStringAsync()
            );

            return clientes.First().Id;
        }

        [Fact]
        public async Task CriarPedido_DadosValidos_DeveCriarComSucesso()
        {
            // Arrange
            var novoPedido = new CriarPedidoDTO
            {
                ClienteId = _clienteId,
                Origem = "01000-000",
                Destino = "02000-000",
                Pacotes = new List<PacoteDTO>
                {
                    new PacoteDTO
                    {
                        Altura = 10,
                        Largura = 20,
                        Comprimento = 30,
                        Peso = 1.5m,
                        Quantidade = 1
                    }
                }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novoPedido),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var resposta = await _client.PostAsync("/v1/Pedidos", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var pedidoCriado = JsonConvert.DeserializeObject<PedidoRespostaDTO>(respostaConteudo);

            Assert.NotNull(pedidoCriado);
            Assert.NotEqual(0, pedidoCriado.Id);
            Assert.Equal(novoPedido.Origem, pedidoCriado.Origem);
            Assert.Equal(StatusPedido.EmProcessamento, pedidoCriado.Status);
        }

        [Fact]
        public async Task ObterPedidos_ComAutenticacao_DeveRetornarLista()
        {
            // Act
            var resposta = await _client.GetAsync("/v1/Pedidos");

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var pedidos = JsonConvert.DeserializeObject<List<PedidoRespostaDTO>>(respostaConteudo);

            Assert.NotNull(pedidos);
        }

        [Fact]
        public async Task AtualizarStatus_StatusValido_DeveAtualizarComSucesso()
        {
            // Primeiro, criar um pedido
            var novoPedido = new CriarPedidoDTO
            {
                ClienteId = _clienteId,
                Origem = "03000-000",
                Destino = "04000-000",
                Pacotes = new List<PacoteDTO>
                {
                    new PacoteDTO
                    {
                        Altura = 10,
                        Largura = 20,
                        Comprimento = 30,
                        Peso = 1.5m,
                        Quantidade = 1
                    }
                }
            };

            var criarContent = new StringContent(
                JsonConvert.SerializeObject(novoPedido),
                Encoding.UTF8,
                "application/json"
            );

            var criarResposta = await _client.PostAsync("/v1/Pedidos", criarContent);
            criarResposta.EnsureSuccessStatusCode();

            var pedidoCriado = JsonConvert.DeserializeObject<PedidoRespostaDTO>(
                await criarResposta.Content.ReadAsStringAsync()
            );

            // Preparar atualização de status
            var statusUpdate = new
            {
                novoStatus = StatusPedido.Enviado
            };

            var statusContent = new StringContent(
                JsonConvert.SerializeObject(statusUpdate),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var atualizarResposta = await _client.PatchAsync(
                $"/v1/Pedidos/{pedidoCriado.Id}/status",
                statusContent
            );

            // Assert
            Assert.Equal(HttpStatusCode.OK, atualizarResposta.StatusCode);

            var respostaConteudo = await atualizarResposta.Content.ReadAsStringAsync();
            var pedidoAtualizado = JsonConvert.DeserializeObject<PedidoRespostaDTO>(respostaConteudo);

            Assert.Equal(StatusPedido.Enviado, pedidoAtualizado.Status);
        }

        [Fact]
        public async Task CriarPedido_ClienteInexistente_DeveLancarErro()
        {
            // Arrange
            var novoPedido = new CriarPedidoDTO
            {
                ClienteId = 999999, // ID de cliente inexistente
                Origem = "05000-000",
                Destino = "06000-000",
                Pacotes = new List<PacoteDTO>
                {
                    new PacoteDTO
                    {
                        Altura = 10,
                        Largura = 20,
                        Comprimento = 30,
                        Peso = 1.5m,
                        Quantidade = 1
                    }
                }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(novoPedido),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var resposta = await _client.PostAsync("/v1/Pedidos", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
    }
}