using Microsoft.Extensions.Logging;
using Perfumaria.Application.DTOs.Pedidos;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Mappers;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly ILogger<PedidoService> _logger;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IClienteRepository clienteRepository,
        IProdutoRepository produtoRepository,
        ILogger<PedidoService> logger)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo pedido de venda a partir do cliente e da lista de itens informados.
    /// Regras aplicadas (delegadas ao Domain): o preço unitário é obtido como snapshot
    /// do preço de venda atual do produto; o estoque de cada produto é debitado através
    /// de <see cref="Estoque.Debitar"/> e não pode ficar negativo; a quantidade e o
    /// desconto de cada item são validados por <see cref="Pedido.AdicionarItem"/>.
    /// </summary>
    public async Task<PedidoResponseDto> CriarAsync(PedidoRequestDto dto, string traceId)
    {
        _logger.LogInformation(
            "Iniciando criação de pedido. ClienteId={ClienteId} QuantidadeItens={QuantidadeItens} TraceId={TraceId}",
            dto.ClienteId, dto.Itens.Count, traceId);

        var cliente = await _clienteRepository.ObterPorIdAsync(dto.ClienteId)
            ?? throw new ResourceNotFoundException(nameof(Cliente), dto.ClienteId);

        var pedido = new Pedido
        {
            NumeroPedido = $"PED-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            ClienteId = cliente.Id,
            Cliente = cliente,
            Observacoes = dto.Observacoes,
            Status = StatusPedido.Aguardando
        };

        foreach (var itemDto in dto.Itens)
        {
            var produto = await _produtoRepository.ObterParaVendaAsync(itemDto.ProdutoId)
                ?? throw new ResourceNotFoundException(nameof(Produto), itemDto.ProdutoId);

            produto.Estoque?.Debitar(itemDto.Quantidade);

            pedido.AdicionarItem(produto, itemDto.Quantidade, itemDto.DescontoPercentual);
        }

        await _pedidoRepository.AdicionarAsync(pedido);

        var criado = await _pedidoRepository.ObterComItensAsync(pedido.Id) ?? pedido;

        _logger.LogInformation(
            "Pedido criado com sucesso. PedidoId={PedidoId} ValorTotal={ValorTotal} TraceId={TraceId}",
            criado.Id, criado.ValorTotal, traceId);

        return criado.ToResponseDto();
    }
}
