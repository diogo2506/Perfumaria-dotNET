using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.API.Exceptions;

/// <summary>
/// Tratamento global de exceções (CP3). Converte exceções não tratadas em respostas
/// no padrão RFC 7807 (<see cref="ProblemDetails"/>, <c>application/problem+json</c>),
/// mapeando exceções de domínio/aplicação para códigos HTTP adequados e evitando
/// vazar detalhes internos (stack trace, mensagens de infraestrutura) fora de
/// Development.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = MapException(exception);
        var traceId = httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Exceção não tratada capturada pelo GlobalExceptionHandler. StatusCode={StatusCode} Path={Path} TraceId={TraceId}",
            statusCode, httpContext.Request.Path, traceId);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path,
            Detail = ShouldExposeDetail(statusCode)
                ? exception.Message
                : "Ocorreu um erro inesperado ao processar a requisição."
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// Mapeamento de exceções para status HTTP:
    /// <list type="bullet">
    /// <item><description><see cref="ResourceNotFoundException"/> / <see cref="KeyNotFoundException"/> → 404</description></item>
    /// <item><description><see cref="ConflictException"/> → 409</description></item>
    /// <item><description><see cref="DomainException"/> / <see cref="ArgumentException"/> → 400</description></item>
    /// <item><description>Demais exceções → 500 (mensagem genérica fora de Development)</description></item>
    /// </list>
    /// </summary>
    private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
    {
        ResourceNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
        ConflictException => (StatusCodes.Status409Conflict, "Conflito de dados"),
        DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
        _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor")
    };

    private static bool ShouldExposeDetail(int statusCode) => statusCode != StatusCodes.Status500InternalServerError;
}
