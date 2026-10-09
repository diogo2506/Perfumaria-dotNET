using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Perfumaria.Application.DTOs.Produtos;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Mappers;
using Perfumaria.Application.Services;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.API.Controllers;

/// <summary>
/// Produtos (perfumes) do catálogo. Usa o repositório específico <c>IProdutoRepository</c>
/// (que estende o repositório genérico) para leitura, e delega a criação/atualização
/// (que envolve validar Categoria, Fabricante e unicidade de SKU) ao
/// <see cref="IProdutoService"/>.
/// </summary>
/// <remarks>
/// Recurso escolhido para o versionamento do CP5: a listagem (<c>GET /api/produtos</c>)
/// convive em duas versões — <c>1.0</c> (deprecada, devolve o array simples do CP3) e
/// <c>2.0</c> (atual, devolve o envelope paginado). Os demais endpoints (busca por id/SKU,
/// estoque baixo, criação, atualização e remoção) não têm contrato alterado entre versões
/// e por isso não usam <c>[MapToApiVersion]</c> — ficam disponíveis em ambas.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoRepository produtoRepository, IProdutoService produtoService)
    {
        _produtoRepository = produtoRepository;
        _produtoService = produtoService;
    }

    /// <summary>
    /// [Deprecado] Lista todos os produtos (array simples), opcionalmente filtrando por
    /// categoria. Contrato idêntico ao do CP3 — mantido apenas para quem ainda consome a
    /// versão 1.0. Novas integrações devem usar a versão 2.0 (envelope paginado).
    /// </summary>
    /// <param name="categoriaId">Filtra produtos de uma categoria específica.</param>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAsyncV1([FromQuery] int? categoriaId)
    {
        var produtos = categoriaId.HasValue
            ? await _produtoRepository.ObterPorCategoriaAsync(categoriaId.Value)
            : await _produtoRepository.ObterTodosAsync();

        return Ok(produtos.Select(p => p.ToResponseDto()));
    }

    /// <summary>
    /// Lista produtos paginados (envelope com <c>page</c>, <c>pageSize</c>,
    /// <c>totalItems</c>, <c>totalPages</c> e <c>items</c>). A página é cortada no banco
    /// (<c>Skip</c>/<c>Take</c> sobre o <c>IQueryable</c>, ordenada por Nome).
    /// </summary>
    /// <param name="categoriaId">Filtra produtos de uma categoria específica.</param>
    /// <param name="page">Página solicitada (padrão 1, mínimo 1).</param>
    /// <param name="pageSize">Tamanho da página (padrão 20, de 1 a 100).</param>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ProdutoPaginadoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListarAsyncV2(
        [FromQuery] int? categoriaId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var pagina = await _produtoService.ListarPaginadoAsync(page, pageSize, categoriaId);
        return Ok(pagina);
    }

    /// <summary>Busca um produto pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdAsync(Guid id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Produto), id);

        return Ok(produto.ToResponseDto());
    }

    /// <summary>Busca um produto pelo código SKU.</summary>
    [HttpGet("sku/{sku}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorSkuAsync(string sku)
    {
        var produto = await _produtoRepository.ObterPorSkuAsync(sku)
            ?? throw new ResourceNotFoundException($"Produto com SKU '{sku}' não foi encontrado.");

        return Ok(produto.ToResponseDto());
    }

    /// <summary>Lista produtos com estoque abaixo do mínimo configurado.</summary>
    [HttpGet("estoque-baixo")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarComEstoqueBaixoAsync()
    {
        var produtos = await _produtoRepository.ObterComEstoqueBaixoAsync();
        return Ok(produtos.Select(p => p.ToResponseDto()));
    }

    /// <summary>Cadastra um novo produto (perfume).</summary>
    /// <remarks>
    /// Valida a existência de Categoria e Fabricante e a unicidade do CodigoSku
    /// antes de persistir. Sujeito a rate limit (CP5): no máximo 10 requisições por
    /// minuto, por IP de origem — ver política <c>escrita-fixa</c> no README.
    /// </remarks>
    [HttpPost]
    [EnableRateLimiting("escrita-fixa")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CriarAsync([FromBody] ProdutoRequestDto dto)
    {
        var criado = await _produtoService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorIdAsync), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um produto existente.</summary>
    [HttpPut("{id:guid}")]
    [EnableRateLimiting("escrita-fixa")]
    [ProducesResponseType(typeof(ProdutoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AtualizarAsync(Guid id, [FromBody] ProdutoRequestDto dto)
    {
        var atualizado = await _produtoService.AtualizarAsync(id, dto);
        return Ok(atualizado);
    }

    /// <summary>Remove um produto pelo id.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverAsync(Guid id)
    {
        if (!await _produtoRepository.ExisteAsync(id))
            throw new ResourceNotFoundException(nameof(Produto), id);

        await _produtoRepository.RemoverAsync(id);
        return NoContent();
    }
}
