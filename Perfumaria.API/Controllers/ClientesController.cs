using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Perfumaria.Application.DTOs.Clientes;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Mappers;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.API.Controllers;

/// <summary>
/// Clientes da perfumaria. Não participa do versionamento do CP5
/// (<c>[ApiVersionNeutral]</c>): contrato inalterado entre versões.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersionNeutral]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;

    public ClientesController(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    /// <summary>Lista todos os clientes cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAsync()
    {
        var clientes = await _clienteRepository.ObterTodosAsync();
        return Ok(clientes.Select(c => c.ToResponseDto()));
    }

    /// <summary>Busca um cliente pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdAsync(Guid id)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Cliente), id);

        return Ok(cliente.ToResponseDto());
    }

    /// <summary>Cadastra um novo cliente.</summary>
    /// <remarks>Valida a unicidade de CPF/CNPJ e e-mail antes de persistir.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarAsync([FromBody] ClienteRequestDto dto)
    {
        if (await _clienteRepository.ObterPorCpfCnpjAsync(dto.CpfCnpj) is not null)
            throw new ConflictException($"Já existe um cliente com o CPF/CNPJ '{dto.CpfCnpj}'.");

        if (await _clienteRepository.ObterPorEmailAsync(dto.Email) is not null)
            throw new ConflictException($"Já existe um cliente com o e-mail '{dto.Email}'.");

        var cliente = dto.ToEntity();
        await _clienteRepository.AdicionarAsync(cliente);

        return CreatedAtAction(nameof(ObterPorIdAsync), new { id = cliente.Id }, cliente.ToResponseDto());
    }

    /// <summary>Lista os pedidos de um cliente.</summary>
    [HttpGet("{id:guid}/pedidos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarPedidosAsync(Guid id)
    {
        if (!await _clienteRepository.ExisteAsync(id))
            throw new ResourceNotFoundException(nameof(Cliente), id);

        var clientes = await _clienteRepository.ObterComPedidosAsync();
        var cliente = clientes.FirstOrDefault(c => c.Id == id)
            ?? throw new ResourceNotFoundException(nameof(Cliente), id);

        return Ok(cliente.Pedidos.Select(p => new
        {
            p.Id,
            p.NumeroPedido,
            p.DataPedido,
            Status = p.Status.ToString(),
            p.ValorTotal
        }));
    }
}
