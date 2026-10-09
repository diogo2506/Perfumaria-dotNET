using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Perfumaria.API.RateLimiting;

/// <summary>
/// Extensões para registrar o rate limiting (CP5), mantendo o <c>Program.cs</c> enxuto.
/// Política <c>fixed window</c> nomeada <c>escrita-fixa</c>, aplicada apenas aos endpoints
/// de escrita marcados com <c>[EnableRateLimiting("escrita-fixa")]</c> (ex.:
/// <c>POST</c>/<c>PUT /api/produtos</c>). Como não há limitador global, <c>GET /health</c>
/// nunca entra no teto — não precisa de <c>DisableRateLimiting</c>.
/// </summary>
public static class PerfumariaRateLimitingExtensions
{
    public const string PoliticaEscrita = "escrita-fixa";

    /// <summary>Limite da política <c>escrita-fixa</c>: requisições por janela.</summary>
    public const int LimiteEscrita = 10;

    /// <summary>Janela (fixed window) da política <c>escrita-fixa</c>.</summary>
    public static readonly TimeSpan JanelaEscrita = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddPerfumariaRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Partição por IP (recomendado pelo CP5): cada endereço de origem tem sua
            // própria janela fixa de LimiteEscrita requisições por JanelaEscrita.
            options.AddPolicy(PoliticaEscrita, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = LimiteEscrita,
                        Window = JanelaEscrita,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                var retryAfterSegundos = (int)JanelaEscrita.TotalSeconds;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    retryAfterSegundos = (int)retryAfter.TotalSeconds;

                context.HttpContext.Response.Headers["Retry-After"] = retryAfterSegundos.ToString();
                context.HttpContext.Response.ContentType = "application/problem+json";

                var traceId = context.HttpContext.TraceIdentifier;

                var problemDetails = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Limite de requisições excedido",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = $"Limite de {LimiteEscrita} requisições por {JanelaEscrita.TotalSeconds:0} segundos " +
                             $"excedido para este endpoint. Tente novamente em {retryAfterSegundos} segundo(s).",
                    instance = context.HttpContext.Request.Path.Value,
                    traceId
                };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
        });

        return services;
    }
}
