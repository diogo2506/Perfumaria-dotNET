using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Perfumaria.API.Swagger;

/// <summary>
/// Gera um documento Swagger por versão de API descoberta pelo
/// <see cref="IApiVersionDescriptionProvider"/> (CP5), marcando a versão deprecada na
/// descrição do documento. Resolvido pela DI (via <c>ConfigureOptions&lt;T&gt;</c>), e não
/// diretamente dentro de <c>AddSwaggerGen</c>, porque precisa do provider de versões.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IConfiguration _configuration;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var title = _configuration["Swagger:Title"] ?? "Perfumaria API";
        var baseDescription = _configuration["Swagger:Description"]
            ?? "API REST para gestão de uma perfumaria: catálogo de produtos, estoque, " +
               "clientes, pedidos de venda e relacionamento com fornecedores e fabricantes.";
        var contactName = _configuration["Swagger:ContactName"] ?? "Perfumaria";

        foreach (var description in _provider.ApiVersionDescriptions)
        {
            var info = new OpenApiInfo
            {
                Title = title,
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated
                    ? $"{baseDescription}\n\n**Esta versão ({description.ApiVersion}) está DEPRECADA** — " +
                      "disponível apenas para compatibilidade com integrações existentes. " +
                      "Utilize a versão mais recente para novas integrações."
                    : baseDescription,
                Contact = new OpenApiContact { Name = contactName }
            };

            options.SwaggerDoc(description.GroupName, info);
        }

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }

        // O ApiExplorer versionado (Asp.Versioning.Mvc.ApiExplorer) já gera uma
        // ApiDescription por versão suportada por cada action — inclusive para controllers
        // [ApiVersionNeutral], que ganham uma entrada em cada grupo. Por isso basta comparar
        // o GroupName calculado pelo provider com o nome do documento Swagger.
        options.DocInclusionPredicate((documentName, apiDescription) =>
            apiDescription.GroupName == documentName);
    }
}
