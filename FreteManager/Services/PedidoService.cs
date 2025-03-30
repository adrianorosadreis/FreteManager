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

        public async Task<Pedido> ObterPorIdAsync(int id)
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado.");
            }
            return pedido;
        }

        public async Task<IEnumerable<Pedido>> ListarTodosAsync()
        {
            return await _pedidoRepository.ListarTodosAsync();
        }

        public async Task<Pedido> CriarAsync(Pedido pedido)
        {
            // Verificar se o cliente existe
            if (!await _clienteService.ClienteExisteAsync(pedido.ClienteId))
            {
                throw new KeyNotFoundException($"Cliente com ID {pedido.ClienteId} não encontrado.");
            }

            // Definir data de criação como data atual se não foi definida
            if (pedido.DataCriacao == default)
            {
                pedido.DataCriacao = DateTime.Now;
            }

            // Definir status inicial como EmProcessamento se não foi definido
            if (pedido.Status == default)
            {
                pedido.Status = StatusPedido.EmProcessamento;
            }

            // Calcular o frete apenas se não tiver sido definido ou for zero/negativo
            if (!pedido.ValorFrete.HasValue || pedido.ValorFrete <= 0)
            {
                pedido.ValorFrete = await CalcularFreteParaPedidoAsync(pedido);
            }

            // Salvar o pedido
            return await _pedidoRepository.AdicionarAsync(pedido);
        }

        public async Task AtualizarAsync(Pedido pedido)
        {
            // Verificar se o pedido existe
            var pedidoExistente = await _pedidoRepository.ObterPorIdAsync(pedido.Id);
            if (pedidoExistente == null)
            {
                throw new KeyNotFoundException($"Pedido com ID {pedido.Id} não encontrado.");
            }

            // Verificar se o cliente existe
            if (!await _clienteService.ClienteExisteAsync(pedido.ClienteId))
            {
                throw new KeyNotFoundException($"Cliente com ID {pedido.ClienteId} não encontrado.");
            }

            // Manter a data de criação original
            pedido.DataCriacao = pedidoExistente.DataCriacao;

            // Recalcular o frete em qualquer um desses casos:
            // 1. Se a origem ou destino foram alterados E o valor do frete não foi explicitamente definido
            // 2. Se o valor do frete foi explicitamente definido como nulo ou zero/negativo
            bool devemosRecalcular =
                ((pedido.Origem != pedidoExistente.Origem || pedido.Destino != pedidoExistente.Destino)
                    && (!pedido.ValorFrete.HasValue || pedido.ValorFrete == pedidoExistente.ValorFrete))
                || (!pedido.ValorFrete.HasValue || pedido.ValorFrete <= 0);

            if (devemosRecalcular)
            {
                pedido.ValorFrete = await CalcularFreteParaPedidoAsync(pedido);
            }

            await _pedidoRepository.AtualizarAsync(pedido);
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