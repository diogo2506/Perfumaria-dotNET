using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Perfumaria.API.HealthChecks;

/// <summary>
/// Health check opcional (recomendado pelo CP4) que verifica a disponibilidade de uma
/// URL externa configurada em <c>HealthChecks:ExternalUrl</c> (ex.: o site da FIAP).
/// Se a URL não responder, este check fica <see cref="HealthStatus.Unhealthy"/>, o que
/// derruba o status agregado de <c>/health</c> — demonstrando o impacto de uma
/// dependência externa indisponível no relatório geral.
/// </summary>
public class SiteExternoHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SiteExternoHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var url = _configuration["HealthChecks:ExternalUrl"];

        if (string.IsNullOrWhiteSpace(url))
            return HealthCheckResult.Healthy("Nenhuma URL externa configurada (check ignorado).");

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(SiteExternoHealthCheck));
            client.Timeout = TimeSpan.FromSeconds(5);

            using var response = await client.GetAsync(url, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"URL externa respondeu {(int)response.StatusCode}.")
                : HealthCheckResult.Unhealthy($"URL externa respondeu {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao acessar a URL externa configurada.", ex);
        }
    }
}
