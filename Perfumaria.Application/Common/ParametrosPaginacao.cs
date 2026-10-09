using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Application.Common;

/// <summary>
/// Parâmetros de paginação validados (CP5): <c>page</c> começa em 1 e <c>pageSize</c>
/// fica restrito ao intervalo [1, 100]. Fora desses limites, o construtor lança
/// <see cref="DomainException"/>, que o <c>GlobalExceptionHandler</c> (CP3) converte em
/// <c>400 Bad Request</c> no padrão <c>ProblemDetails</c>.
/// </summary>
public class ParametrosPaginacao
{
    public const int PagePadrao = 1;
    public const int PageSizePadrao = 20;
    public const int PageSizeMinimo = 1;
    public const int PageSizeMaximo = 100;

    public int Page { get; }
    public int PageSize { get; }

    public ParametrosPaginacao(int page, int pageSize)
    {
        if (page < 1)
            throw new DomainException($"O parâmetro 'page' deve ser maior ou igual a 1. Valor informado: {page}.");

        if (pageSize < PageSizeMinimo || pageSize > PageSizeMaximo)
        {
            throw new DomainException(
                $"O parâmetro 'pageSize' deve estar entre {PageSizeMinimo} e {PageSizeMaximo}. " +
                $"Valor informado: {pageSize}.");
        }

        Page = page;
        PageSize = pageSize;
    }
}
