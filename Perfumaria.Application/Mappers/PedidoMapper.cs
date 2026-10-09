using Perfumaria.Application.DTOs.Pedidos;
using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Mappers;

public static class PedidoMapper
{
    public static PedidoResponseDto ToResponseDto(this Pedido pedido) => new()
    {
        Id = pedido.Id,
        NumeroPedido = pedido.NumeroPedido,
        DataPedido = pedido.DataPedido,
        Status = pedido.Status.ToString(),
        ValorTotal = pedido.ValorTotal,
        Observacoes = pedido.Observacoes,
        ClienteId = pedido.ClienteId,
        ClienteNome = pedido.Cliente?.Nome,
        Itens = pedido.Itens.Select(i => new ItemPedidoResponseDto
        {
            ProdutoId = i.ProdutoId,
            ProdutoNome = i.Produto?.Nome,
            Quantidade = i.Quantidade,
            PrecoUnitario = i.PrecoUnitario,
            DescontoPercentual = i.DescontoPercentual
        }).ToList()
    };
}
