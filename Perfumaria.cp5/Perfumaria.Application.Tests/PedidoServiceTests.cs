using Microsoft.Extensions.Logging;
using Moq;
using Perfumaria.Application.DTOs.Pedidos;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Services;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Application.Tests;

/// <summary>
/// Testes de aplicação (com mock, sem subir API/banco) para <see cref="PedidoService"/>,
/// usando Moq nas interfaces de repositório.
/// </summary>
public class PedidoServiceTests
{
    private static PedidoRequestDto CriarDtoValido(Guid clienteId, Guid produtoId, int quantidade = 1) => new()
    {
        ClienteId = clienteId,
        Itens = new List<ItemPedidoRequestDto>
        {
            new() { ProdutoId = produtoId, Quantidade = quantidade, DescontoPercentual = 0m }
        }
    };

    private static Cliente CriarCliente(Guid id) => new()
    {
        Id = id,
        Nome = "Cliente Teste",
        CpfCnpj = "000.000.000-00",
        Email = "cliente@teste.com",
        Telefone = "11999999999"
    };

    private static Produto CriarProduto(Guid id, int estoqueAtual) => new()
    {
        Id = id,
        Nome = "Aurora Eau de Parfum",
        CodigoSku = "SKU-001",
        PrecoVenda = 100m,
        VolumeMl = 100m,
        TipoFragancia = TipoFragancia.Floral,
        CategoriaId = 1,
        FabricanteId = 1,
        Estoque = new Estoque { QuantidadeAtual = estoqueAtual, QuantidadeMinima = 5, Localizacao = "A1" }
    };

    [Fact]
    public async Task CriarAsync_QuandoClienteNaoExiste_DeveLancarResourceNotFoundExceptionENaoPersistir()
    {
        // Arrange
        var pedidoRepoMock = new Mock<IPedidoRepository>();
        var clienteRepoMock = new Mock<IClienteRepository>();
        var produtoRepoMock = new Mock<IProdutoRepository>();

        clienteRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Cliente?)null);

        var service = new PedidoService(
            pedidoRepoMock.Object, clienteRepoMock.Object, produtoRepoMock.Object,
            Mock.Of<ILogger<PedidoService>>());

        var dto = CriarDtoValido(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CriarAsync(dto, "trace-teste-1"));

        pedidoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_QuandoProdutoNaoExiste_DeveLancarResourceNotFoundExceptionENaoPersistir()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var pedidoRepoMock = new Mock<IPedidoRepository>();
        var clienteRepoMock = new Mock<IClienteRepository>();
        var produtoRepoMock = new Mock<IProdutoRepository>();

        clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId)).ReturnsAsync(CriarCliente(clienteId));
        produtoRepoMock.Setup(r => r.ObterParaVendaAsync(It.IsAny<Guid>())).ReturnsAsync((Produto?)null);

        var service = new PedidoService(
            pedidoRepoMock.Object, clienteRepoMock.Object, produtoRepoMock.Object,
            Mock.Of<ILogger<PedidoService>>());

        var dto = CriarDtoValido(clienteId, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CriarAsync(dto, "trace-teste-2"));

        pedidoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_QuandoEstoqueInsuficiente_DeveLancarDomainExceptionENaoPersistir()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var pedidoRepoMock = new Mock<IPedidoRepository>();
        var clienteRepoMock = new Mock<IClienteRepository>();
        var produtoRepoMock = new Mock<IProdutoRepository>();

        clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId)).ReturnsAsync(CriarCliente(clienteId));
        produtoRepoMock.Setup(r => r.ObterParaVendaAsync(produtoId)).ReturnsAsync(CriarProduto(produtoId, estoqueAtual: 1));

        var service = new PedidoService(
            pedidoRepoMock.Object, clienteRepoMock.Object, produtoRepoMock.Object,
            Mock.Of<ILogger<PedidoService>>());

        var dto = CriarDtoValido(clienteId, produtoId, quantidade: 5);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => service.CriarAsync(dto, "trace-teste-3"));

        pedidoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DevePersistirUmaVezEDebitarEstoque()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var pedidoRepoMock = new Mock<IPedidoRepository>();
        var clienteRepoMock = new Mock<IClienteRepository>();
        var produtoRepoMock = new Mock<IProdutoRepository>();

        var produto = CriarProduto(produtoId, estoqueAtual: 10);

        clienteRepoMock.Setup(r => r.ObterPorIdAsync(clienteId)).ReturnsAsync(CriarCliente(clienteId));
        produtoRepoMock.Setup(r => r.ObterParaVendaAsync(produtoId)).ReturnsAsync(produto);
        pedidoRepoMock.Setup(r => r.ObterComItensAsync(It.IsAny<Guid>())).ReturnsAsync((Pedido?)null);

        var service = new PedidoService(
            pedidoRepoMock.Object, clienteRepoMock.Object, produtoRepoMock.Object,
            Mock.Of<ILogger<PedidoService>>());

        var dto = CriarDtoValido(clienteId, produtoId, quantidade: 3);

        // Act
        var resultado = await service.CriarAsync(dto, "trace-teste-4");

        // Assert
        Assert.Equal(300m, resultado.ValorTotal); // 3 * 100, sem desconto
        Assert.Equal(7, produto.Estoque!.QuantidadeAtual); // 10 - 3
        pedidoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Pedido>()), Times.Once);
    }
}
