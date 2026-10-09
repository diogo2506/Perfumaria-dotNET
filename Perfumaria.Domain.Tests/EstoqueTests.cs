using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Domain.Tests;

/// <summary>
/// Testes de domínio (sem mock, sem Infrastructure/API) para a regra de negócio de
/// <see cref="Estoque.Debitar"/>: a quantidade debitada deve ser positiva e não pode
/// exceder a quantidade atualmente disponível.
/// </summary>
public class EstoqueTests
{
    private static Estoque CriarEstoque(int quantidadeAtual) => new()
    {
        Id = 1,
        QuantidadeAtual = quantidadeAtual,
        QuantidadeMinima = 5,
        Localizacao = "A1"
    };

    [Fact]
    public void Debitar_QuandoQuantidadeDisponivel_DeveReduzirQuantidadeAtual()
    {
        // Arrange
        var estoque = CriarEstoque(quantidadeAtual: 10);

        // Act
        estoque.Debitar(4);

        // Assert
        Assert.Equal(6, estoque.QuantidadeAtual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Debitar_QuandoQuantidadeInvalida_DeveLancarDomainException(int quantidade)
    {
        // Arrange
        var estoque = CriarEstoque(quantidadeAtual: 10);

        // Act & Assert
        Assert.Throws<DomainException>(() => estoque.Debitar(quantidade));
    }

    [Fact]
    public void Debitar_QuandoQuantidadeMaiorQueDisponivel_DeveLancarDomainException()
    {
        // Arrange
        var estoque = CriarEstoque(quantidadeAtual: 3);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => estoque.Debitar(4));
        Assert.Contains("insuficiente", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
