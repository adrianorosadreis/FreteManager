using FreteManager.Models;
using System.ComponentModel.DataAnnotations;

namespace FreteManager.DTOs
{
    // DTO para receber os dados na criação de pedido
    public class CriarPedidoDTO
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        [StringLength(200)]
        public string Origem { get; set; }

        [Required]
        [StringLength(200)]
        public string Destino { get; set; }

        // Status é opcional na criação, será definido como padrão se não for fornecido
        public StatusPedido? Status { get; set; }

        // ValorFrete é opcional, será calculado automaticamente se não for fornecido
        public decimal? ValorFrete { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValorDeclarado { get; set; } = 100.00m;

        // Lista de pacotes associados ao pedido
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }

    // DTO para receber os dados na atualização de pedido
    public class AtualizarPedidoDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        [StringLength(200)]
        public string Origem { get; set; }

        [Required]
        [StringLength(200)]
        public string Destino { get; set; }

        [Required]
        public StatusPedido Status { get; set; }

        // ValorFrete é opcional, será recalculado se necessário
        public decimal? ValorFrete { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ValorDeclarado { get; set; } = 100.00m;

        // Lista de pacotes associados ao pedido
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }

    // DTO para representar um pacote sem referência circular
    public class PacoteDTO
    {
        // Id é opcional, só informado em atualizações
        public int? Id { get; set; }

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

    // DTO para retornar dados completos de um pedido
    public class PedidoRespostaDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public string Origem { get; set; }
        public string Destino { get; set; }
        public DateTime DataCriacao { get; set; }
        public StatusPedido Status { get; set; }
        public string StatusDescricao { get; set; }
        public decimal? ValorFrete { get; set; }
        public decimal ValorDeclarado { get; set; }
        public List<PacoteDTO> Pacotes { get; set; } = new List<PacoteDTO>();
    }
}