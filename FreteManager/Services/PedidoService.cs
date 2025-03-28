using FreteManager.Models;
using FreteManager.Repositories;
using FreteManager.Services;
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

            // Definir data de criação como data atual
            pedido.DataCriacao = DateTime.Now;

            // Definir status inicial como EmProcessamento
            pedido.Status = StatusPedido.EmProcessamento;

            // Calcular o frete automaticamente
            pedido.ValorFrete = await CalcularFreteAsync(pedido.Origem, pedido.Destino);

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

            // Recalcular o frete se a origem ou destino foram alterados
            if (pedido.Origem != pedidoExistente.Origem || pedido.Destino != pedidoExistente.Destino)
            {
                pedido.ValorFrete = await CalcularFreteParaPedidoAsync(pedido.Origem, pedido.Destino);
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
            // Aqui poderíamos extrair mais informações do pedido, como peso dos itens
            var parametros = new ParametrosFrete
            {
                CepOrigem = pedido.Origem,
                CepDestino = pedido.Destino,
                ValorDeclarado = 100.00m, // Idealmente seria o valor total do pedido
                Pacotes = new List<PacoteFrete>
            {
                new PacoteFrete
                {
                    Altura = 10,
                    Largura = 15,
                    Comprimento = 20,
                    Peso = 1.0m,
                    Quantidade = 1
                }
                // Em um sistema real, criaríamos pacotes baseados nos itens do pedido
            }
            };

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