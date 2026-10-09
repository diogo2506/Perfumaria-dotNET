using System.ComponentModel.DataAnnotations;

namespace Perfumaria.Application.DTOs.Clientes;

/// <summary>Payload de entrada para criação/atualização de um cliente.</summary>
public class ClienteRequestDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(18)]
    public string CpfCnpj { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Endereco { get; set; }
}

/// <summary>Payload de saída representando um cliente.</summary>
public class ClienteResponseDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public DateTime DataCadastro { get; set; }
}
