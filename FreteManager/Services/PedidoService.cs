using FreteManager.DTOs;
using FreteManager.Helpers;
using FreteManager.Models;
using FreteManager.Repositories;
using static FreteManager.Models.FreteModels;

namespace FreteManager.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IClienteService _clienteService;
        private readonly IFreteService _freteService;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(IPedidoRepository pedidoRepository,
                             IClienteService clienteService,
                             IFreteService freteService,
                             ILogger<PedidoService> logger)
        {
            _pedidoRepository = pedidoRepository;
            _clienteService = clienteService;
            _freteService = freteService;
            _logger = logger;
        }

        public async Task<PedidoRespostaDTO> ObterPorIdAsync(int id)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado.");
            }
            return PedidoMapper.ParaDTO(pedido);
        }

        public async Task<IEnumerable<PedidoRespostaDTO>> ListarTodosAsync()
        {
            var pedidos = await _pedidoRepository.ListarTodosAsync();
            return pedidos.Select(p => PedidoMapper.ParaDTO(p));
        }

        public async Task<PedidoRespostaDTO> CriarAsync(CriarPedidoDTO pedidoDTO)
        {
            // Verificar se o cliente existe
            if (!await _clienteService.ClienteExisteAsync(pedidoDTO.ClienteId))
            {
                throw new KeyNotFoundException($"Cliente com ID {pedidoDTO.ClienteId} não encontrado.");
            }

            // Converter DTO para entidade
            var pedido = PedidoMapper.ParaPedido(pedidoDTO);

            // Calcular o frete se não foi informado
            if (!pedido.ValorFrete.HasValue || pedido.ValorFrete <= 0)
            {
                pedido.ValorFrete = await CalcularFreteParaPedidoAsync(pedido);
            }

            // Salvar o pedido
            var pedidoCriado = await _pedidoRepository.AdicionarAsync(pedido);

            // Retornar DTO de resposta
            return PedidoMapper.ParaDTO(pedidoCriado);
        }

        public async Task<PedidoRespostaDTO> AtualizarAsync(AtualizarPedidoDTO pedidoDTO)
        {
            // Verificar se o pedido existe
            var pedidoExistente = await _pedidoRepository.ObterPorIdAsync(pedidoDTO.Id);
            if (pedidoExistente == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {pedidoDTO.Id} não encontrado.");
            }

            // Verificar se o cliente existe
            if (!await _clienteService.ClienteExisteAsync(pedidoDTO.ClienteId))
            {
                throw new KeyNotFoundException($"Cliente com ID {pedidoDTO.ClienteId} não encontrado.");
            }

            // Atualizar a entidade com os dados do DTO
            PedidoMapper.AtualizarPedido(pedidoExistente, pedidoDTO);

            // Recalcular o frete se necessário (origem/destino alterados)
            bool devemosRecalcular =
                (pedidoDTO.Origem != pedidoExistente.Origem || pedidoDTO.Destino != pedidoExistente.Destino)
                || (!pedidoDTO.ValorFrete.HasValue || pedidoDTO.ValorFrete <= 0);

            if (devemosRecalcular)
            {
                pedidoExistente.ValorFrete = await CalcularFreteParaPedidoAsync(pedidoExistente);
            }

            // Salvar as alterações
            await _pedidoRepository.AtualizarAsync(pedidoExistente);

            // Recarregar o pedido para garantir que temos todos os dados atualizados
            var pedidoAtualizado = await _pedidoRepository.ObterPorIdAsync(pedidoDTO.Id);

            // Retornar DTO de resposta
            return PedidoMapper.ParaDTO(pedidoAtualizado);
        }

        public async Task ExcluirAsync(int id)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado.");
            }

            await _pedidoRepository.ExcluirAsync(id);
        }

        public async Task<IEnumerable<Pedido>> ListarPorClienteAsync(int clienteId)
        {
            // Verificar se o cliente existe
            if (!await _clienteService.ClienteExisteAsync(clienteId))
            {
                throw new KeyNotFoundException($"Cliente com ID {clienteId} não encontrado.");
            }

            return await _pedidoRepository.ListarPorClienteAsync(clienteId);
        }

        /// <summary>
        /// Calcula o frete para um pedido específico, considerando suas características
        /// </summary>
        public async Task<decimal> CalcularFreteParaPedidoAsync(Pedido pedido)
        {
            // Se não houver pacotes definidos, criar um pacote padrão
            if (pedido.Pacotes == null || !pedido.Pacotes.Any())
            {
                _logger.LogInformation("Nenhum pacote definido para o pedido. Usando valores padrão.");
            }

            // Converter os pacotes do pedido para o formato esperado pelo serviço de frete
            var pacotes = pedido.Pacotes?.Select(p => new FreteModels.PacoteFrete
            {
                Altura = p.Altura,
                Largura = p.Largura,
                Comprimento = p.Comprimento,
                Peso = p.Peso,
                Quantidade = p.Quantidade
            }).ToList() ?? new List<FreteModels.PacoteFrete>
            {
                new FreteModels.PacoteFrete
                {
                    Altura = 10,
                    Largura = 15,
                    Comprimento = 20,
                    Peso = 1.0m,
                    Quantidade = 1
                }
            };

            // Criar os parâmetros para o cálculo de frete
            var parametros = new FreteModels.ParametrosFrete
            {
                CepOrigem = pedido.Origem,
                CepDestino = pedido.Destino,
                ValorDeclarado = pedido.ValorDeclarado,
                Pacotes = pacotes
            };

            // Calcular o frete
            return await _freteService.CalcularFreteDetalhadoAsync(parametros);
        }

        public async Task<Pedido> AtualizarStatusAsync(int id, StatusPedido novoStatus)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado.");
            }

            // Validar a transição de status
            ValidarTransicaoStatus(pedido.Status, novoStatus);

            pedido.Status = novoStatus;
            await _pedidoRepository.AtualizarAsync(pedido);

            _logger.LogInformation($"Status do pedido {id} atualizado de {pedido.Status} para {novoStatus}");

            return pedido;
        }

        private void ValidarTransicaoStatus(StatusPedido statusAtual, StatusPedido novoStatus)
        {
            // Regras de transição de status
            switch (statusAtual)
            {
                case StatusPedido.EmProcessamento:
                    // De EmProcessamento só pode ir para Enviado ou Cancelado
                    if (novoStatus != StatusPedido.Enviado && novoStatus != StatusPedido.Cancelado)
                    {
                        throw new InvalidOperationException($"Não é possível alterar o status de {statusAtual} para {novoStatus}");
                    }
                    break;
                case StatusPedido.Enviado:
                    // De Enviado só pode ir para Entregue
                    if (novoStatus != StatusPedido.Entregue)
                    {
                        throw new InvalidOperationException($"Não é possível alterar o status de {statusAtual} para {novoStatus}");
                    }
                    break;
                case StatusPedido.Entregue:
                    // Pedidos entregues não podem mais mudar de status
                    throw new InvalidOperationException("Não é possível alterar o status de um pedido já entregue");

                case StatusPedido.Cancelado:
                    // Pedidos cancelados não podem mais mudar de status
                    throw new InvalidOperationException("Não é possível alterar o status de um pedido cancelado");
            }
        }
    }
}