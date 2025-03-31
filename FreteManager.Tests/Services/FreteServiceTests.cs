using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using FreteManager.Services;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Tests.Services
{
    /// <summary>
    /// Testes unitários para o serviço de cálculo de frete
    /// </summary>
    public class FreteServiceTests : IDisposable
    {
        // Mocks para simular dependências externas
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<FreteService>> _mockLogger;
        private readonly Mock<IMemoryCache> _mockCache;

        // Instância do serviço de frete para teste
        private readonly FreteService _freteService;

        public FreteServiceTests()
        {
            // Configuração dos mocks
            _mockHttpClient = new Mock<HttpClient>();

            // Configurar mock de configuração com token de teste
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration
                .Setup(c => c["Frenet:Token"])
                .Returns("TEST_TOKEN_FRENET");

            // Configurar mock de logger
            _mockLogger = new Mock<ILogger<FreteService>>();

            // Configurar mock de cache de memória
            _mockCache = new Mock<IMemoryCache>();

            // Criar handler HTTP mockado para simulação de respostas
            var handlerMock = new Mock<HttpMessageHandler>();

            // Configurar resposta simulada do serviço externo
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(
                        new FrenetResponse
                        {
                            ShippingSevicesArray = new List<FrenetShippingService>
                            {
                                new FrenetShippingService
                                {
                                    Carrier = "Correios",
                                    ServiceDescription = "SEDEX",
                                    ShippingPrice = "50.00",
                                    DeliveryTime = "3",
                                    Error = false
                                }
                            }
                        }
                    ), Encoding.UTF8, "application/json")
                });

            // Criar cliente HTTP mockado
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frenet.com.br")
            };

            // Criar uma instância real de cache de memória
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Inicializar o serviço de frete com os mocks
            _freteService = new FreteService(
                httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object,
                memoryCache
            );
        }

        [Fact]
        public async Task CalcularFreteDetalhadoAsync_RespostaValida_DeveRetornarValorCorreto()
        {
            // Arrange: Preparar parâmetros de frete
            var parametros = new ParametrosFrete
            {
                CepOrigem = "01000000",
                CepDestino = "02000000",
                ValorDeclarado = 1000.00m,
                Pacotes = new List<PacoteFrete>
                {
                    new PacoteFrete
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 1
                    }
                }
            };

            // Act: Calcular frete
            var valorFrete = await _freteService.CalcularFreteDetalhadoAsync(parametros);

            // Assert: Verificar se o valor retornado corresponde ao esperado
            Assert.Equal(50.00m, valorFrete);

            // Verificar se o logger registrou informações
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Frete calculado")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task CalcularFreteDetalhadoAsync_CepInvalido_DeveLancarExcecao()
        {
            // Arrange: Preparar parâmetros com CEP inválido
            var parametros = new ParametrosFrete
            {
                CepOrigem = "123", // CEP inválido
                CepDestino = "456", // CEP inválido
                ValorDeclarado = 1000.00m,
                Pacotes = new List<PacoteFrete>
                {
                    new PacoteFrete
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 1
                    }
                }
            };

            // Act & Assert: Verificar se lança exceção para CEP inválido
            await Assert.ThrowsAsync<ArgumentException>(
                () => _freteService.CalcularFreteDetalhadoAsync(parametros)
            );
        }

        [Fact]
        public async Task CalcularFreteDetalhadoAsync_SemPacotes_DeveLancarExcecao()
        {
            // Arrange: Preparar parâmetros sem pacotes
            var parametros = new ParametrosFrete
            {
                CepOrigem = "01000000",
                CepDestino = "02000000",
                ValorDeclarado = 1000.00m,
                Pacotes = new List<PacoteFrete>() // Lista vazia
            };

            // Act & Assert: Verificar se lança exceção para lista de pacotes vazia
            await Assert.ThrowsAsync<ArgumentException>(
                () => _freteService.CalcularFreteDetalhadoAsync(parametros)
            );
        }

        [Fact]
        public async Task CalcularFreteDetalhadoAsync_FalhaAPI_DeveUsarCalculoFallback()
        {
            // Configurar mock para simular falha na API
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Erro de comunicação"));

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frenet.com.br")
            };

            // Recriar serviço com cliente HTTP mockado para falha
            var freteService = new FreteService(
                httpClient,
                _mockConfiguration.Object,
                _mockLogger.Object,
                new MemoryCache(new MemoryCacheOptions())
            );

            // Arrange: Preparar parâmetros de frete
            var parametros = new ParametrosFrete
            {
                CepOrigem = "01000000",
                CepDestino = "02000000",
                ValorDeclarado = 1000.00m,
                Pacotes = new List<PacoteFrete>
                {
                    new PacoteFrete
                    {
                        Altura = 20,
                        Largura = 30,
                        Comprimento = 40,
                        Peso = 5.0m,
                        Quantidade = 1
                    }
                }
            };

            // Act: Calcular frete com falha de API
            var valorFrete = await freteService.CalcularFreteDetalhadoAsync(parametros);

            // Assert: Verificar se usa cálculo fallback
            // Cálculo fallback: valorBase (15.00) + (pesoTotal * valorPorKg)
            var pesoTotal = parametros.Pacotes.Sum(p => p.Peso * p.Quantidade);
            var valorEsperado = Math.Round(15.00m + (pesoTotal * 2.50m), 2);

            Assert.Equal(valorEsperado, valorFrete);
        }

        public void Dispose()
        {
            // Limpeza de recursos, se necessário
            _mockHttpClient.Reset();
            _mockConfiguration.Reset();
            _mockLogger.Reset();
            _mockCache.Reset();
        }
    }

    // Classes para deserialização da resposta da API (podem estar em outro arquivo)
    public class FrenetResponse
    {
        public List<FrenetShippingService> ShippingSevicesArray { get; set; }
    }

    public class FrenetShippingService
    {
        public string Carrier { get; set; }
        public string ServiceDescription { get; set; }
        public string ShippingPrice { get; set; }
        public string DeliveryTime { get; set; }
        public bool Error { get; set; }
    }
}