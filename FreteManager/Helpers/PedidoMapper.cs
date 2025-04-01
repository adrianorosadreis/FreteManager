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
            // Atualizar propriedades básicas do pedido
            pedido.ClienteId = dto.ClienteId;
            pedido.Origem = dto.Origem;
            pedido.Destino = dto.Destino;
            pedido.Status = dto.Status;
            pedido.ValorFrete = dto.ValorFrete;
            pedido.ValorDeclarado = dto.ValorDeclarado;

            // Tratamento dos pacotes
            if (dto.Pacotes != null)
            {
                // Dicionário para facilitar a busca de pacotes existentes pelo ID
                var pacotesExistentes = pedido.Pacotes.ToDictionary(p => p.Id);

                // Lista para armazenar os pacotes que devem ser mantidos
                var pacotesAtualizados = new List<Pacote>();

                foreach (var pacoteDto in dto.Pacotes)
                {
                    if (pacoteDto.Id.HasValue && pacotesExistentes.TryGetValue(pacoteDto.Id.Value, out var pacoteExistente))
                    {
                        // Atualizar pacote existente
                        pacoteExistente.Altura = pacoteDto.Altura;
                        pacoteExistente.Largura = pacoteDto.Largura;
                        pacoteExistente.Comprimento = pacoteDto.Comprimento;
                        pacoteExistente.Peso = pacoteDto.Peso;
                        pacoteExistente.Quantidade = pacoteDto.Quantidade;

                        pacotesAtualizados.Add(pacoteExistente);
                        pacotesExistentes.Remove(pacoteDto.Id.Value); // Remove do dicionário para marcar como processado
                    }
                    else
                    {
                        // Adicionar novo pacote
                        var novoPacote = new Pacote
                        {
                            PedidoId = pedido.Id,
                            Altura = pacoteDto.Altura,
                            Largura = pacoteDto.Largura,
                            Comprimento = pacoteDto.Comprimento,
                            Peso = pacoteDto.Peso,
                            Quantidade = pacoteDto.Quantidade
                        };

                        pacotesAtualizados.Add(novoPacote);
                    }
                }

                // Substitui a coleção de pacotes pelos pacotes atualizados
                // Os pacotes que não estavam no DTO serão removidos automaticamente pelo Entity Framework
                pedido.Pacotes.Clear();
                foreach (var pacote in pacotesAtualizados)
                {
                    pedido.Pacotes.Add(pacote);
                }
            }
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