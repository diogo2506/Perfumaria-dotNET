namespace Perfumaria.Domain.Exceptions;

/// <summary>
/// Lançada em conflitos de estado (ex.: violação de unicidade de negócio,
/// como SKU, CPF/CNPJ ou e-mail já cadastrados).
/// Mapeada pelo <c>GlobalExceptionHandler</c> (API) para HTTP 409.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
