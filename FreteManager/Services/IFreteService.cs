using static FreteManager.Models.FreteModels;

namespace FreteManager.Services
{
    public interface IFreteService
    {
        /// <summary>
        /// Calcula o frete com informações detalhadas de dimensões, peso e valor
        /// </summary>
        /// <param name="parametros">Parâmetros completos para cálculo</param>
        /// <returns>Valor do frete em reais</returns>
        Task<decimal> CalcularFreteDetalhadoAsync(ParametrosFrete parametros);
    }
}