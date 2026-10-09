using System.ComponentModel.DataAnnotations;
using Perfumaria.Domain.Enums;

namespace Perfumaria.Application.DTOs.Produtos;

/// <summary>Payload de entrada para criação/atualização de um produto (perfume).</summary>
public class ProdutoRequestDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Descricao { get; set; }

    [Required, StringLength(50)]
    public string CodigoSku { get; set; } = string.Empty;

    [Range(0.01, 100000)]
    public decimal PrecoVenda { get; set; }

    [Range(1, 5000)]
    public decimal VolumeMl { get; set; }

    [Required]
    public TipoFragancia TipoFragancia { get; set; }

    [StringLength(200)]
    public string? NotaOlfativa { get; set; }

    [Required]
    public int CategoriaId { get; set; }

    [Required]
    public int FabricanteId { get; set; }
}

/// <summary>Payload de saída representando um produto (perfume).</summary>
public class ProdutoResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public decimal PrecoVenda { get; set; }
    public decimal VolumeMl { get; set; }
    public string TipoFragancia { get; set; } = string.Empty;
    public string? NotaOlfativa { get; set; }
    public int CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public int FabricanteId { get; set; }
    public string? FabricanteNome { get; set; }
}

/// <summary>
/// Envelope de paginação (CP5) retornado pela listagem de produtos na versão 2.0 da API.
/// A versão 1.0 (deprecada) continua devolvendo o array simples de <see cref="ProdutoResponseDto"/>.
/// </summary>
public class ProdutoPaginadoResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public List<ProdutoResponseDto> Items { get; set; } = new();
}
