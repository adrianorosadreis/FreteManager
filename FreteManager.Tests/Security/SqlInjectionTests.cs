using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;
using FreteManager.Models;
using FreteManager.Tests.Integration;

namespace FreteManager.Tests.Security
{
    /// <summary>
    /// Testes de integração para verificar vulnerabilidades de Injeção de SQL
    /// </summary>
    public class SqlInjectionTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _adminToken;

        public SqlInjectionTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();

            // Obter token de autenticação para admin
            _adminToken = ObterTokenAdmin().GetAwaiter().GetResult();
        }

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
        /// Conjunto de payloads de injeção de SQL para teste
        /// </summary>
        public static TheoryData<string> SqlInjectionPayloads => new TheoryData<string>
        {
            // Payloads clássicos de SQL Injection
            "' OR 1=1--",
            "'; DROP TABLE Usuarios; --",
            "1 OR '1'='1'",
            "' UNION SELECT * FROM Usuarios--",
            "admin' --",
            "' OR ''=''"
        };

        [Theory]
        [MemberData(nameof(SqlInjectionPayloads))]
        public async Task ClientesEndpoint_SqlInjectionAttempt_ShouldNotAllowUnauthorizedAccess(string payload)
        {
            // Configurar cliente sem autenticação
            _client.DefaultRequestHeaders.Clear();

            // Tentar acessar endpoint de clientes com payload de injeção
            var response = await _client.GetAsync($"/v1/Clientes/{payload}");

            // Verificar que o acesso não é autorizado
            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"Payload de SQL Injection '{payload}' não foi corretamente bloqueado"
            );
        }

        [Theory]
        [MemberData(nameof(SqlInjectionPayloads))]
        public async Task PedidosEndpoint_SqlInjectionAttempt_ShouldNotAllowUnauthorizedAccess(string payload)
        {
            // Configurar cliente sem autenticação
            _client.DefaultRequestHeaders.Clear();

            // Tentar acessar endpoint de pedidos com payload de injeção
            var response = await _client.GetAsync($"/v1/Pedidos/{payload}");

            // Verificar que o acesso não é autorizado
            Assert.True(
                response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden,
                $"Payload de SQL Injection '{payload}' não foi corretamente bloqueado"
            );
        }

        [Theory]
        [MemberData(nameof(SqlInjectionPayloads))]
        public async Task FreteEndpoint_SqlInjectionAttempt_ShouldNotAllowUnauthorizedAccess(string payload)
        {
            // Preparar conteúdo de requisição com payload de injeção
            var requestBody = new
            {
                CepOrigem = payload,
                CepDestino = "01000000",
                ValorDeclarado = 1000.00m,
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

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            // Tentar calcular frete com payload de injeção
            var response = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Verificar que o acesso não é autorizado ou retorna erro de validação
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Unauthorized,
                $"Payload de SQL Injection '{payload}' não foi corretamente bloqueado"
            );
        }

        [Fact]
        public async Task AuthEndpoint_SensitiveErrorMessages_ShouldNotRevealInternalDetails()
        {
            // Tentar login com credenciais inválidas
            var loginRequest = new LoginRequest
            {
                Email = "' OR 1=1--",
                Senha = "' OR 1=1--"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Fazer requisição de login
            var response = await _client.PostAsync("/v1/Auth/login", content);

            // Verificar resposta de erro
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            // Ler conteúdo da resposta
            var responseContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

            // Verificar que detalhes internos não são expostos
            Assert.NotNull(errorResponse);
            Assert.False(
                ((string)errorResponse.detail).Contains("SQL"),
                "Mensagem de erro não deve conter detalhes internos de banco de dados"
            );
            Assert.False(
                ((string)errorResponse.detail).Contains("Exception"),
                "Mensagem de erro não deve conter detalhes de exceção"
            );
        }
    }
}