using Asp.Versioning.ApiExplorer;

namespace Perfumaria.API.Swagger;

/// <summary>
/// Configuração centralizada do Swagger/OpenAPI. A partir do CP5, um documento é gerado
/// por versão de API (<see cref="ConfigureSwaggerOptions"/>), e a UI permite alternar
/// entre os grupos <c>v1</c> (deprecado) e <c>v2</c>.
/// </summary>
public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddPerfumariaSwagger(this IServiceCollection services)
    {
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    public static IApplicationBuilder UsePerfumariaSwagger(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(d => d.ApiVersion))
            {
                var label = description.IsDeprecated
                    ? $"Perfumaria API {description.GroupName} (deprecada)"
                    : $"Perfumaria API {description.GroupName}";

                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", label);
            }

            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
