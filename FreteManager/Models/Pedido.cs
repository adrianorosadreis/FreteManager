using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreteManager.Models
{
    /// <summary>
    /// Enumerar os possíveis status de um pedido
    /// </summary>
    public enum StatusPedido
    {
        /// <summary>
        /// Pedido criado e em fase de processamento inicial
        /// </summary>
        EmProcessamento = 1,

        /// <summary>
        /// Pedido despachado para entrega
        /// </summary>
        Enviado = 2,

        /// <summary>
        /// Pedido entregue ao destinatário
        /// </summary>
        Entregue = 3,

        /// <summary>
        /// Pedido cancelado (não será entregue)
        /// </summary>
        Cancelado = 4
    }

    /// <summary>
    /// Modelo que representa um pedido de transporte
    /// </summary>
    public class Pedido
    {
        /// <summary>
        /// Identificador único do pedido
        /// </summary>
        /// <example>1</example>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do cliente associado ao pedido
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int ClienteId { get; set; }

        /// <summary>
        /// Referência para o objeto Cliente (usado pelo Entity Framework)
        /// </summary>
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        /// <summary>
        /// CEP ou endereço de origem do pedido
        /// </summary>
        /// <example>01000-000</example>
        [Required(ErrorMessage = "Origem é obrigatória")]
        [StringLength(200, ErrorMessage = "Origem muito longa")]
        public string Origem { get; set; }

        /// <summary>
        /// CEP ou endereço de destino do pedido
        /// </summary>
        /// <example>02000-000</example>
        [Required(ErrorMessage = "Destino é obrigatório")]
        [StringLength(200, ErrorMessage = "Destino muito longo")]
        public string Destino { get; set; }

        /// <summary>
        /// Data e hora de criação do pedido
        /// </summary>
        /// <example>2025-03-31T14:30:00</example>
        [Required(ErrorMessage = "Data de criação é obrigatória")]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Status atual do pedido
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "Status do pedido é obrigatório")]
        public StatusPedido Status { get; set; }

        /// <summary>
        /// Valor calculado do frete em reais
        /// </summary>
        /// <example>50.00</example>
        [Range(0, double.MaxValue, ErrorMessage = "Valor do frete deve ser positivo")]
        public decimal? ValorFrete { get; set; }

        /// <summary>
        /// Valor declarado da mercadoria para fins de cálculo de frete e seguro
        /// </summary>
        /// <example>1000.00</example>
        [Range(0, double.MaxValue)]
        public decimal ValorDeclarado { get; set; } = 100.00m;

        /// <summary>
        /// Coleção de pacotes associados ao pedido
        /// </summary>
        public virtual ICollection<Pacote> Pacotes { get; set; } = new List<Pacote>();
    }
}