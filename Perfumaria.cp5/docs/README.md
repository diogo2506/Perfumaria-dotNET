# docs/

Pasta reservada para os artefatos de entrega (evidências do CP3 e do CP4).

## CP3

- `swagger.png` (ou similar) — print/export da Swagger UI (`/swagger`) mostrando os
  endpoints documentados com seus `ProducesResponseType`.
- `problem-details-exemplo.json` — trecho de uma resposta `ProblemDetails` real,
  capturada de um teste de erro (ex.: `GET /api/produtos/{id-inexistente}` → 404),
  com qualquer dado sensível anonimizado.
- `mer.png` / `mer.svg` — diagrama MER do domínio (herdado do CP1, com os nomes de
  campos atualizados para o domínio de perfumaria).

## CP4

- `health-healthy.json` — trecho da resposta de `GET /health` com a API e o banco
  no ar (status 200, todos os checks `Healthy`).
- `health-unhealthy.json` (ou print) — trecho da resposta de `GET /health` com o
  banco indisponível (status 503), obtido apontando `ConnectionStrings:DefaultConnection`
  para um arquivo/caminho inválido em ambiente **local** (nunca commitar credenciais
  reais) ou parando o SGBD.
- `log-traceid.png` (ou trecho de texto) — log de console de um `POST /api/pedidos`
  mostrando as mensagens de início/sucesso com `TraceId` correlacionado.
- `dotnet-test.png` (ou saída de texto) — resultado de `dotnet test` com todos os
  testes passando (`Perfumaria.Domain.Tests`, `Perfumaria.Application.Tests` e
  `Perfumaria.Infrastructure.Tests`).

## CP5

- `produtos-v1.json` — resposta de `GET /api/produtos?api-version=1.0` (array simples).
- `produtos-v2.json` — resposta de `GET /api/produtos` (envelope paginado, `pageSize`
  pequeno para evidenciar `totalPages`).
- `headers-versao.png` (ou trecho de texto) — headers `api-supported-versions` e
  `api-deprecated-versions` de qualquer resposta de `/api/produtos`.
- `swagger-versoes.png` — Swagger UI mostrando os dois grupos (`v1`, `v2`) e a `v1`
  identificada como deprecada.
- `paginacao-400.json` — resposta de `page=0` ou `pageSize=9999` (400, `ProblemDetails`).
- `paginacao-paginas.json` — páginas 1 e 2 (com `pageSize` pequeno) mostrando itens
  distintos e `totalPages` coerente com `totalItems`.
- `rate-limit-429.json` (ou print) — resposta 429 de `POST /api/produtos` após estourar
  o limite, com o header `Retry-After` e o corpo JSON.
- `health-apos-429.json` — `GET /health` ainda `200` imediatamente depois do 429 acima.
- `dotnet-test-cp5.png` (ou saída de texto) — `dotnet test` com os testes do CP4 e os
  novos testes de paginação (`ParametrosPaginacaoTests`, `ProdutoServiceTests`) verdes.

Estes arquivos não foram gerados automaticamente nesta adaptação e devem ser
adicionados pelo grupo antes da entrega final (não é necessário para rodar o projeto).
