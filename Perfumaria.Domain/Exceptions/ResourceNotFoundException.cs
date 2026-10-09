namespace Perfumaria.Domain.Exceptions;

/// <summary>
/// Lançada quando um recurso solicitado (por id) não é encontrado.
/// Mapeada pelo <c>GlobalExceptionHandler</c> (API) para HTTP 404.
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resource, object id)
        : base($"{resource} com id '{id}' não foi encontrado(a).") { }

    public ResourceNotFoundException(string message) : base(message) { }
}
