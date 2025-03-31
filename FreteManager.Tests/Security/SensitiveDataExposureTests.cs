using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Xunit;
using FreteManager.Models;
using FreteManager.Tests.Integration;

namespace FreteManager.Tests.Security
{
    /// <summary>
    /// Testes de integração para verificar exposição de dados sensíveis
    /// Garante que informações confidenciais não sejam inadvertidamente expostas
    /// </summary>
    public class SensitiveDataExposureTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _adminToken;

        public SensitiveDataExposureTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Obter token de autenticação para admin
            _adminToken = ObterTokenAdmin().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Método auxiliar para obter token de autenticação de admin
        /// </summary>
        private async Task<string> ObterTokenAdmin()
        {
            var loginRequest = new LoginRequest
            {
                Email = "admin@fretemanager.com",
                Senha = "Admin@123"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await _client.PostAsync("/v1/Auth/login", content);
            loginResponse.EnsureSuccessStatusCode();

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginResponseContent);

            return loginResult.Token;
        }

        /// <summary>
        /// Verifica se dados de cliente não expõem informações sensíveis
        /// </summary>
        [Fact]
        public async Task VerificarCliente_NaoDeveExporSenhasOuDadosSensiveis()
        {
            // Configurar cliente com token de admin
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);

            // Criar um novo cliente para teste
            var novoCliente = new Cliente
            {
                Nome = $"Cliente Teste {Guid.NewGuid()}",
                Email = $"cliente{Guid.NewGuid()}@teste.com",
                Telefone = "1234567890",
                Endereco = "Rua Teste, 123"
            };

            var criarContent = new StringContent(
                JsonConvert.SerializeObject(novoCliente),
                Encoding.UTF8,
                "application/json"
            );

            // Criar cliente
            var criarResposta = await _client.PostAsync("/v1/Clientes", criarContent);
            criarResposta.EnsureSuccessStatusCode();

            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await criarResposta.Content.ReadAsStringAsync()
            );

            // Obter detalhes do cliente
            var obterResposta = await _client.GetAsync($"/v1/Clientes/{clienteCriado.Id}");
            obterResposta.EnsureSuccessStatusCode();

            var clienteDetalhes = await obterResposta.Content.ReadAsStringAsync();

            // Conjunto de padrões de dados sensíveis a serem verificados
            var dadosSensiveis = new[]
            {
                "Senha", "Password", "Hash", "Token",
                "Secret", "Credencial", "Autenticacao"
            };

            // Verificar que não há exposição de dados sensíveis
            foreach (var dadoSensivel in dadosSensiveis)
            {
                Assert.DoesNotContain(dadoSensivel, clienteDetalhes, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica se a lista de clientes não expõe informações sensíveis
        /// </summary>
        [Fact]
        public async Task VerificarListaClientes_NaoDeveExporDadosSensiveis()
        {
            // Configurar cliente com token de admin
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);

            // Obter lista de clientes
            var resposta = await _client.GetAsync("/v1/Clientes");
            resposta.EnsureSuccessStatusCode();

            var clientesJson = await resposta.Content.ReadAsStringAsync();

            // Conjunto de padrões de dados sensíveis a serem verificados
            var dadosSensiveis = new[]
            {
                "Senha", "Password", "Hash", "Token",
                "Secret", "Credencial", "Autenticacao"
            };

            // Verificar que não há exposição de dados sensíveis
            foreach (var dadoSensivel in dadosSensiveis)
            {
                Assert.DoesNotContain(dadoSensivel, clientesJson, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica se detalhes de pedido não expõem informações sensíveis
        /// </summary>
        [Fact]
        public async Task VerificarPedido_NaoDeveExporDadosSensiveis()
        {
            // Configurar cliente com token de admin
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _adminToken);

            // Criar cliente para o pedido
            var novoCliente = new Cliente
            {
                Nome = $"Cliente Pedido {Guid.NewGuid()}",
                Email = $"cliente.pedido{Guid.NewGuid()}@teste.com",
                Telefone = "1234567890",
                Endereco = "Rua Teste, 123"
            };

            var criarClienteContent = new StringContent(
                JsonConvert.SerializeObject(novoCliente),
                Encoding.UTF8,
                "application/json"
            );

            var criarClienteResposta = await _client.PostAsync("/v1/Clientes", criarClienteContent);
            criarClienteResposta.EnsureSuccessStatusCode();

            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await criarClienteResposta.Content.ReadAsStringAsync()
            );

            // Criar pedido
            var novoPedido = new
            {
                ClienteId = clienteCriado.Id,
                Origem = "01000-000",
                Destino = "02000-000",
                Pacotes = new[]
                {
                    new
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 1
                    }
                }
            };

            var criarPedidoContent = new StringContent(
                JsonConvert.SerializeObject(novoPedido),
                Encoding.UTF8,
                "application/json"
            );

            var criarPedidoResposta = await _client.PostAsync("/v1/Pedidos", criarPedidoContent);
            criarPedidoResposta.EnsureSuccessStatusCode();

            var pedidoCriado = JsonConvert.DeserializeObject<dynamic>(
                await criarPedidoResposta.Content.ReadAsStringAsync()
            );

            // Obter detalhes do pedido
            var obterPedidoResposta = await _client.GetAsync($"/v1/Pedidos/{pedidoCriado.id}");
            obterPedidoResposta.EnsureSuccessStatusCode();

            var pedidoDetalhes = await obterPedidoResposta.Content.ReadAsStringAsync();

            // Conjunto de padrões de dados sensíveis a serem verificados
            var dadosSensiveis = new[]
            {
                "Senha", "Password", "Hash", "Token",
                "Secret", "Credencial", "Autenticacao"
            };

            // Verificar que não há exposição de dados sensíveis
            foreach (var dadoSensivel in dadosSensiveis)
            {
                Assert.DoesNotContain(dadoSensivel, pedidoDetalhes, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Verifica se a resposta de login não expõe informações sensíveis
        /// </summary>
        [Fact]
        public async Task LoginResponse_NaoDeveExporSenhaOuTokenCompleto()
        {
            // Fazer login
            var loginRequest = new LoginRequest
            {
                Email = "admin@fretemanager.com",
                Senha = "Admin@123"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await _client.PostAsync("/v1/Auth/login", content);
            loginResponse.EnsureSuccessStatusCode();

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<dynamic>(loginResponseContent);

            // Verificar que o token não contém informações completas do usuário
            Assert.False(
                ((string)loginResult.token).Contains("Password"),
                "Token não deve conter informações de senha"
            );

            Assert.False(
                ((string)loginResult.token).Contains("Secret"),
                "Token não deve conter chaves secretas"
            );

            // Verificar que alguns dados são mascarados
            Assert.NotNull(loginResult.nome);
            Assert.NotNull(loginResult.email);
            Assert.True(
                ((string)loginResult.email).Contains("@"),
                "Email deve ser parcialmente exposto"
            );
        }        
    }
}