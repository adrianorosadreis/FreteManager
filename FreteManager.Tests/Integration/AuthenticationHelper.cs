using FreteManager.Models;
using Newtonsoft.Json;
using System.Text;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Classe auxiliar para autenticação em testes de integração
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Obtém um token de autenticação para testes
        /// </summary>
        /// <param name="client">Cliente HTTP para realizar a requisição</param>
        /// <param name="email">Email para login</param>
        /// <param name="senha">Senha para login</param>
        /// <returns>Token de autenticação</returns>
        public static async Task<string> ObterTokenAutenticacao(
            HttpClient client,
            string email = "teste@integracao.com",
            string senha = "SenhaTesteSecurity123!")
        {
            var loginRequest = new LoginRequest
            {
                Email = email,
                Senha = senha
            };

            var loginContent = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await client.PostAsync("/v1/Auth/login", loginContent);

            // Garantir que o login foi bem-sucedido
            loginResponse.EnsureSuccessStatusCode();

            var responseContent = await loginResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

            return result.Token;
        }

        /// <summary>
        /// Adiciona o token de autenticação ao cabeçalho do cliente HTTP
        /// </summary>
        public static void AdicionarTokenAutenticacao(HttpClient client, string token)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}