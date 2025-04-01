namespace FreteManager.Models
{
    /// <summary>
    /// Classes para manipulação de dados de frete
    /// </summary>
    public class FreteModels
    {
        /// <summary>
        /// Representa um pacote individual para cálculo de frete
        /// </summary>
        public class PacoteFrete
        {
            /// <summary>
            /// Altura do pacote em centímetros
            /// </summary>
            /// <example>20</example>
            public decimal Altura { get; set; }

            /// <summary>
            /// Largura do pacote em centímetros
            /// </summary>
            /// <example>30</example>
            public decimal Largura { get; set; }

            /// <summary>
            /// Comprimento do pacote em centímetros
            /// </summary>
            /// <example>40</example>
            public decimal Comprimento { get; set; }

            /// <summary>
            /// Peso do pacote em quilogramas
            /// </summary>
            /// <example>5.0</example>
            public decimal Peso { get; set; }

            /// <summary>
            /// Quantidade de pacotes com essas mesmas dimensões
            /// </summary>
            /// <example>1</example>
            public int Quantidade { get; set; } = 1;
        }

        /// <summary>
        /// Parâmetros para cálculo de frete junto à API de transportadoras
        /// </summary>
        public class ParametrosFrete
        {
            /// <summary>
            /// CEP de origem da mercadoria (apenas números ou formatado)
            /// </summary>
            /// <example>01000000</example>
            public string CepOrigem { get; set; }

            /// <summary>
            /// CEP de destino da mercadoria (apenas números ou formatado)
            /// </summary>
            /// <example>02000000</example>
            public string CepDestino { get; set; }

            /// <summary>
            /// Valor declarado da mercadoria para fins de cálculo de frete e seguro
            /// </summary>
            /// <example>1000.00</example>
            public decimal ValorDeclarado { get; set; }

            /// <summary>
            /// Lista de pacotes para envio
            /// </summary>
            public List<PacoteFrete> Pacotes { get; set; } = new List<PacoteFrete>();
        }
    }
}
