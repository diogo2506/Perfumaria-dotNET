using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Infrastructure.Data;
using Perfumaria.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Perfumaria.Infrastructure.Tests;

public class ProdutoRepositoryTests
{
    private PerfumariaDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<PerfumariaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PerfumariaDbContext(options);
    }

    [Fact]
    public async Task AdicionarProduto_DeveSalvarERecuperarPeloSku()
    {
        // Arrange — preparar os dados
        using var ctx = CriarContexto();
        var repo = new ProdutoRepository(ctx);

        var categoria = new Categoria { Nome = "Perfumaria Feminina" };
        var fabricante = new Fabricante { Nome = "Maison Aurora", Cnpj = "00.000.000/0001-00" };
        ctx.Categorias.Add(categoria);
        ctx.Fabricantes.Add(fabricante);
        await ctx.SaveChangesAsync();

        var produto = new Produto
        {
            Nome = "Aurora Eau de Parfum",
            CodigoSku = "SKU-001",
            PrecoVenda = 289.90m,
            VolumeMl = 100m,
            TipoFragancia = TipoFragancia.Floral,
            NotaOlfativa = "Jasmim, Baunilha e Almíscar",
            CategoriaId = categoria.Id,
            FabricanteId = fabricante.Id
        };

        // Act — executar o que está sendo testado
        await repo.AdicionarAsync(produto);
        var resultado = await repo.ObterPorSkuAsync("SKU-001");

        // Assert — verificar se deu certo
        Assert.NotNull(resultado);
        Assert.Equal("Aurora Eau de Parfum", resultado.Nome);
    }

    [Fact]
    public async Task ExisteAsync_DeveRetornarTrue_QuandoProdutoExiste()
    {
        using var ctx = CriarContexto();
        var repo = new ProdutoRepository(ctx);

        var categoria = new Categoria { Nome = "Perfumaria Masculina" };
        var fabricante = new Fabricante { Nome = "Maison Noir", Cnpj = "11.111.111/0001-11" };
        ctx.Categorias.Add(categoria);
        ctx.Fabricantes.Add(fabricante);
        await ctx.SaveChangesAsync();

        var produto = new Produto
        {
            Nome = "Noir Intense",
            CodigoSku = "SKU-002",
            PrecoVenda = 349.90m,
            VolumeMl = 50m,
            TipoFragancia = TipoFragancia.Amadeirado,
            CategoriaId = categoria.Id,
            FabricanteId = fabricante.Id
        };
        await repo.AdicionarAsync(produto);

        var existe = await repo.ExisteAsync(produto.Id);
        var naoExiste = await repo.ExisteAsync(Guid.NewGuid());

        Assert.True(existe);
        Assert.False(naoExiste);
    }

    [Fact]
    public async Task ObterComEstoqueBaixoAsync_DeveRetornarApenasProdutosAbaixoDoMinimo()
    {
        using var ctx = CriarContexto();
        var repo = new ProdutoRepository(ctx);

        var categoria = new Categoria { Nome = "Perfumaria Unissex" };
        var fabricante = new Fabricante { Nome = "Maison Vento", Cnpj = "22.222.222/0001-22" };
        ctx.Categorias.Add(categoria);
        ctx.Fabricantes.Add(fabricante);
        await ctx.SaveChangesAsync();

        var produtoBaixo = new Produto
        {
            Nome = "Vento Cítrico",
            CodigoSku = "SKU-003",
            PrecoVenda = 199.90m,
            VolumeMl = 75m,
            TipoFragancia = TipoFragancia.Citrico,
            CategoriaId = categoria.Id,
            FabricanteId = fabricante.Id,
            Estoque = new Estoque { QuantidadeAtual = 2, QuantidadeMinima = 10, Localizacao = "A1" }
        };

        var produtoOk = new Produto
        {
            Nome = "Vento Suave",
            CodigoSku = "SKU-004",
            PrecoVenda = 179.90m,
            VolumeMl = 75m,
            TipoFragancia = TipoFragancia.Aquatico,
            CategoriaId = categoria.Id,
            FabricanteId = fabricante.Id,
            Estoque = new Estoque { QuantidadeAtual = 50, QuantidadeMinima = 10, Localizacao = "A2" }
        };

        await repo.AdicionarAsync(produtoBaixo);
        await repo.AdicionarAsync(produtoOk);

        var resultado = (await repo.ObterComEstoqueBaixoAsync()).ToList();

        Assert.Single(resultado);
        Assert.Equal("Vento Cítrico", resultado[0].Nome);
    }
}
