using FreteManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Controllers
{
    /// <summary>
    /// Controlador para operações de cálculo de frete
    /// </summary>
    [Route("api/[controller]")]
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
            try
            {
                if (parametros == null)
                {
                    return BadRequest("Parâmetros inválidos");
                }

                if (string.IsNullOrWhiteSpace(parametros.CepOrigem) ||
                    string.IsNullOrWhiteSpace(parametros.CepDestino))
                {
                    return BadRequest("CEP de origem e destino são obrigatórios");
                }

                if (parametros.Pacotes == null || !parametros.Pacotes.Any())
                {
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação no cálculo de frete");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete");
                return StatusCode(500, "Erro interno ao calcular o frete");
            }
        }
    }
}
