using FreteManager.DTOs;
using FreteManager.Exceptions;
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
            _logger.LogInformation($"Obtendo detalhes do pedido ID {id}");

            // Repositório já lançará EntityNotFoundException se não encontrar
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);

            var dto = PedidoMapper.ParaDTO(pedido);
            _logger.LogDebug($"Pedido ID {id} convertido para DTO com sucesso");

            return dto;
        }

        public async Task<IEnumerable<PedidoRespostaDTO>> ListarTodosAsync()
        {
            _logger.LogInformation("Listando todos os pedidos");

            var pedidos = await _pedidoRepository.ListarTodosAsync();
            var dtos = pedidos.Select(p => PedidoMapper.ParaDTO(p)).ToList();

            _logger.LogDebug($"Convertidos {dtos.Count} pedidos para DTOs");

            return dtos;
        }

        public async Task<PedidoRespostaDTO> CriarAsync(CriarPedidoDTO pedidoDTO)
        {
            _logger.LogInformation($"Iniciando criação de pedido para cliente ID {pedidoDTO.ClienteId}");

            // Verificar se o cliente existe
            var clienteExiste = await _clienteService.ClienteExisteAsync(pedidoDTO.ClienteId);
            if (!clienteExiste)
            {
                _logger.LogWarning($"Tentativa de criar pedido para cliente inexistente ID {pedidoDTO.ClienteId}");
                throw new EntityNotFoundException("Cliente", pedidoDTO.ClienteId);
            }

            _logger.LogDebug("Cliente validado. Convertendo DTO para entidade");

            // Converter DTO para entidade
            var pedido = PedidoMapper.ParaPedido(pedidoDTO);

            // Calcular o frete se não foi informado
            if (!pedido.ValorFrete.HasValue || pedido.ValorFrete <= 0)
            {
                _logger.LogDebug("Calculando valor de frete para o pedido");
                pedido.ValorFrete = await CalcularFreteParaPedidoAsync(pedido);
                _logger.LogInformation($"Valor de frete calculado: {pedido.ValorFrete:C}");
            }

            // Salvar o pedido
            _logger.LogDebug("Salvando pedido no banco de dados");
            var pedidoCriado = await _pedidoRepository.AdicionarAsync(pedido);
            _logger.LogInformation($"Pedido criado com sucesso, ID {pedidoCriado.Id}");

            // Retornar DTO de resposta
            var dto = PedidoMapper.ParaDTO(pedidoCriado);
            return dto;
        }

        public async Task<PedidoRespostaDTO> AtualizarAsync(AtualizarPedidoDTO pedidoDTO)
        {
            _logger.LogInformation($"Iniciando atualização do pedido ID {pedidoDTO.Id}");

            // Verificar se o pedido existe - o repositório já lançará exceção se não encontrar
            var pedidoExistente = await _pedidoRepository.ObterPorIdAsync(pedidoDTO.Id);

            // Verificar se o cliente existe
            var clienteExiste = await _clienteService.ClienteExisteAsync(pedidoDTO.ClienteId);
            if (!clienteExiste)
            {
                _logger.LogWarning($"Tentativa de atualizar pedido com cliente inexistente ID {pedidoDTO.ClienteId}");
                throw new EntityNotFoundException("Cliente", pedidoDTO.ClienteId);
            }

            _logger.LogDebug("Cliente validado. Atualizando entidade com dados do DTO");

            // Atualizar a entidade com os dados do DTO
            PedidoMapper.AtualizarPedido(pedidoExistente, pedidoDTO);

            // Recalcular o frete se necessário (origem/destino alterados)
            bool devemosRecalcular =
                (pedidoDTO.Origem != pedidoExistente.Origem || pedidoDTO.Destino != pedidoExistente.Destino)
                || (!pedidoDTO.ValorFrete.HasValue || pedidoDTO.ValorFrete <= 0);

            if (devemosRecalcular)
            {
                _logger.LogDebug("Recalculando frete devido a alterações na origem, destino ou valor");
                pedidoExistente.ValorFrete = await CalcularFreteParaPedidoAsync(pedidoExistente);
                _logger.LogInformation($"Novo valor de frete calculado: {pedidoExistente.ValorFrete:C}");
            }

            // Salvar as alterações
            _logger.LogDebug("Salvando alterações no banco de dados");
            await _pedidoRepository.AtualizarAsync(pedidoExistente);
            _logger.LogInformation($"Pedido ID {pedidoDTO.Id} atualizado com sucesso");

            // Recarregar o pedido para garantir que temos todos os dados atualizados
            var pedidoAtualizado = await _pedidoRepository.ObterPorIdAsync(pedidoDTO.Id);

            // Retornar DTO de resposta
            var dto = PedidoMapper.ParaDTO(pedidoAtualizado);
            return dto;
        }

        public async Task ExcluirAsync(int id)
        {
            _logger.LogInformation($"Iniciando exclusão do pedido ID {id}");

            // O repositório verificará se o pedido existe e lançará exceção se não encontrar
            await _pedidoRepository.ExcluirAsync(id);

            _logger.LogInformation($"Pedido ID {id} excluído com sucesso");
        }

        public async Task<IEnumerable<PedidoRespostaDTO>> ListarPorClienteAsync(int clienteId)
        {
            _logger.LogInformation($"Listando pedidos do cliente ID {clienteId}");

            // Verificar se o cliente existe
            var clienteExiste = await _clienteService.ClienteExisteAsync(clienteId);
            if (!clienteExiste)
            {
                _logger.LogWarning($"Tentativa de listar pedidos de cliente inexistente ID {clienteId}");
                throw new EntityNotFoundException("Cliente", clienteId);
            }

            var pedidos = await _pedidoRepository.ListarPorClienteAsync(clienteId);
            var dtos = pedidos.Select(p => PedidoMapper.ParaDTO(p)).ToList();

            _logger.LogDebug($"Encontrados {dtos.Count} pedidos para o cliente ID {clienteId}");

            return dtos;
        }

        public async Task<decimal> CalcularFreteParaPedidoAsync(Pedido pedido)
        {
            _logger.LogDebug($"Calculando frete para pedido. Origem: {pedido.Origem}, Destino: {pedido.Destino}");

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

            _logger.LogDebug($"Preparados {pacotes.Count} pacotes para cálculo de frete");

            // Criar os parâmetros para o cálculo de frete
            var parametros = new FreteModels.ParametrosFrete
            {
                CepOrigem = pedido.Origem,
                CepDestino = pedido.Destino,
                ValorDeclarado = pedido.ValorDeclarado,
                Pacotes = pacotes
            };

            // Calcular o frete
            _logger.LogDebug("Enviando solicitação para o serviço de frete");
            var valorFrete = await _freteService.CalcularFreteDetalhadoAsync(parametros);
            _logger.LogInformation($"Frete calculado: {valorFrete:C}");

            return valorFrete;
        }

        public async Task<PedidoRespostaDTO> AtualizarStatusAsync(int id, StatusPedido novoStatus)
        {
            _logger.LogInformation($"Atualizando status do pedido ID {id} para {novoStatus}");

            // Obter o pedido - o repositório verificará se existe
            var pedido = await _pedidoRepository.ObterPorIdAsync(id);
            var statusAnterior = pedido.Status;

            try
            {
                // Validar a transição de status
                ValidarTransicaoStatus(pedido.Status, novoStatus);
            }
            catch (BusinessRuleViolationException)
            {
                _logger.LogWarning($"Transição de status inválida: de {pedido.Status} para {novoStatus}");
                throw; // Repassar a exceção
            }

            pedido.Status = novoStatus;
            await _pedidoRepository.AtualizarAsync(pedido);

            _logger.LogInformation($"Status do pedido {id} atualizado de {statusAnterior} para {novoStatus}");

            // Recarregar o pedido para garantir que temos todos os dados atualizados
            var pedidoAtualizado = await _pedidoRepository.ObterPorIdAsync(id);

            return PedidoMapper.ParaDTO(pedidoAtualizado);
        }

        private void ValidarTransicaoStatus(StatusPedido statusAtual, StatusPedido novoStatus)
        {
            _logger.LogDebug($"Validando transição de status: {statusAtual} -> {novoStatus}");

            // Regras de transição de status
            switch (statusAtual)
            {
                case StatusPedido.EmProcessamento:
                    // De EmProcessamento só pode ir para Enviado ou Cancelado
                    if (novoStatus != StatusPedido.Enviado && novoStatus != StatusPedido.Cancelado)
                    {
                        throw new BusinessRuleViolationException($"Não é possível alterar o status de {statusAtual} para {novoStatus}");
                    }
                    break;

                case StatusPedido.Enviado:
                    // De Enviado só pode ir para Entregue
                    if (novoStatus != StatusPedido.Entregue)
                    {
                        throw new BusinessRuleViolationException($"Não é possível alterar o status de {statusAtual} para {novoStatus}");
                    }
                    break;

                case StatusPedido.Entregue:
                    // Pedidos entregues não podem mais mudar de status
                    throw new BusinessRuleViolationException("Não é possível alterar o status de um pedido já entregue");

                case StatusPedido.Cancelado:
                    // Pedidos cancelados não podem mais mudar de status
                    throw new BusinessRuleViolationException("Não é possível alterar o status de um pedido cancelado");
            }

            _logger.LogDebug("Transição de status validada com sucesso");
        }
    }
}