using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Controllers
{
    /// <summary>
    /// Controlador para operações de cálculo de frete
    /// </summary>
    [Route("v1/[controller]")]
    [ApiController]
    public class FreteController : ControllerBase
    {
        private readonly IFreteService _freteService;
        private readonly ILogger<FreteController> _logger;

        public FreteController(IFreteService freteService, ILogger<FreteController> logger)
        {
            _freteService = freteService;
            _logger = logger;
        }

        /// <summary>
        /// Calcula o frete usando parâmetros detalhados como dimensões e peso
        /// </summary>
        /// <param name="parametros">Parâmetros completos para cálculo de frete</param>
        /// <returns>Valor do frete em reais</returns>
        [HttpPost("calcular-frete")]
        [Authorize]
        public async Task<ActionResult<decimal>> CalcularFrete(
            [FromBody] ParametrosFrete parametros)
        {
            _logger.LogInformation($"Requisição POST para calcular frete de {parametros.CepOrigem} para {parametros.CepDestino}");

            if (parametros == null)
            {
                _logger.LogWarning("Parâmetros de frete nulos");
                return BadRequest("Parâmetros inválidos");
            }

            if (string.IsNullOrWhiteSpace(parametros.CepOrigem) ||
                string.IsNullOrWhiteSpace(parametros.CepDestino))
            {
                _logger.LogWarning("CEP de origem ou destino não informado");
                return BadRequest("CEP de origem e destino são obrigatórios");
            }

            if (parametros.Pacotes == null || !parametros.Pacotes.Any())
            {
                _logger.LogWarning("Nenhum pacote informado");
                return BadRequest("É necessário informar pelo menos um pacote");
            }

            var valorFrete = await _freteService.CalcularFreteDetalhadoAsync(parametros);

            _logger.LogInformation(
                $"Frete calculado de {parametros.CepOrigem} para {parametros.CepDestino}: {valorFrete:C}");

            return Ok(new
            {
                valorFrete,
                cepOrigem = parametros.CepOrigem,
                cepDestino = parametros.CepDestino,
                valorDeclarado = parametros.ValorDeclarado,
                quantidadePacotes = parametros.Pacotes.Sum(p => p.Quantidade)
            });
        }
    }
}
