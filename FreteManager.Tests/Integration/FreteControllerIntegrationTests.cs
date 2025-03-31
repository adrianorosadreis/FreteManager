using Newtonsoft.Json;
using System.Net;
using System.Text;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Tests.Integration
{
    /// <summary>
    /// Testes de integração para FreteController
    /// Verifica o comportamento completo do endpoint de cálculo de frete
    /// </summary>
    public class FreteControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private string _token;

        public FreteControllerIntegrationTests(CustomWebApplicationFactory factory)
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
        /// Método para criar parâmetros de frete padrão para testes
        /// </summary>
        private ParametrosFrete CriarParametrosFreteValidos() => new ParametrosFrete
        {
            CepOrigem = "01000000", // CEP de São Paulo
            CepDestino = "02000000", // CEP de outro local em São Paulo
            ValorDeclarado = 1000.00m,
            Pacotes = new List<PacoteFrete>
            {
                new PacoteFrete
                {
                    Altura = 20,    // Altura em cm
                    Largura = 30,   // Largura em cm
                    Comprimento = 40, // Comprimento em cm
                    Peso = 5.0m,    // Peso em kg
                    Quantidade = 1
                }
            }
        };

        [Fact]
        public async Task CalcularFrete_DadosValidos_DeveRetornarSucesso()
        {
            // Arrange: Preparar parâmetros de frete
            var parametros = CriarParametrosFreteValidos();

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<dynamic>(respostaConteudo);

            Assert.NotNull(resultado);
            Assert.True(resultado.valorFrete > 0, "Valor de frete deve ser positivo");
            Assert.Equal(parametros.CepOrigem, (string)resultado.cepOrigem);
            Assert.Equal(parametros.CepDestino, (string)resultado.cepDestino);
        }

        [Fact]
        public async Task CalcularFrete_CepOrigemInvalido_DeveLancarErro()
        {
            // Arrange: Preparar parâmetros com CEP de origem inválido
            var parametros = CriarParametrosFreteValidos();
            parametros.CepOrigem = "INVALIDO";

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta de erro
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task CalcularFrete_CepDestinoInvalido_DeveLancarErro()
        {
            // Arrange: Preparar parâmetros com CEP de destino inválido
            var parametros = CriarParametrosFreteValidos();
            parametros.CepDestino = "INVALIDO";

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta de erro
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task CalcularFrete_SemPacotes_DeveLancarErro()
        {
            // Arrange: Preparar parâmetros sem pacotes
            var parametros = CriarParametrosFreteValidos();
            parametros.Pacotes.Clear();

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta de erro
            Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        }

        [Fact]
        public async Task CalcularFrete_MultiplosPacotes_DeveCalcularCorretamente()
        {
            // Arrange: Preparar parâmetros com múltiplos pacotes
            var parametros = new ParametrosFrete
            {
                CepOrigem = "01000000",
                CepDestino = "02000000",
                ValorDeclarado = 2000.00m,
                Pacotes = new List<PacoteFrete>
                {
                    new PacoteFrete
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 2
                    },
                    new PacoteFrete
                    {
                        Altura = 15,
                        Largura = 25,
                        Comprimento = 35,
                        Peso = 3.0m,
                        Quantidade = 1
                    }
                }
            };

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<dynamic>(respostaConteudo);

            Assert.NotNull(resultado);
            Assert.True(resultado.valorFrete > 0, "Valor de frete deve ser positivo para múltiplos pacotes");
            Assert.Equal(3, (int)resultado.quantidadePacotes);
        }

        [Fact]
        public async Task CalcularFrete_ValorDeclaradoMinimo_DeveCalcularCorretamente()
        {
            // Arrange: Preparar parâmetros com valor declarado mínimo
            var parametros = CriarParametrosFreteValidos();
            parametros.ValorDeclarado = 0.01m;

            // Converter para JSON
            var content = new StringContent(
                JsonConvert.SerializeObject(parametros),
                Encoding.UTF8,
                "application/json"
            );

            // Act: Chamar endpoint de cálculo de frete
            var resposta = await _client.PostAsync("/v1/Frete/calcular-frete", content);

            // Assert: Verificar resposta
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

            var respostaConteudo = await resposta.Content.ReadAsStringAsync();
            var resultado = JsonConvert.DeserializeObject<dynamic>(respostaConteudo);

            Assert.NotNull(resultado);
            Assert.True(resultado.valorFrete > 0, "Valor de frete deve ser positivo mesmo com valor declarado mínimo");
        }
    }
}