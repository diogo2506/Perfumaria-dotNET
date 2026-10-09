# Perfumaria — CP1 + CP2 + CP3 + CP4 + CP5

## Integrantes

| Nome | RM |
|------|-----|
| Diogo Cunha Abrão de Oliveira | 563654 |

---

## Domínio

Sistema de gerenciamento para uma **perfumaria**: catálogo de produtos (perfumes), controle
de estoque, pedidos de venda e relacionamento com fornecedores e casas fabricantes.

> Este repositório é a evolução do CP2, originalmente modelado como uma loja de tintas.
> A partir do CP3, o domínio foi adaptado para uma perfumaria, mantendo a mesma
> estrutura de entidades e relacionamentos (apenas os campos específicos de produto
> foram trocados — volume em litros → mililitros, tipo de tinta → tipo de fragrância,
> cor base → nota olfativa) para não descaracterizar o trabalho de modelagem do CP1/CP2.

## Entidades

| Entidade | PK |
|---|---|
| `Categoria` | `int Id` |
| `Fabricante` | `int Id` |
| `Produto` | `Guid Id` |
| `Estoque` | `int Id` |
| `Fornecedor` | `int Id` |
| `ProdutoFornecedor` | `(Guid ProdutoId, int FornecedorId)` |
| `Cliente` | `Guid Id` |
| `Pedido` | `Guid Id` |
| `ItemPedido` | `int Id` |

## Relacionamentos

| Relacionamento | Cardinalidade |
|---|---|
| Categoria → Produto | 1:N |
| Fabricante → Produto | 1:N |
| Produto → Estoque | 1:1 |
| Produto ↔ Fornecedor | N:N via `ProdutoFornecedor` |
| Cliente → Pedido | 1:N |
| Pedido → ItemPedido | 1:N |
| Produto → ItemPedido | 1:N |

## Arquitetura

Clean Architecture em 4 projetos + 3 projetos de teste:

- **Domain** — entidades (com regras de negócio, ex.: `Estoque.Debitar`,
  `Pedido.AdicionarItem`), enums e exceções de domínio (`DomainException`,
  `ResourceNotFoundException`, `ConflictException`), sem dependências externas.
- **Application** — interfaces dos repositórios (genérico e por agregado), DTOs de
  request/response (incluindo o envelope paginado `ProdutoPaginadoResponseDto`,
  CP5), validação de paginação (`ParametrosPaginacao`, CP5), mappers de extensão
  (entidade ↔ DTO) e **serviços de aplicação** (`IProdutoService`, `IPedidoService`)
  que orquestram repositórios e regras de Domain — compartilhados entre as versões
  1.0 e 2.0 da API, sem duplicação de regra de negócio.
- **Infrastructure** — EF Core, `DbContext`, Fluent API (mappings), migrations e
  implementações dos repositórios, incluindo a paginação cortada no banco
  (`ProdutoRepository.ObterPaginadoAsync`, CP5).
- **API** — entry point, injeção de dependência, controllers REST versionados (CP5),
  Swagger por versão, rate limiting (CP5), health checks e tratamento global de
  exceções. **Não** acessa o `DbContext` diretamente.
- **Domain.Tests** — testes xUnit **sem mock**, referenciando somente o Domain.
- **Application.Tests** — testes xUnit **com mock** (Moq) das interfaces de
  repositório, exercitando os serviços de aplicação sem subir API nem banco.
- **Infrastructure.Tests** — testes de repositório com EF Core InMemory (herdado do CP3).

## Banco de Dados

SQLite — arquivo `perfumaria.db` criado automaticamente na raiz da API (as migrations
do CP2 são aplicadas automaticamente em ambiente de Development, via
`db.Database.Migrate()` no `Program.cs`).

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=perfumaria.db"
  }
}
```

## Como executar

```bash
dotnet restore

dotnet ef database update --project Perfumaria.Infrastructure --startup-project Perfumaria.API

dotnet run --project Perfumaria.API
```

A API sobe em `http://localhost:5169` (perfil `http` do `launchSettings.json`).

- Swagger: `http://localhost:5169/swagger`
- Health check: `http://localhost:5169/health`
- Listagem de produtos — **v1** (deprecada, array): `http://localhost:5169/api/produtos?api-version=1.0`
- Listagem de produtos — **v2** (atual, envelope paginado): `http://localhost:5169/api/produtos` (ou `?api-version=2.0`)

### Swagger

Com o ambiente em `Development`, a documentação interativa fica disponível em:

```
http://localhost:5169/swagger
```

O Swagger é gerado com **Swashbuckle.AspNetCore** + **Asp.Versioning.Mvc.ApiExplorer**,
incluindo:
- **Um documento por versão de API (CP5)**: `v1` e `v2`, descobertos em runtime via
  `IApiVersionDescriptionProvider` (`Perfumaria.API/Swagger/ConfigureSwaggerOptions.cs`).
  A UI (`/swagger`) mostra um seletor para alternar entre os dois grupos; a descrição do
  documento `v1` deixa explícito que a versão está **deprecada**.
- Metadados do documento (título, descrição, contato) configurados em `appsettings.json`
  (seção `Swagger`).
- Comentários XML dos controllers e DTOs (`GenerateDocumentationFile` habilitado no
  `.csproj` da API, incluídos via `IncludeXmlComments`).
- `[ProducesResponseType]` em cada action, documentando os códigos de sucesso e erro
  (incluindo `429` nos endpoints com rate limit).
- O endpoint `/health` **não** é um controller e, portanto, não aparece no Swagger
  (conforme escopo do CP4 — health check e documentação de negócio são públicos
  diferentes).

## Endpoints principais

| Método | Rota | Versão | Descrição |
|--------|------|--------|-----------|
| `GET` | `/health` | — | Health check (único endpoint de disponibilidade) |
| `GET` | `/api/categorias` | 1.0 e 2.0 | Lista categorias |
| `GET` | `/api/categorias/{id}` | 1.0 e 2.0 | Busca categoria por id |
| `POST` | `/api/categorias` | 1.0 e 2.0 | Cria categoria |
| `PUT` | `/api/categorias/{id}` | 1.0 e 2.0 | Atualiza categoria |
| `DELETE` | `/api/categorias/{id}` | 1.0 e 2.0 | Remove categoria |
| `GET` | `/api/produtos` | **1.0** (deprecada) | Lista produtos — array simples (filtro opcional `?categoriaId=`) |
| `GET` | `/api/produtos` | **2.0** (padrão) | Lista produtos — envelope paginado (`page`, `pageSize`, `?categoriaId=`) |
| `GET` | `/api/produtos/{id}` | 1.0 e 2.0 | Busca produto por id |
| `GET` | `/api/produtos/sku/{sku}` | 1.0 e 2.0 | Busca produto por SKU |
| `GET` | `/api/produtos/estoque-baixo` | 1.0 e 2.0 | Lista produtos com estoque abaixo do mínimo |
| `POST` | `/api/produtos` | 1.0 e 2.0 | Cria produto — **rate limit**: 10 req/min por IP |
| `PUT` | `/api/produtos/{id}` | 1.0 e 2.0 | Atualiza produto — **rate limit**: 10 req/min por IP |
| `DELETE` | `/api/produtos/{id}` | 1.0 e 2.0 | Remove produto |
| `GET` | `/api/clientes` | 1.0 e 2.0 | Lista clientes |
| `GET` | `/api/clientes/{id}` | 1.0 e 2.0 | Busca cliente por id |
| `POST` | `/api/clientes` | 1.0 e 2.0 | Cria cliente |
| `GET` | `/api/clientes/{id}/pedidos` | 1.0 e 2.0 | Lista pedidos de um cliente |
| `GET` | `/api/pedidos` | 1.0 e 2.0 | Lista pedidos (filtro opcional `?status=`) |
| `GET` | `/api/pedidos/{id}` | 1.0 e 2.0 | Busca pedido por id, com itens |
| `POST` | `/api/pedidos` | 1.0 e 2.0 | Cria pedido (debita estoque dos produtos) |
| `PATCH` | `/api/pedidos/{id}/status` | 1.0 e 2.0 | Atualiza o status do pedido |

`Categorias`, `Clientes` e `Pedidos` são marcados `[ApiVersionNeutral]` — contrato
inalterado entre versões, por isso continuam respondendo em 1.0 e 2.0 sem precisar de
`[MapToApiVersion]` em cada action.

Exemplos de chamadas (sucesso e erro, incluindo as duas versões da listagem de
produtos, paginação e rate limit) estão em `Perfumaria.API/Perfumaria.API.http`.

## Repositório genérico (CP3)

- Contrato `IRepository<TEntity, TKey>` em
  `Perfumaria.Application/Interfaces/Repositories/IRepository.cs`, restrito a
  entidades que implementam `IEntity<TKey>` (`Perfumaria.Domain/Common/IEntity.cs`).
- Operações: `ObterPorIdAsync`, `ObterTodosAsync`, `AdicionarAsync`, `AtualizarAsync`,
  `RemoverAsync`, `ExisteAsync`.
- Implementação `Repository<TEntity, TKey>` em
  `Perfumaria.Infrastructure/Repositories/Repository.cs`, usando `DbContext.Set<T>()`
  e `AsNoTracking()` nas leituras.
- Registrado na DI em `Program.cs`:
  ```csharp
  builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
  ```
- **Uso demonstrado** diretamente pelo `CategoriasController` (CRUD completo de
  `Categoria` via `IRepository<Categoria, int>`) e pelos serviços de aplicação
  `ProdutoService`/`PedidoService`, que usam `IRepository<Categoria, int>` e
  `IRepository<Fabricante, int>` para validar as referências antes de persistir um produto.
- Os repositórios específicos por agregado (`IProdutoRepository`, `IClienteRepository`,
  `IPedidoRepository`, `IEstoqueRepository`) estendem o contrato genérico e adicionam
  apenas as consultas que vão além do CRUD básico (ex.: busca por SKU, por status, com
  estoque baixo).

## Tratamento global de exceções (CP3)

- `GlobalExceptionHandler` (`Perfumaria.API/Exceptions/GlobalExceptionHandler.cs`)
  implementa `IExceptionHandler` e converte qualquer exceção não tratada em uma
  resposta `ProblemDetails` (RFC 7807), com `Content-Type: application/problem+json`.
- Registro no `Program.cs`:
  ```csharp
  builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
  builder.Services.AddProblemDetails();
  // ...
  app.UseExceptionHandler(); // antes de MapControllers e do Swagger
  ```
- Em Development, a resposta inclui `exceptionType` e `stackTrace` nas extensões do
  `ProblemDetails`; fora de Development, o campo `detail` é substituído por uma
  mensagem genérica para erros 500 (nenhum detalhe de infraestrutura é exposto).
- **(CP4)** Toda resposta de erro inclui `traceId` (`HttpContext.TraceIdentifier`) nas
  extensões do `ProblemDetails`, e a mesma exceção é logada em nível `Error` com o
  `traceId` como propriedade nomeada — permitindo correlacionar o erro retornado ao
  cliente com a entrada correspondente no log do servidor.

### Mapeamento de exceções → status HTTP

| Exceção | Status HTTP | Quando ocorre |
|---|---|---|
| `ResourceNotFoundException` (Domain) | `404 Not Found` | Recurso buscado por id/SKU não existe |
| `KeyNotFoundException` | `404 Not Found` | Fallback para buscas que não encontraram o recurso |
| `ConflictException` (Domain) | `409 Conflict` | Violação de unicidade de negócio (SKU, CPF/CNPJ, e-mail duplicados) |
| `DomainException` (Domain) | `400 Bad Request` | Regra de negócio violada (ex.: estoque insuficiente para o pedido) |
| `ArgumentException` | `400 Bad Request` | Argumento inválido não coberto por uma exceção de domínio específica |
| Demais exceções | `500 Internal Server Error` | Erro inesperado — mensagem genérica fora de Development |

Validações de payload (`DataAnnotations` nos DTOs) são tratadas automaticamente pelo
pipeline do ASP.NET Core (`AddProblemDetails` + `[ApiController]`), retornando
`400 Bad Request` no mesmo padrão `ProblemDetails` antes mesmo de chegar ao controller.

---

## Health checks (CP4)

Único endpoint de disponibilidade: **`GET /health`**, registrado em
`Perfumaria.API/HealthChecks/PerfumariaHealthCheckExtensions.cs` (não é uma rota de
negócio nem usa `EnsureCreated()` — apenas `AddHealthChecks()`/`MapHealthChecks`).

Checks incluídos:

| Check | O que verifica |
|---|---|
| `self` | Processo da API no ar (`HealthCheckResult.Healthy` fixo). |
| `database` | Conectividade com o SQLite via `AddDbContextCheck<PerfumariaDbContext>` (abordagem A recomendada pelo CP4 — alinhada ao `DbContext` do CP2). |
| `external-site` | *(Recomendado)* Verifica uma URL externa configurável (`HealthChecks:ExternalUrl` no `appsettings.json`) via `SiteExternoHealthCheck` (`IHealthCheck` customizado). Se a URL cair, esse check fica `Unhealthy` e derruba o status agregado de `/health` — demonstrando o impacto de uma dependência externa indisponível. |

A resposta é um **JSON customizado** (não o texto padrão `Healthy`/`Unhealthy`), com
status geral, duração total, `traceId` da requisição e a lista de checks (nome,
status, duração e descrição — o campo `exception` só é preenchido em Development):

```json
{
  "status": "Healthy",
  "totalDurationMs": 12.4,
  "traceId": "0HN...:00000001",
  "checks": [
    { "name": "self", "status": "Healthy", "durationMs": 0.01, "description": "A API está no ar.", "exception": null },
    { "name": "database", "status": "Healthy", "durationMs": 8.2, "description": null, "exception": null },
    { "name": "external-site", "status": "Healthy", "durationMs": 210.5, "description": "URL externa respondeu 200.", "exception": null }
  ]
}
```

Mapeamento de status para HTTP (`ResultStatusCodes`):

| Status agregado | HTTP |
|---|---|
| `Healthy` | `200 OK` |
| `Degraded` | `200 OK` (ainda serve tráfego, com aviso) |
| `Unhealthy` | `503 Service Unavailable` |

**Como simular falha do banco (local):** altere temporariamente
`ConnectionStrings:DefaultConnection` em `appsettings.Development.json` para um
caminho inválido (ex.: um diretório sem permissão de escrita) e reinicie a API — o
check `database` passa a `Unhealthy` e `/health` retorna `503`. Não é necessário
(e não deve ser feito) commitar essa alteração nem qualquer credencial real.

## Observabilidade — logs (CP4)

- Logging nativo (`ILogger<T>`), sem dependências externas (Serilog não foi
  necessário para este escopo).
- **Fluxo de escrita instrumentado:** criação de pedido (`PedidoService.CriarAsync`)
  registra um log de **início** (com `ClienteId`, `QuantidadeItens` e `TraceId` como
  propriedades nomeadas — nunca concatenando string solta) e um log de **sucesso**
  (com `PedidoId`, `ValorTotal` e `TraceId`). O mesmo padrão foi aplicado em
  `ProdutoService.CriarAsync`/`AtualizarAsync`.
- O `traceId` logado é sempre `HttpContext.TraceIdentifier`, propagado do controller
  para o serviço de aplicação como parâmetro — permitindo correlacionar todas as
  linhas de log de uma mesma requisição.
- `GlobalExceptionHandler` loga toda exceção não tratada em nível `Error`, incluindo
  `StatusCode`, `Path` e `TraceId` como propriedades nomeadas, e o mesmo `TraceId` é
  devolvido ao cliente nas extensões do `ProblemDetails`.
- Exemplo de log de console (formato ilustrativo):
  ```
  info: Perfumaria.Application.Services.PedidoService[0]
        Iniciando criação de pedido. ClienteId=3f2c... QuantidadeItens=2 TraceId=0HN...:00000001
  info: Perfumaria.Application.Services.PedidoService[0]
        Pedido criado com sucesso. PedidoId=8ab1... ValorTotal=289.90 TraceId=0HN...:00000001
  ```
- `/health` não aparece no Swagger e não expõe métricas (`/metrics`, Prometheus,
  Jaeger, Seq e OpenTelemetry Tracing estão fora do escopo deste CP, conforme enunciado).

---

## Versionamento de API (CP5)

O recurso escolhido para versionar foi **Produtos** (o catálogo mais sujeito a crescer
em volume). Apenas a **listagem** (`GET /api/produtos`) muda de contrato entre versões;
os demais endpoints do CP3 continuam no ar, sem duplicação de regra de negócio — as
duas versões chamam o mesmo `ProdutoService`.

- Pacotes: `Asp.Versioning.Mvc` e `Asp.Versioning.Mvc.ApiExplorer`.
- Configuração em `Program.cs`:
  ```csharp
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
  ```
- **v1 (deprecada)** — `[ApiVersion("1.0", Deprecated = true)]` no `ProdutosController`,
  action `ListarAsyncV1` mapeada com `[MapToApiVersion("1.0")]`: devolve o **array**
  simples de produtos, exatamente como no CP3.
- **v2 (atual)** — `[ApiVersion("2.0")]`, action `ListarAsyncV2` mapeada com
  `[MapToApiVersion("2.0")]`: devolve o **envelope paginado** (seção seguinte).

**Como informar a versão** (os três funcionam):
```http
GET /api/produtos?api-version=1.0
```
```http
GET /api/produtos
X-Api-Version: 1.0
```
```http
GET /api/produtos
```
O terceiro, sem versão informada, cai na **2.0** (`AssumeDefaultVersionWhenUnspecified`).

Toda resposta inclui os headers `api-supported-versions` (`1.0, 2.0`) e
`api-deprecated-versions` (`1.0`), graças a `ReportApiVersions = true`.

## Paginação (CP5 — só na listagem v2)

A v1 preserva o contrato antigo (array) de propósito — paginar ali seria uma quebra de
contrato silenciosa, que o versionamento existe justamente para evitar.

Parâmetros de query da **v2**:

| Parâmetro | Padrão | Regra |
|-----------|--------|-------|
| `page` | `1` | inteiro ≥ 1 |
| `pageSize` | `20` | inteiro de 1 a 100 |

- `page < 1` ou `pageSize` fora de `[1, 100]` → **400 Bad Request**, no mesmo padrão
  `ProblemDetails` do CP3 (`ParametrosPaginacao` lança `DomainException`, mapeada pelo
  `GlobalExceptionHandler`).
- Página além do total de itens → **200 OK** com `items: []` (não é erro).
- Corpo de sucesso:
  ```json
  {
    "page": 1,
    "pageSize": 20,
    "totalItems": 137,
    "totalPages": 7,
    "hasPrevious": false,
    "hasNext": true,
    "items": []
  }
  ```
- Onde cada camada entra:
  - **Controller** (`ProdutosController.ListarAsyncV2`) lê `page` e `pageSize` da query.
  - **Application** (`ProdutoService.ListarPaginadoAsync`) valida o intervalo via
    `ParametrosPaginacao` e monta o envelope (`ProdutoPaginadoResponseDto`), calculando
    `totalPages = ceil(totalItems / pageSize)`.
  - **Infrastructure** (`ProdutoRepository.ObterPaginadoAsync`) executa `Count` +
    `OrderBy(Nome).ThenBy(Id)` + `Skip` + `Take` sobre o `IQueryable`, materializando só
    no final (`ToListAsync`). A ordenação (obrigatória) usa o helper genérico
    `ObterPaginadoAsync` da classe base `Repository<TEntity, TKey>`, reutilizável por
    qualquer outro repositório que precise paginar.
- `GetById`/`ObterPorIdAsync` não pagina — segue devolvendo um único recurso.

## Rate limit (CP5)

Middleware nativo `Microsoft.AspNetCore.RateLimiting` (sem pacote de terceiros),
configurado em `Perfumaria.API/RateLimiting/PerfumariaRateLimitingExtensions.cs`:

- Política **fixed window** nomeada `escrita-fixa`: **10 requisições por minuto**,
  particionada por IP de origem (`RemoteIpAddress`).
- Aplicada em **`POST /api/produtos`** e **`PUT /api/produtos/{id}`** via
  `[EnableRateLimiting("escrita-fixa")]`.
- Ao estourar, responde **`429 Too Many Requests`** com:
  - Header **`Retry-After`** (segundos até a janela liberar de novo).
  - Corpo **JSON** (`application/problem+json`) com `status`, `title`, `detail` e
    `traceId`.
- `app.UseRateLimiter()` é registrado **depois** de `UseExceptionHandler()` e **antes**
  de `MapControllers()`.
- **`GET /health` nunca entra no teto**: como não há limitador global (apenas a
  política nomeada aplicada via atributo nos dois endpoints acima), o health check
  nunca é avaliado pelo rate limiter — não precisou de `DisableRateLimiting`.

**Como testar:** disparar `POST /api/produtos` 11+ vezes em menos de um minuto (ex.:
pela aba do Swagger `v1`/`v2` ou pelo `Perfumaria.API.http`) — a partir da 11ª chamada,
a resposta é `429` com `Retry-After`; `GET /health` continua `200` imediatamente depois.

---

## Testes (CP4 + CP5)

```bash
dotnet test
```

Três projetos de teste na mesma solução:

- **`Perfumaria.Domain.Tests`** — referencia **somente** `Perfumaria.Domain`, **sem
  mock**, seguindo o padrão AAA (Arrange/Act/Assert) e a convenção de nomes
  `MetodoOuCenario_Condicao_ResultadoEsperado`:
  - `EstoqueTests` — `[Fact]` no caminho feliz de `Estoque.Debitar` e `[Theory]` +
    `[InlineData]` para quantidades inválidas (zero, negativa e maior que o
    disponível), todas lançando `DomainException`.
  - `PedidoTests` — `[Fact]` no caminho feliz de `Pedido.AdicionarItem` (valida o
    recálculo de `ValorTotal`) e `[Theory]` + `[InlineData]` para quantidade
    inválida e desconto fora do intervalo `[0, 100]`.
- **`Perfumaria.Application.Tests`** — referencia `Perfumaria.Application` (e o
  Domain, indiretamente); **não** sobe a API nem o banco; usa **Moq** para mockar as
  interfaces de repositório:
  - `ProdutoServiceTests` — categoria inexistente e SKU duplicado lançam a exceção
    já mapeada no CP3 e **não** chamam `AdicionarAsync` (`Times.Never`); caminho
    feliz persiste uma vez (`Times.Once`).
  - `PedidoServiceTests` — cliente inexistente, produto inexistente e estoque
    insuficiente lançam a exceção correspondente e **não** chamam `AdicionarAsync`
    do pedido (`Times.Never`); caminho feliz persiste uma vez (`Times.Once`) e
    confirma o débito do estoque.
  - `ParametrosPaginacaoTests` **(CP5)** — `[Fact]` no intervalo válido de `page`/
    `pageSize` e `[Theory]` + `[InlineData]` para `page`/`pageSize` inválidos,
    todos lançando `DomainException`.
  - `ProdutoServiceTests.ListarPaginadoAsync_*` **(CP5)** — envelope com totais
    corretos a partir do que o repositório mockado devolve, e `page`/`pageSize`
    inválidos lançando `DomainException` **sem** consultar o repositório
    (`Times.Never`).
- **`Perfumaria.Infrastructure.Tests`** *(herdado do CP3)* — testes com EF Core
  InMemory, cobrindo o repositório de produtos (`ObterPorSkuAsync`, `ExisteAsync`,
  `ObterComEstoqueBaixoAsync`).

## Diagrama MER

O diagrama MER original do CP1 foi produzido para o domínio de loja de tintas; a
estrutura de entidades e relacionamentos é idêntica à adotada aqui — apenas os nomes de
campos específicos de `Produto` mudaram (ver seção *Domínio* acima). Recomenda-se
regenerar o diagrama (ex.: dbdiagram.io, drawSQL) com os nomes atuais das entidades e
salvá-lo em `docs/mer.png` / `docs/mer.svg`.

## `/docs`

Pasta reservada para os artefatos de entrega (ver `docs/README.md`):
- Print ou export do Swagger mostrando os endpoints documentados (CP3).
- Um exemplo de resposta `ProblemDetails` de um teste de erro, anonimizado (CP3).
- Prints/JSON de `/health` Healthy e Unhealthy, trecho de log com `traceId` e saída
  de `dotnet test` (CP4).
- JSON da listagem v1 (array) e v2 (envelope) do mesmo recurso, headers de versão,
  print do Swagger com os dois grupos, 400 de paginação inválida, páginas 1 e 2 sem
  sobreposição e 429 com `Retry-After` seguido de `/health` ainda `200` (CP5).
