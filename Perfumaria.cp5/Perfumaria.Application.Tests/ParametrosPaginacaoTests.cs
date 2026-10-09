using Perfumaria.Application.Common;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Application.Tests;

/// <summary>
/// Testes de aplicação (CP5, sem subir API nem banco) para a validação de
/// <see cref="ParametrosPaginacao"/>, usada pela listagem paginada de produtos (v2).
/// </summary>
public class ParametrosPaginacaoTests
{
    [Fact]
    public void Construtor_ComPageEPageSizeValidos_DeveAtribuirOsValores()
    {
        // Arrange & Act
        var parametros = new ParametrosPaginacao(page: 2, pageSize: 50);

        // Assert
        Assert.Equal(2, parametros.Page);
        Assert.Equal(50, parametros.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComPageInvalido_DeveLancarDomainException(int page)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new ParametrosPaginacao(page, pageSize: 20));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(101)]
    [InlineData(9999)]
    public void Construtor_ComPageSizeForaDoIntervalo_DeveLancarDomainException(int pageSize)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new ParametrosPaginacao(page: 1, pageSize));
    }
}
