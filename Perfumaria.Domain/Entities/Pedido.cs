using Perfumaria.Domain.Common;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Domain.Entities;

public class Pedido : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string NumeroPedido { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public StatusPedido Status { get; set; } = StatusPedido.Aguardando;
    public decimal ValorTotal { get; set; }
    public string? Observacoes { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();

    /// <summary>
    /// Adiciona um item ao pedido a partir do produto vendido, tirando um snapshot do
    /// preço de venda atual e recalculando <see cref="ValorTotal"/>. Regras de negócio:
    /// a quantidade deve ser positiva e o desconto deve estar entre 0 e 100%.
    /// </summary>
    /// <exception cref="DomainException">
    /// Lançada quando <paramref name="quantidade"/> não é positiva ou quando
    /// <paramref name="descontoPercentual"/> está fora do intervalo [0, 100].
    /// </exception>
    public ItemPedido AdicionarItem(Produto produto, int quantidade, decimal descontoPercentual)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade do item deve ser maior que zero.");

        if (descontoPercentual < 0 || descontoPercentual > 100)
            throw new DomainException("O desconto percentual deve estar entre 0 e 100.");

        var item = new ItemPedido
        {
            PedidoId = Id,
            Pedido = this,
            ProdutoId = produto.Id,
            Produto = produto,
            Quantidade = quantidade,
            PrecoUnitario = produto.PrecoVenda,
            DescontoPercentual = descontoPercentual
        };

        Itens.Add(item);

        var subtotal = item.PrecoUnitario * item.Quantidade * (1 - item.DescontoPercentual / 100m);
        ValorTotal += subtotal;

        return item;
    }
}
