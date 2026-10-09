namespace Perfumaria.Domain.Exceptions;

/// <summary>
/// Exceção base para violações de regras de negócio/domínio.
/// Mapeada pelo <c>GlobalExceptionHandler</c> (API) para HTTP 400.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
