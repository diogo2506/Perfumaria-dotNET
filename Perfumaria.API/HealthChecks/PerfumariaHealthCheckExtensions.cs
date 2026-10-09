using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Perfumaria.Infrastructure.Data;

namespace Perfumaria.API.HealthChecks;

/// <summary>
/// Extensões para registrar e mapear os health checks da API (CP4), mantendo o
/// <c>Program.cs</c> enxuto.
/// </summary>
public static class PerfumariaHealthCheckExtensions
{
    /// <summary>
    /// Registra os checks de health: <c>self</c> (processo no ar), <c>database</c>
    /// (via <see cref="PerfumariaDbContext"/>, abordagem (A) recomendada pelo CP4) e,
    /// opcionalmente, <c>external-site</c> (URL externa configurável).
    /// </summary>
    public static IServiceCollection AddPerfumariaHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient();

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("A API está no ar."), tags: ["self"])
            .AddDbContextCheck<PerfumariaDbContext>("database", tags: ["database"])
            .AddCheck<SiteExternoHealthCheck>("external-site", tags: ["external"]);

        return services;
    }

    /// <summary>
    /// Mapeia o único endpoint de health check exigido pelo CP4: <c>GET /health</c>,
    /// com um response writer JSON (status geral, duração total, traceId e a lista de
    /// checks individuais) e o mapeamento de status HTTP Healthy/Degraded → 200,
    /// Unhealthy → 503.
    /// </summary>
    public static IEndpointRouteBuilder UsePerfumariaHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteResponseAsync,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            }
        });

        return endpoints;
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var isDevelopment = context.RequestServices
            .GetRequiredService<IHostEnvironment>()
            .IsDevelopment();

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            traceId = context.TraceIdentifier,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = isDevelopment ? entry.Value.Exception?.Message : null
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return context.Response.WriteAsync(json);
    }
}
