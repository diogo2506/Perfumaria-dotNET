using System.ComponentModel.DataAnnotations;

namespace Perfumaria.Application.DTOs.Categorias;

/// <summary>Payload de entrada para criação/atualização de uma categoria de perfumes.</summary>
public class CategoriaRequestDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }
}

/// <summary>Payload de saída representando uma categoria de perfumes.</summary>
public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
