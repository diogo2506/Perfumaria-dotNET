using Microsoft.Extensions.Logging;
using Moq;
using Perfumaria.Application.DTOs.Produtos;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Services;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Application.Tests;

/// <summary>
/// Testes de aplicação (com mock, sem subir API/banco) para <see cref="ProdutoService"/>,
/// usando Moq nas interfaces de repositório (<see cref="IProdutoRepository"/> e
/// <see cref="IRepository{TEntity, TKey}"/>).
/// </summary>
public class ProdutoServiceTests
{
    private static ProdutoRequestDto CriarDtoValido() => new()
    {
        Nome = "Aurora Eau de Parfum",
        CodigoSku = "SKU-001",
        PrecoVenda = 289.90m,
        VolumeMl = 100m,
        TipoFragancia = TipoFragancia.Floral,
        CategoriaId = 1,
        FabricanteId = 1
    };

    [Fact]
    public async Task CriarAsync_QuandoCategoriaNaoExiste_DeveLancarResourceNotFoundExceptionENaoPersistir()
    {
        // Arrange
        var produtoRepoMock = new Mock<IProdutoRepository>();
        var categoriaRepoMock = new Mock<IRepository<Categoria, int>>();
        var fabricanteRepoMock = new Mock<IRepository<Fabricante, int>>();

        categoriaRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Categoria?)null);

        var service = new ProdutoService(
            produtoRepoMock.Object,
            categoriaRepoMock.Object,
            fabricanteRepoMock.Object,
            Mock.Of<ILogger<ProdutoService>>());

        var dto = CriarDtoValido();

        // Act & Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(() => service.CriarAsync(dto));

        produtoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Produto>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_QuandoSkuJaExiste_DeveLancarConflictExceptionENaoPersistir()
    {
        // Arrange
        var produtoRepoMock = new Mock<IProdutoRepository>();
        var categoriaRepoMock = new Mock<IRepository<Categoria, int>>();
        var fabricanteRepoMock = new Mock<IRepository<Fabricante, int>>();

        categoriaRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Categoria { Id = 1, Nome = "Floral" });
        fabricanteRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Fabricante { Id = 1, Nome = "Maison Aurora", Cnpj = "00.000.000/0001-00" });
        produtoRepoMock.Setup(r => r.ObterPorSkuAsync(It.IsAny<string>()))
            .ReturnsAsync(new Produto
            {
                Nome = "Existente",
                CodigoSku = "SKU-001",
                CategoriaId = 1,
                FabricanteId = 1
            });

        var service = new ProdutoService(
            produtoRepoMock.Object,
            categoriaRepoMock.Object,
            fabricanteRepoMock.Object,
            Mock.Of<ILogger<ProdutoService>>());

        var dto = CriarDtoValido();

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => service.CriarAsync(dto));

        produtoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Produto>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_ComDadosValidos_DevePersistirUmaVez()
    {
        // Arrange
        var produtoRepoMock = new Mock<IProdutoRepository>();
        var categoriaRepoMock = new Mock<IRepository<Categoria, int>>();
        var fabricanteRepoMock = new Mock<IRepository<Fabricante, int>>();

        categoriaRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Categoria { Id = 1, Nome = "Floral" });
        fabricanteRepoMock.Setup(r => r.ObterPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Fabricante { Id = 1, Nome = "Maison Aurora", Cnpj = "00.000.000/0001-00" });
        produtoRepoMock.Setup(r => r.ObterPorSkuAsync(It.IsAny<string>()))
            .ReturnsAsync((Produto?)null);

        var service = new ProdutoService(
            produtoRepoMock.Object,
            categoriaRepoMock.Object,
            fabricanteRepoMock.Object,
            Mock.Of<ILogger<ProdutoService>>());

        var dto = CriarDtoValido();

        // Act
        var resultado = await service.CriarAsync(dto);

        // Assert
        Assert.Equal(dto.CodigoSku, resultado.CodigoSku);
        produtoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Produto>()), Times.Once);
    }

    [Fact]
    public async Task ListarPaginadoAsync_ComParametrosValidos_DeveMontarEnvelopeComTotaisCorretos()
    {
        // Arrange
        var produtoRepoMock = new Mock<IProdutoRepository>();
        var categoriaRepoMock = new Mock<IRepository<Categoria, int>>();
        var fabricanteRepoMock = new Mock<IRepository<Fabricante, int>>();

        var itens = new List<Produto>
        {
            new() { Nome = "A", CodigoSku = "SKU-A", CategoriaId = 1, FabricanteId = 1 },
            new() { Nome = "B", CodigoSku = "SKU-B", CategoriaId = 1, FabricanteId = 1 }
        };

        produtoRepoMock
            .Setup(r => r.ObterPaginadoAsync(2, 2, null))
            .ReturnsAsync((itens, 5));

        var service = new ProdutoService(
            produtoRepoMock.Object,
            categoriaRepoMock.Object,
            fabricanteRepoMock.Object,
            Mock.Of<ILogger<ProdutoService>>());

        // Act
        var pagina = await service.ListarPaginadoAsync(page: 2, pageSize: 2);

        // Assert
        Assert.Equal(2, pagina.Page);
        Assert.Equal(2, pagina.PageSize);
        Assert.Equal(5, pagina.TotalItems);
        Assert.Equal(3, pagina.TotalPages); // ceil(5 / 2)
        Assert.True(pagina.HasPrevious);
        Assert.True(pagina.HasNext);
        Assert.Equal(2, pagina.Items.Count);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task ListarPaginadoAsync_ComPageOuPageSizeInvalido_DeveLancarDomainExceptionENaoConsultarRepositorio(
        int page, int pageSize)
    {
        // Arrange
        var produtoRepoMock = new Mock<IProdutoRepository>();
        var categoriaRepoMock = new Mock<IRepository<Categoria, int>>();
        var fabricanteRepoMock = new Mock<IRepository<Fabricante, int>>();

        var service = new ProdutoService(
            produtoRepoMock.Object,
            categoriaRepoMock.Object,
            fabricanteRepoMock.Object,
            Mock.Of<ILogger<ProdutoService>>());

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => service.ListarPaginadoAsync(page, pageSize));

        produtoRepoMock.Verify(
            r => r.ObterPaginadoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }
}
