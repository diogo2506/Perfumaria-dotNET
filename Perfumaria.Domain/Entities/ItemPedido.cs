using Perfumaria.Domain.Common;

namespace Perfumaria.Domain.Entities;

public class ItemPedido : IEntity<int>
{
    public int Id { get; set; }
    public int Quantidade { get; set; }

    // Preço snapshot — salvo no momento da venda
    public decimal PrecoUnitario { get; set; }
    public decimal DescontoPercentual { get; set; } = 0;

    public Guid PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
}
