using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Domain.Tests;

/// <summary>
/// Testes de domínio (sem mock, sem Infrastructure/API) para a regra de negócio de
/// <see cref="Pedido.AdicionarItem"/>: quantidade positiva, desconto entre 0 e 100%,
/// e recálculo correto de <see cref="Pedido.ValorTotal"/>.
/// </summary>
public class PedidoTests
{
    private static Pedido CriarPedido() => new()
    {
        NumeroPedido = "PED-TESTE-0001",
        ClienteId = Guid.NewGuid()
    };

    private static Produto CriarProduto(decimal precoVenda) => new()
    {
        Nome = "Aurora Eau de Parfum",
        CodigoSku = "SKU-TESTE",
        PrecoVenda = precoVenda,
        VolumeMl = 100m,
        TipoFragancia = TipoFragancia.Floral,
        CategoriaId = 1,
        FabricanteId = 1
    };

    [Fact]
    public void AdicionarItem_ComDadosValidos_DeveAdicionarItemEAtualizarValorTotal()
    {
        // Arrange
        var pedido = CriarPedido();
        var produto = CriarProduto(precoVenda: 100m);

        // Act
        var item = pedido.AdicionarItem(produto, quantidade: 2, descontoPercentual: 10m);

        // Assert
        Assert.Single(pedido.Itens);
        Assert.Same(item, pedido.Itens.First());
        Assert.Equal(100m, item.PrecoUnitario);
        Assert.Equal(180m, pedido.ValorTotal); // 2 * 100 * (1 - 0.10)
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AdicionarItem_ComQuantidadeInvalida_DeveLancarDomainException(int quantidade)
    {
        // Arrange
        var pedido = CriarPedido();
        var produto = CriarProduto(precoVenda: 100m);

        // Act & Assert
        Assert.Throws<DomainException>(() => pedido.AdicionarItem(produto, quantidade, descontoPercentual: 0m));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void AdicionarItem_ComDescontoForaDoIntervalo_DeveLancarDomainException(decimal desconto)
    {
        // Arrange
        var pedido = CriarPedido();
        var produto = CriarProduto(precoVenda: 100m);

        // Act & Assert
        Assert.Throws<DomainException>(() => pedido.AdicionarItem(produto, quantidade: 1, desconto));
    }
}
