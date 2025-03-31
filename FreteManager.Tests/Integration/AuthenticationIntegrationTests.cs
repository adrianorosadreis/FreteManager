using FreteManager.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Testes de integração para autenticação e autorização
    /// Verifica o comportamento do sistema de segurança em diferentes cenários
    /// </summary>
    public class AuthenticationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthenticationIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Conjunto de credenciais inválidas para teste
        /// </summary>
        public static TheoryData<string, string> InvalidCredentials => new TheoryData<string, string>
        {
            { "naoexiste@teste.com", "senhaincorreta" },
            { "", "" },
            { "usuario@teste.com", "" },
            { "", "senha123" },
            { "email_invalido", "senha123" }
        };

        [Theory]
        [MemberData(nameof(InvalidCredentials))]
        public async Task Login_InvalidCredentials_ShouldReturnUnauthorized(string email, string senha)
        {
            // Arrange: Preparar requisição de login com credenciais inválidas
            var loginRequest = new LoginRequest
            {
                Email = email,
                Senha = senha
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Tentar fazer login
            var resposta = await _client.PostAsync("/v1/Auth/login", content);

            // Assert: Verificar resposta de não autorizado
            Assert.True(
                resposta.StatusCode == HttpStatusCode.Unauthorized ||
                resposta.StatusCode == HttpStatusCode.BadRequest,
                $"Esperado Unauthorized ou BadRequest para email: {email}"
            );
        }

        /// <summary>
        /// Classe para representar diferentes perfis de teste
        /// </summary>
        public class UserRoleTestCase
        {
            public string Email { get; set; }
            public string Senha { get; set; }
            public string Role { get; set; }
            public string[] AccessibleEndpoints { get; set; }
            public string[] RestrictedEndpoints { get; set; }
        }

        /// <summary>
        /// Casos de teste para diferentes perfis de usuário
        /// </summary>
        public static TheoryData<UserRoleTestCase> UserRoleTestCases => new TheoryData<UserRoleTestCase>
        {
            new UserRoleTestCase
            {
                Email = "admin@fretemanager.com",
                Senha = "Admin@123",
                Role = "Admin",
                AccessibleEndpoints = new[]
                {
                    "/v1/Clientes",
                    "/v1/Pedidos",
                    "/v1/Frete/calcular-frete"
                },
                RestrictedEndpoints = new string[] { } // Admin tem acesso a tudo
            },
            new UserRoleTestCase
            {
                Email = "operador@fretemanager.com",
                Senha = "Operador@123",
                Role = "Operador",
                AccessibleEndpoints = new[]
                {
                    "/v1/Pedidos",
                    "/v1/Frete/calcular-frete"
                },
                RestrictedEndpoints = new[]
                {
                    "/v1/Clientes/criar",
                    "/v1/Clientes/atualizar"
                }
            }
        };

        [Theory]
        [MemberData(nameof(UserRoleTestCases))]
        public async Task Authorization_VerifyAccessControl(UserRoleTestCase testCase)
        {
            // Arrange: Fazer login com usuário específico
            var loginRequest = new LoginRequest
            {
                Email = testCase.Email,
                Senha = testCase.Senha
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await _client.PostAsync("/v1/Auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginResponseContent);

            // Configurar cliente com token de autorização
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

            // Testar endpoints acessíveis
            foreach (var endpoint in testCase.AccessibleEndpoints)
            {
                var accessResponse = await _client.GetAsync(endpoint);
                Assert.True(
                    accessResponse.StatusCode == HttpStatusCode.OK,
                    $"Usuário {testCase.Role} deveria ter acesso a {endpoint}"
                );
            }

            // Testar endpoints restritos
            foreach (var endpoint in testCase.RestrictedEndpoints)
            {
                var restrictedResponse = await _client.GetAsync(endpoint);
                Assert.True(
                    restrictedResponse.StatusCode == HttpStatusCode.Forbidden,
                    $"Usuário {testCase.Role} não deveria ter acesso a {endpoint}"
                );
            }
        }

        [Fact]
        public async Task JwtToken_Expiration_ShouldPreventAccess()
        {
            // Arrange: Fazer login
            var loginRequest = new LoginRequest
            {
                Email = "admin@fretemanager.com",
                Senha = "Admin@123"
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await _client.PostAsync("/v1/Auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginResponseContent);

            // Simular espera para expiração do token
            await Task.Delay(TimeSpan.FromHours(2)); // Tempo maior que o tempo de expiração do token

            // Configurar cliente com token expirado
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

            // Act: Tentar acessar endpoint protegido
            var accessResponse = await _client.GetAsync("/v1/Clientes");

            // Assert: Verificar que o acesso é negado
            Assert.Equal(HttpStatusCode.Unauthorized, accessResponse.StatusCode);
        }
    }
}