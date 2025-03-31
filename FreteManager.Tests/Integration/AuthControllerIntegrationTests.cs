using FreteManager.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Testes de integração para AuthController
    /// </summary>
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Registrar_UsuarioNovo_DeveRetornarSucesso()
        {
            // Arrange
            var registroRequest = new RegisterRequest
            {
                Nome = $"Usuário Teste {Guid.NewGuid()}",
                Email = $"usuario{Guid.NewGuid()}@teste.com",
                Senha = "SenhaTeste123!",
                ConfirmacaoSenha = "SenhaTeste123!"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(registroRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var resposta = await _client.PostAsync("/v1/Auth/register", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<dynamic>(respostaConteudo);

            Assert.NotNull(resultado.userId);
        }

        [Fact]
        public async Task Login_CredenciaisValidas_DeveRetornarToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "teste@integracao.com", // Usuário pré-cadastrado na factory
                Senha = "SenhaTesteSecurity123!"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var resposta = await _client.PostAsync("/v1/Auth/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<LoginResponse>(respostaConteudo);

            Assert.NotNull(resultado.Token);
            Assert.NotEmpty(resultado.Token);
        }

        [Fact]
        public async Task Login_CredenciaisInvalidas_DeveRetornarNaoAutorizado()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "naoexiste@teste.com",
                Senha = "SenhaInvalida123!"
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var resposta = await _client.PostAsync("/v1/Auth/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
    }
}