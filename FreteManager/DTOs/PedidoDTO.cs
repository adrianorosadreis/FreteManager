using FreteManager.Models;
using System.ComponentModel.DataAnnotations;

namespace FreteManager.DTOs
{
    /// <summary>
    /// DTO para receber os dados na criação de pedido
    /// </summary>
    public class CriarPedidoDTO
    {
        /// <summary>
        /// Identificador do cliente associado ao pedido
        /// </summary>
        /// <example>1</example>
        [Required]
        public int ClienteId { get; set; }

        /// <summary>
        /// CEP ou endereço de origem do pedido
        /// </summary>
        /// <example>01000-000</example>
        [Required]
        [StringLength(200)]
        public string Origem { get; set; }

        /// <summary>
        /// CEP ou endereço de destino do pedido
        /// </summary>
        /// <example>02000-000</example>
        [Required]
        [StringLength(200)]
        public string Destino { get; set; }

        /// <summary>
        /// Status inicial do pedido. Se não informado, será definido como "Em Processamento"
        /// </summary>
        /// <example>1</example>
        public StatusPedido? Status { get; set; }

        /// <summary>
        /// Valor do frete. Se não informado, será calculado automaticamente
        /// </summary>
        /// <example>50.00</example>
        public decimal? ValorFrete { get; set; }

        /// <summary>
        /// Valor declarado da mercadoria para fins de cálculo de frete e seguro
        /// </summary>
        /// <example>1000.00</example>
        [Range(0, double.MaxValue)]
        public decimal ValorDeclarado { get; set; } = 100.00m;

        /// <summary>
        /// Lista de pacotes associados ao pedido
        /// </summary>
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }

    /// <summary>
    /// DTO para receber os dados na atualização de pedido
    /// </summary>
    public class AtualizarPedidoDTO
    {
        /// <summary>
        /// Identificador único do pedido
        /// </summary>
        /// <example>1</example>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do cliente associado ao pedido
        /// </summary>
        /// <example>1</example>
        [Required]
        public int ClienteId { get; set; }

        /// <summary>
        /// CEP ou endereço de origem do pedido
        /// </summary>
        /// <example>01000-000</example>
        [Required]
        [StringLength(200)]
        public string Origem { get; set; }

        /// <summary>
        /// CEP ou endereço de destino do pedido
        /// </summary>
        /// <example>02000-000</example>
        [Required]
        [StringLength(200)]
        public string Destino { get; set; }

        /// <summary>
        /// Status atual do pedido
        /// </summary>
        /// <example>2</example>
        [Required]
        public StatusPedido Status { get; set; }

        /// <summary>
        /// Valor do frete. Se não informado, será recalculado se necessário
        /// </summary>
        /// <example>50.00</example>
        public decimal? ValorFrete { get; set; }

        /// <summary>
        /// Valor declarado da mercadoria para fins de cálculo de frete e seguro
        /// </summary>
        /// <example>1000.00</example>
        [Range(0, double.MaxValue)]
        public decimal ValorDeclarado { get; set; } = 100.00m;

        /// <summary>
        /// Lista de pacotes associados ao pedido
        /// </summary>
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }

    /// <summary>
    /// DTO para representar um pacote sem referência circular
    /// </summary>
    public class PacoteDTO
    {
        /// <summary>
        /// Identificador único do pacote. Opcional, só informado em atualizações
        /// </summary>
        /// <example>1</example>
        public int? Id { get; set; }

        /// <summary>
        /// Altura do pacote em centímetros
        /// </summary>
        /// <example>20</example>
        [Range(0.1, double.MaxValue)]
        public decimal Altura { get; set; }

        /// <summary>
        /// Largura do pacote em centímetros
        /// </summary>
        /// <example>30</example>
        [Range(0.1, double.MaxValue)]
        public decimal Largura { get; set; }

        /// <summary>
        /// Comprimento do pacote em centímetros
        /// </summary>
        /// <example>40</example>
        [Range(0.1, double.MaxValue)]
        public decimal Comprimento { get; set; }

        /// <summary>
        /// Peso do pacote em quilogramas
        /// </summary>
        /// <example>5.0</example>
        [Range(0.1, double.MaxValue)]
        public decimal Peso { get; set; }

        /// <summary>
        /// Quantidade de pacotes com essas mesmas dimensões
        /// </summary>
        /// <example>1</example>
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; } = 1;
    }

    /// <summary>
    /// DTO para retornar dados completos de um pedido
    /// </summary>
    public class PedidoRespostaDTO
    {
        /// <summary>
        /// Identificador único do pedido
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Identificador do cliente associado ao pedido
        /// </summary>
        /// <example>1</example>
        public int ClienteId { get; set; }

        /// <summary>
        /// Nome do cliente associado ao pedido
        /// </summary>
        /// <example>Empresa ABC</example>
        public string ClienteNome { get; set; }

        /// <summary>
        /// CEP ou endereço de origem do pedido
        /// </summary>
        /// <example>01000-000</example>
        public string Origem { get; set; }

        /// <summary>
        /// CEP ou endereço de destino do pedido
        /// </summary>
        /// <example>02000-000</example>
        public string Destino { get; set; }

        /// <summary>
        /// Data e hora de criação do pedido
        /// </summary>
        /// <example>2025-03-31T14:30:00</example>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Status atual do pedido
        /// </summary>
        /// <example>2</example>
        public StatusPedido Status { get; set; }

        /// <summary>
        /// Descrição textual do status atual do pedido
        /// </summary>
        /// <example>Enviado</example>
        public string StatusDescricao { get; set; }

        /// <summary>
        /// Valor do frete em reais
        /// </summary>
        /// <example>50.00</example>
        public decimal? ValorFrete { get; set; }

        /// <summary>
        /// Valor declarado da mercadoria para fins de cálculo de frete e seguro
        /// </summary>
        /// <example>1000.00</example>
        public decimal ValorDeclarado { get; set; }

        /// <summary>
        /// Lista de pacotes associados ao pedido
        /// </summary>
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }
}