using Asp.Versioning;
using Perfumaria.API.Exceptions;
using Perfumaria.API.HealthChecks;
using Perfumaria.API.RateLimiting;
using Perfumaria.API.Swagger;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Services;
using Perfumaria.Infrastructure.Data;
using Perfumaria.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Persistência (herdada do CP2)
builder.Services.AddDbContext<PerfumariaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositório genérico (CP3) — disponível para qualquer entidade que implemente IEntity<TKey>
builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

// Repositórios específicos por agregado (convivem com o genérico quando há consultas extras)
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IEstoqueRepository, EstoqueRepository>();

// Serviços de aplicação (CP4) — orquestram repositórios + regras de Domain,
// tornando os fluxos de escrita testáveis com mocks (Perfumaria.Application.Tests).
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Tratamento global de exceções (CP3) — RFC 7807 / ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks (CP4)
builder.Services.AddPerfumariaHealthChecks(builder.Configuration);

// Versionamento de API (CP5): recurso Produtos convive em 1.0 (deprecada) e 2.0 (atual).
// Sem versão na requisição, assume a 2.0; aceita a versão por query string ou header.
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(2, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// Rate limiting (CP5): fixed window nos endpoints de escrita marcados com
// [EnableRateLimiting]. Sem limitador global, então GET /health nunca é afetado.
builder.Services.AddPerfumariaRateLimiting();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddPerfumariaSwagger();

var app = builder.Build();

// O tratamento global de exceções deve vir antes de UseRateLimiter/MapControllers/Swagger.
app.UseExceptionHandler();

// Entre o tratamento de exceções e o roteamento para os controllers (CP5).
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UsePerfumariaSwagger();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PerfumariaDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.MapControllers();

// Único endpoint de health check (CP4): processo (self) + banco (database) + URL
// externa opcional (external-site), com writer JSON e status HTTP coerentes.
// Fora do rate limiting (CP5): não está marcado com [EnableRateLimiting].
app.UsePerfumariaHealthChecks();

app.Run();

/// <summary>
/// Classe parcial exposta apenas para permitir testes de integração (ex.: WebApplicationFactory)
/// referenciarem o entry point da API.
/// </summary>
public partial class Program { }
