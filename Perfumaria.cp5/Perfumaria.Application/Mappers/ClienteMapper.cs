using Perfumaria.Application.DTOs.Clientes;
using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Mappers;

public static class ClienteMapper
{
    public static ClienteResponseDto ToResponseDto(this Cliente cliente) => new()
    {
        Id = cliente.Id,
        Nome = cliente.Nome,
        CpfCnpj = cliente.CpfCnpj,
        Email = cliente.Email,
        Telefone = cliente.Telefone,
        Endereco = cliente.Endereco,
        DataCadastro = cliente.DataCadastro
    };

    public static Cliente ToEntity(this ClienteRequestDto dto) => new()
    {
        Nome = dto.Nome,
        CpfCnpj = dto.CpfCnpj,
        Email = dto.Email,
        Telefone = dto.Telefone,
        Endereco = dto.Endereco
    };

    public static void ApplyTo(this ClienteRequestDto dto, Cliente entity)
    {
        entity.Nome = dto.Nome;
        entity.CpfCnpj = dto.CpfCnpj;
        entity.Email = dto.Email;
        entity.Telefone = dto.Telefone;
        entity.Endereco = dto.Endereco;
    }
}
