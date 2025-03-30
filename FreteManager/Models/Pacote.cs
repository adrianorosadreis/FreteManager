using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FreteManager.Models
{
    public class Pacote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido Pedido { get; set; }

        [Range(0.1, double.MaxValue)]
        public decimal Altura { get; set; }

        [Range(0.1, double.MaxValue)]
        public decimal Largura { get; set; }

        [Range(0.1, double.MaxValue)]
        public decimal Comprimento { get; set; }

        [Range(0.1, double.MaxValue)]
        public decimal Peso { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; } = 1;
    }
}
