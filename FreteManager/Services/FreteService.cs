using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Services
{
    /// Serviço que integra com a Frenet API para cálculo de frete com sistema de cache
    public class FreteService : IFreteService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FreteService> _logger;
        private readonly IMemoryCache _cache;

        /// Inicializa uma nova instância do serviço Frenet
        public FreteService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FreteService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;

            // HttpClient com os cabeçalhos necessários para a API Frenet
            _httpClient.BaseAddress = new Uri("https://api.frenet.com.br");
            _httpClient.DefaultRequestHeaders.Add("token", _configuration["Frenet:Token"]);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// Método principal que realiza o cálculo detalhado de frete
        public async Task<decimal> CalcularFreteDetalhadoAsync(ParametrosFrete parametros)
        {
            // Verificar cache primeiro para evitar chamadas desnecessárias
            string cacheKey = GerarChaveCache(parametros);
            if (_cache.TryGetValue(cacheKey, out decimal cachedValue))
            {
                _logger.LogInformation("Valor de frete recuperado do cache: {Valor}", cachedValue);
                return cachedValue;
            }

            try
            {
                // Sanitizar e validar os CEPs
                string cepOrigem = new string(parametros.CepOrigem.Where(char.IsDigit).ToArray());
                string cepDestino = new string(parametros.CepDestino.Where(char.IsDigit).ToArray());

                if (cepOrigem.Length != 8 || cepDestino.Length != 8)
                {
                    throw new ArgumentException("CEPs de origem e destino devem ter 8 dígitos");
                }

                // Preparar o payload da requisição conforme documentação da Frenet
                var shippingItems = parametros.Pacotes.Select(p => new
                {
                    Height = Math.Round(p.Altura, 2),
                    Length = Math.Round(p.Comprimento, 2),
                    Width = Math.Round(p.Largura, 2),
                    Weight = Math.Round(p.Peso, 2),
                    Quantity = p.Quantidade
                }).ToArray();

                var requestData = new
                {
                    SellerCEP = cepOrigem,
                    RecipientCEP = cepDestino,
                    ShipmentInvoiceValue = parametros.ValorDeclarado,
                    ShippingItemArray = shippingItems,
                    RecipientCountry = "BR" // Brasil
                };

                // Fazer a chamada à API Frenet
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/shipping/quote", content);
                response.EnsureSuccessStatusCode();

                // Processar a resposta
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Resposta da API Frenet: {Response}", responseContent);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var frenetResponse = JsonSerializer.Deserialize<FrenetResponse>(responseContent, options);

                // Verificar se existem opções de frete disponíveis
                if (frenetResponse?.ShippingSevicesArray == null || !frenetResponse.ShippingSevicesArray.Any())
                {
                    _logger.LogWarning("Nenhuma opção de frete disponível para os CEPs informados");
                    return CalcularFreteFallback(parametros);
                }

                // Filtrar opções válidas (sem erro)
                var validOptions = frenetResponse.ShippingSevicesArray.Where(s => !s.Error).ToList();
                if (!validOptions.Any())
                {
                    _logger.LogWarning("Nenhuma opção de frete válida disponível");
                    return CalcularFreteFallback(parametros);
                }

                // Selecionar a opção mais barata
                var cheapestOption = validOptions
                    .OrderBy(s => decimal.Parse(s.ShippingPrice, CultureInfo.InvariantCulture))
                    .First();

                _logger.LogInformation(
                    "Frete calculado: {Carrier} - {Service}, Valor: {Price}, Prazo: {Time} dias",
                    cheapestOption.Carrier,
                    cheapestOption.ServiceDescription,
                    cheapestOption.ShippingPrice,
                    cheapestOption.DeliveryTime);

                // Converter para decimal e armazenar em cache
                decimal freteValue = decimal.Parse(cheapestOption.ShippingPrice, CultureInfo.InvariantCulture);

                // Armazenamos em cache por 6 horas - prazo ajustável conforme necessidade
                _cache.Set(cacheKey, freteValue, TimeSpan.FromHours(6));

                return freteValue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao comunicar com a API Frenet");
                return CalcularFreteFallback(parametros);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao processar resposta da API Frenet");
                return CalcularFreteFallback(parametros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete");
                return CalcularFreteFallback(parametros);
            }
        }

        /// <summary>
        /// Método de fallback que calcula um valor aproximado de frete quando a API falha
        /// </summary>
        private decimal CalcularFreteFallback(ParametrosFrete parametros)
        {
            _logger.LogWarning("Usando cálculo de frete fallback");

            // Cálculo simplificado baseado no peso total
            decimal pesoTotal = parametros.Pacotes.Sum(p => p.Peso * p.Quantidade);

            // Valores base para um cálculo aproximado
            decimal valorBase = 15.00m;      // Taxa fixa
            decimal valorPorKg = 2.50m;      // Taxa por kg

            // Cálculo final
            return Math.Round(valorBase + (pesoTotal * valorPorKg), 2);
        }

        /// <summary>
        /// Gera uma chave única para o cache baseada nos parâmetros de frete
        /// </summary>
        private string GerarChaveCache(ParametrosFrete parametros)
        {
            // Criamos uma string que representa unicamente os pacotes
            string pacotesHash = string.Join("_",
                parametros.Pacotes.Select(p =>
                    $"{p.Altura}x{p.Largura}x{p.Comprimento}x{p.Peso}x{p.Quantidade}"));

            // Combinamos com os outros parâmetros para criar uma chave única
            return $"frete_{parametros.CepOrigem}_{parametros.CepDestino}_{parametros.ValorDeclarado}_{pacotesHash}";
        }
    }

    /// <summary>
    /// Classe para deserialização da resposta da Frenet API
    /// </summary>
    public class FrenetResponse
    {
        public List<FrenetShippingService> ShippingSevicesArray { get; set; }
        public int Timeout { get; set; }
    }

    /// <summary>
    /// Representa uma opção de serviço de frete na resposta da Frenet
    /// </summary>
    public class FrenetShippingService
    {
        public string Carrier { get; set; }            // Transportadora (ex: Correios)
        public string CarrierCode { get; set; }        // Código da transportadora
        public string DeliveryTime { get; set; }       // Prazo de entrega em dias
        public string Msg { get; set; }                // Mensagem (se houver erro)
        public string ServiceCode { get; set; }        // Código do serviço (ex: 40010 = SEDEX)
        public string ServiceDescription { get; set; } // Descrição do serviço (ex: SEDEX)
        public string ShippingPrice { get; set; }      // Preço do frete
        public string OriginalDeliveryTime { get; set; } // Prazo original antes de ajustes
        public string OriginalShippingPrice { get; set; } // Preço original antes de ajustes
        public bool Error { get; set; }                // Indica se houve erro na cotação
    }
}