using FreteManager.DTOs;
using FreteManager.Models;

namespace FreteManager.Helpers
{
    public static class PedidoMapper
    {
        // Converte um DTO de criação para uma entidade Pedido
        public static Pedido ParaPedido(CriarPedidoDTO dto)
        {
            var pedido = new Pedido
            {
                ClienteId = dto.ClienteId,
                Origem = dto.Origem,
                Destino = dto.Destino,
                DataCriacao = DateTime.Now,
                Status = dto.Status ?? StatusPedido.EmProcessamento,
                ValorFrete = dto.ValorFrete,
                ValorDeclarado = dto.ValorDeclarado,
                Pacotes = dto.Pacotes?.Select(p => new Pacote
                {
                    Altura = p.Altura,
                    Largura = p.Largura,
                    Comprimento = p.Comprimento,
                    Peso = p.Peso,
                    Quantidade = p.Quantidade
                }).ToList() ?? new List<Pacote>()
            };

            return pedido;
        }

        // Atualiza uma entidade Pedido com os dados de um DTO de atualização
        public static void AtualizarPedido(Pedido pedido, AtualizarPedidoDTO dto)
        {
            pedido.ClienteId = dto.ClienteId;
            pedido.Origem = dto.Origem;
            pedido.Destino = dto.Destino;
            pedido.Status = dto.Status;
            pedido.ValorFrete = dto.ValorFrete;
            pedido.ValorDeclarado = dto.ValorDeclarado;

            // Aqui, a lógica de atualização de pacotes pode ser mais complexa
            // Precisamos identificar quais pacotes foram adicionados, atualizados ou removidos
        }

        // Converte uma entidade Pedido para um DTO de resposta
        public static PedidoRespostaDTO ParaDTO(Pedido pedido)
        {
            return new PedidoRespostaDTO
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                ClienteNome = pedido.Cliente?.Nome ?? "Cliente não encontrado",
                Origem = pedido.Origem,
                Destino = pedido.Destino,
                DataCriacao = pedido.DataCriacao,
                Status = pedido.Status,
                StatusDescricao = pedido.Status.ToString(),
                ValorFrete = pedido.ValorFrete,
                ValorDeclarado = pedido.ValorDeclarado,
                Pacotes = pedido.Pacotes?.Select(p => new PacoteDTO
                {
                    Id = p.Id,
                    Altura = p.Altura,
                    Largura = p.Largura,
                    Comprimento = p.Comprimento,
                    Peso = p.Peso,
                    Quantidade = p.Quantidade
                }).ToList() ?? new List<PacoteDTO>()
            };
        }
    }
}