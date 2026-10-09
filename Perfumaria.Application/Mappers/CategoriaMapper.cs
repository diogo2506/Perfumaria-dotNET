using Perfumaria.Application.DTOs.Categorias;
using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Mappers;

public static class CategoriaMapper
{
    public static CategoriaResponseDto ToResponseDto(this Categoria categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao
    };

    public static Categoria ToEntity(this CategoriaRequestDto dto) => new()
    {
        Nome = dto.Nome,
        Descricao = dto.Descricao
    };

    public static void ApplyTo(this CategoriaRequestDto dto, Categoria entity)
    {
        entity.Nome = dto.Nome;
        entity.Descricao = dto.Descricao;
    }
}
