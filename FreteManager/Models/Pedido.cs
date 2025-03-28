using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FreteManager.Models
{
    // Enum para representar os status do pedido
    public enum StatusPedido
    {
        EmProcessamento = 1,
        Enviado = 2,
        Entregue = 3,
        Cancelado = 4
    }

    public class Pedido
    {
        // Chave primária
        [Key]
        public int Id { get; set; }

        // Relacionamento com Cliente
        [Required(ErrorMessage = "Cliente é obrigatório")]
        public int ClienteId { get; set; }

        // Navegação para o Cliente (para Entity Framework)
        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        // Origem do pedido
        [Required(ErrorMessage = "Origem é obrigatória")]
        [StringLength(200, ErrorMessage = "Origem muito longa")]
        public string Origem { get; set; }

        // Destino do pedido
        [Required(ErrorMessage = "Destino é obrigatório")]
        [StringLength(200, ErrorMessage = "Destino muito longo")]
        public string Destino { get; set; }

        // Data de criação do pedido
        [Required(ErrorMessage = "Data de criação é obrigatória")]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; }

        // Status do pedido
        [Required(ErrorMessage = "Status do pedido é obrigatório")]
        public StatusPedido Status { get; set; }

        // Valor do frete (opcional)
        [Range(0, double.MaxValue, ErrorMessage = "Valor do frete deve ser positivo")]
        public decimal? ValorFrete { get; set; }
    }
}