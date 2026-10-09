using Perfumaria.Application.DTOs.Produtos;
using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Mappers;

public static class ProdutoMapper
{
    public static ProdutoResponseDto ToResponseDto(this Produto produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        Descricao = produto.Descricao,
        CodigoSku = produto.CodigoSku,
        PrecoVenda = produto.PrecoVenda,
        VolumeMl = produto.VolumeMl,
        TipoFragancia = produto.TipoFragancia.ToString(),
        NotaOlfativa = produto.NotaOlfativa,
        CategoriaId = produto.CategoriaId,
        CategoriaNome = produto.Categoria?.Nome,
        FabricanteId = produto.FabricanteId,
        FabricanteNome = produto.Fabricante?.Nome
    };

    public static Produto ToEntity(this ProdutoRequestDto dto) => new()
    {
        Nome = dto.Nome,
        Descricao = dto.Descricao,
        CodigoSku = dto.CodigoSku,
        PrecoVenda = dto.PrecoVenda,
        VolumeMl = dto.VolumeMl,
        TipoFragancia = dto.TipoFragancia,
        NotaOlfativa = dto.NotaOlfativa,
        CategoriaId = dto.CategoriaId,
        FabricanteId = dto.FabricanteId
    };

    public static void ApplyTo(this ProdutoRequestDto dto, Produto entity)
    {
        entity.Nome = dto.Nome;
        entity.Descricao = dto.Descricao;
        entity.CodigoSku = dto.CodigoSku;
        entity.PrecoVenda = dto.PrecoVenda;
        entity.VolumeMl = dto.VolumeMl;
        entity.TipoFragancia = dto.TipoFragancia;
        entity.NotaOlfativa = dto.NotaOlfativa;
        entity.CategoriaId = dto.CategoriaId;
        entity.FabricanteId = dto.FabricanteId;
    }
}
