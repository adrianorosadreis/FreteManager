using FreteManager.Models;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Testes de integração para ClientesController
    /// Verifica o comportamento completo dos endpoints de gerenciamento de clientes
    /// </summary>
    public class ClientesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token;

        public ClientesControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        }

        /// <summary>
        /// Método para criar um cliente de teste com dados únicos
        /// </summary>
        private Cliente CriarClienteUnico() => new Cliente
        {
            Nome = $"Cliente Teste {Guid.NewGuid()}",
            Email = $"cliente{Guid.NewGuid()}@teste.com",
            Telefone = "1234567890",
            Endereco = "Rua Teste, 123"
        };

        [Fact]
        public async Task CriarCliente_DadosValidos_DeveCriarComSucesso()
        {
            // Arrange: Preparar dados do cliente
            var novoCliente = CriarClienteUnico();

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(novoCliente),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de criação de cliente
            var resposta = await _client.PostAsync("/v1/Clientes", content);

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(respostaConteudo);

            Assert.NotNull(clienteCriado);
            Assert.NotEqual(0, clienteCriado.Id);
            Assert.Equal(novoCliente.Nome, clienteCriado.Nome);
            Assert.Equal(novoCliente.Email, clienteCriado.Email);
        }

        [Fact]
        public async Task CriarCliente_EmailDuplicado_DeveLancarErro()
        {
            // Arrange: Criar primeiro cliente
            var primeiroCliente = CriarClienteUnico();
            var content = new StringContent(
                JsonConvert.SerializeObject(primeiroCliente),
                Encoding.UTF8,
                "application/json"
            );

            // Primeiro, criar o cliente
            var primeiraCriacaoResposta = await _client.PostAsync("/v1/Clientes", content);
            primeiraCriacaoResposta.EnsureSuccessStatusCode();

            // Tentar criar cliente com o mesmo email
            var segundoCliente = CriarClienteUnico();
            segundoCliente.Email = primeiroCliente.Email;

            var segundaContent = new StringContent(
                JsonConvert.SerializeObject(segundoCliente),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Tentar criar cliente com email duplicado
            var respostaSegunda = await _client.PostAsync("/v1/Clientes", segundaContent);

            // Assert: Verificar resposta de erro
            Assert.Equal(HttpStatusCode.BadRequest, respostaSegunda.StatusCode);
        }

        [Fact]
        public async Task AtualizarCliente_DadosValidos_DeveAtualizarComSucesso()
        {
            // Arrange: Criar cliente para atualizar
            var clienteOriginal = CriarClienteUnico();
            var content = new StringContent(
                JsonConvert.SerializeObject(clienteOriginal),
                Encoding.UTF8,
                "application/json"
            );

            // Criar cliente
            var criacaoResposta = await _client.PostAsync("/v1/Clientes", content);
            criacaoResposta.EnsureSuccessStatusCode();

            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await criacaoResposta.Content.ReadAsStringAsync()
            );

            // Preparar dados para atualização
            clienteCriado.Nome = "Cliente Atualizado";
            clienteCriado.Endereco = "Rua Atualizada, 456";

            var atualizarContent = new StringContent(
                JsonConvert.SerializeObject(clienteCriado),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Atualizar cliente
            var atualizarResposta = await _client.PutAsync(
                $"/v1/Clientes/{clienteCriado.Id}",
                atualizarContent
            );

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.NoContent, atualizarResposta.StatusCode);

            // Verificar se a atualização foi realizada
            var obterResposta = await _client.GetAsync($"/v1/Clientes/{clienteCriado.Id}");
            obterResposta.EnsureSuccessStatusCode();

            var obterConteudo = await obterResposta.Content.ReadAsStringAsync();
            var clienteAtualizado = JsonConvert.DeserializeObject<Cliente>(obterConteudo);

            Assert.Equal("Cliente Atualizado", clienteAtualizado.Nome);
            Assert.Equal("Rua Atualizada, 456", clienteAtualizado.Endereco);
        }

        [Fact]
        public async Task AtualizarCliente_EmailDuplicado_DeveLancarErro()
        {
            // Arrange: Criar dois clientes diferentes
            var primeiroCliente = CriarClienteUnico();
            var segundoCliente = CriarClienteUnico();

            // Criar ambos os clientes
            var primeiroCriacaoContent = new StringContent(
                JsonConvert.SerializeObject(primeiroCliente),
                Encoding.UTF8,
                "application/json"
            );
            var segundoCriacaoContent = new StringContent(
                JsonConvert.SerializeObject(segundoCliente),
                Encoding.UTF8,
                "application/json"
            );

            var primeiraCriacaoResposta = await _client.PostAsync("/v1/Clientes", primeiroCriacaoContent);
            primeiraCriacaoResposta.EnsureSuccessStatusCode();
            var segundaCriacaoResposta = await _client.PostAsync("/v1/Clientes", segundoCriacaoContent);
            segundaCriacaoResposta.EnsureSuccessStatusCode();

            // Recuperar os clientes criados para obter seus IDs
            var primeiroClienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await primeiraCriacaoResposta.Content.ReadAsStringAsync()
            );
            var segundoClienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await segundaCriacaoResposta.Content.ReadAsStringAsync()
            );

            // Tentar atualizar o segundo cliente com o email do primeiro
            segundoClienteCriado.Email = primeiroCliente.Email;

            var atualizarContent = new StringContent(
                JsonConvert.SerializeObject(segundoClienteCriado),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Atualizar cliente com email duplicado
            var atualizarResposta = await _client.PutAsync(
                $"/v1/Clientes/{segundoClienteCriado.Id}",
                atualizarContent
            );

            // Assert: Verificar resposta de erro
            Assert.Equal(HttpStatusCode.BadRequest, atualizarResposta.StatusCode);
        }

        [Fact]
        public async Task ExcluirCliente_ClienteExistente_DeveExcluirComSucesso()
        {
            // Arrange: Criar cliente para exclusão
            var clienteParaExcluir = CriarClienteUnico();
            var content = new StringContent(
                JsonConvert.SerializeObject(clienteParaExcluir),
                Encoding.UTF8,
                "application/json"
            );

            // Criar cliente
            var criacaoResposta = await _client.PostAsync("/v1/Clientes", content);
            criacaoResposta.EnsureSuccessStatusCode();

            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await criacaoResposta.Content.ReadAsStringAsync()
            );

            // Act: Excluir cliente
            var exclusaoResposta = await _client.DeleteAsync($"/v1/Clientes/{clienteCriado.Id}");

            // Assert: Verificar resposta de exclusão
            Assert.Equal(HttpStatusCode.NoContent, exclusaoResposta.StatusCode);

            // Verificar se o cliente foi realmente excluído
            var obterResposta = await _client.GetAsync($"/v1/Clientes/{clienteCriado.Id}");
            Assert.Equal(HttpStatusCode.NotFound, obterResposta.StatusCode);
        }

        [Fact]
        public async Task ObterClientes_ComAutenticacao_DeveRetornarLista()
        {
            // Act: Obter lista de clientes
            var resposta = await _client.GetAsync("/v1/Clientes");

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var clientes = JsonConvert.DeserializeObject<List<Cliente>>(respostaConteudo);

            Assert.NotNull(clientes);
            Assert.NotEmpty(clientes);
        }

        [Fact]
        public async Task ObterCliente_ClienteExistente_DeveRetornarDetalhes()
        {
            // Arrange: Criar cliente para obter
            var clienteOriginal = CriarClienteUnico();
            var content = new StringContent(
                JsonConvert.SerializeObject(clienteOriginal),
                Encoding.UTF8,
                "application/json"
            );

            // Criar cliente
            var criacaoResposta = await _client.PostAsync("/v1/Clientes", content);
            criacaoResposta.EnsureSuccessStatusCode();

            var clienteCriado = JsonConvert.DeserializeObject<Cliente>(
                await criacaoResposta.Content.ReadAsStringAsync()
            );

            // Act: Obter detalhes do cliente
            var obterResposta = await _client.GetAsync($"/v1/Clientes/{clienteCriado.Id}");

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.OK, obterResposta.StatusCode);

            var obterConteudo = await obterResposta.Content.ReadAsStringAsync();
            var clienteDetalhes = JsonConvert.DeserializeObject<Cliente>(obterConteudo);

            Assert.NotNull(clienteDetalhes);
            Assert.Equal(clienteCriado.Id, clienteDetalhes.Id);
            Assert.Equal(clienteOriginal.Nome, clienteDetalhes.Nome);
            Assert.Equal(clienteOriginal.Email, clienteDetalhes.Email);
        }

        [Fact]
        public async Task ObterCliente_ClienteInexistente_DeveRetornarNaoEncontrado()
        {
            // Act: Tentar obter cliente com ID inexistente
            var resposta = await _client.GetAsync("/v1/Clientes/999999");

            // Assert: Verificar resposta de não encontrado
            Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        }
    }
}