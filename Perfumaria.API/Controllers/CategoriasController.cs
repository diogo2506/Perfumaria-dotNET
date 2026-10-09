using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Perfumaria.Application.DTOs.Categorias;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Mappers;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.API.Controllers;

/// <summary>
/// Categorias de perfumes (ex.: Masculina, Feminina, Unissex).
/// Demonstra o uso direto do repositório genérico <c>IRepository&lt;Categoria, int&gt;</c>,
/// registrado via <c>AddScoped(typeof(IRepository&lt;,&gt;), typeof(Repository&lt;,&gt;))</c>.
/// Não participa do versionamento do CP5 (<c>[ApiVersionNeutral]</c>): seu contrato não
/// muda entre versões e o recurso segue respondendo normalmente nas duas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersionNeutral]
public class CategoriasController : ControllerBase
{
    private readonly IRepository<Categoria, int> _repository;

    public CategoriasController(IRepository<Categoria, int> repository)
    {
        _repository = repository;
    }

    /// <summary>Lista todas as categorias cadastradas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAsync()
    {
        var categorias = await _repository.ObterTodosAsync();
        return Ok(categorias.Select(c => c.ToResponseDto()));
    }

    /// <summary>Busca uma categoria pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdAsync(int id)
    {
        var categoria = await _repository.ObterPorIdAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Categoria), id);

        return Ok(categoria.ToResponseDto());
    }

    /// <summary>Cadastra uma nova categoria.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarAsync([FromBody] CategoriaRequestDto dto)
    {
        var categoria = dto.ToEntity();
        await _repository.AdicionarAsync(categoria);

        return CreatedAtAction(nameof(ObterPorIdAsync), new { id = categoria.Id }, categoria.ToResponseDto());
    }

    /// <summary>Atualiza uma categoria existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarAsync(int id, [FromBody] CategoriaRequestDto dto)
    {
        var categoria = await _repository.ObterPorIdAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Categoria), id);

        dto.ApplyTo(categoria);
        await _repository.AtualizarAsync(categoria);

        return Ok(categoria.ToResponseDto());
    }

    /// <summary>Remove uma categoria pelo id.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverAsync(int id)
    {
        if (!await _repository.ExisteAsync(id))
            throw new ResourceNotFoundException(nameof(Categoria), id);

        await _repository.RemoverAsync(id);
        return NoContent();
    }
}
