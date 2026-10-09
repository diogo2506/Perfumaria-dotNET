using Perfumaria.Application.DTOs.Produtos;

namespace Perfumaria.Application.Services;

/// <summary>
/// Serviço de aplicação para criação e atualização de produtos (perfumes),
/// concentrando as validações de negócio que envolvem múltiplos repositórios
/// (existência de Categoria/Fabricante, unicidade de SKU) fora do controller.
/// </summary>
public interface IProdutoService
{
    Task<ProdutoResponseDto> CriarAsync(ProdutoRequestDto dto);
    Task<ProdutoResponseDto> AtualizarAsync(Guid id, ProdutoRequestDto dto);

    /// <summary>
    /// Lista produtos paginados (CP5): valida <paramref name="page"/> e
    /// <paramref name="pageSize"/> (lança <see cref="Perfumaria.Domain.Exceptions.DomainException"/>
    /// fora dos limites, mapeada para <c>400</c> pelo <c>GlobalExceptionHandler</c>) e pede a
    /// página já cortada no banco ao repositório.
    /// </summary>
    Task<ProdutoPaginadoResponseDto> ListarPaginadoAsync(int page, int pageSize, int? categoriaId = null);
}
