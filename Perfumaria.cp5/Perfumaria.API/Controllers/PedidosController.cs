using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Perfumaria.Application.DTOs.Pedidos;
using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Application.Mappers;
using Perfumaria.Application.Services;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.API.Controllers;

/// <summary>
/// Pedidos de venda. Não participa do versionamento do CP5
/// (<c>[ApiVersionNeutral]</c>): contrato inalterado entre versões.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiVersionNeutral]
public class PedidosController : ControllerBase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoRepository pedidoRepository, IPedidoService pedidoService)
    {
        _pedidoRepository = pedidoRepository;
        _pedidoService = pedidoService;
    }

    /// <summary>Lista todos os pedidos, opcionalmente filtrando por status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PedidoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAsync([FromQuery] StatusPedido? status)
    {
        var pedidos = status.HasValue
            ? await _pedidoRepository.ObterPorStatusAsync(status.Value)
            : await _pedidoRepository.ObterTodosAsync();

        return Ok(pedidos.Select(p => p.ToResponseDto()));
    }

    /// <summary>Busca um pedido pelo id, incluindo seus itens.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorIdAsync(Guid id)
    {
        var pedido = await _pedidoRepository.ObterComItensAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Pedido), id);

        return Ok(pedido.ToResponseDto());
    }

    /// <summary>
    /// Cria um novo pedido de venda a partir do cliente e da lista de itens informados.
    /// </summary>
    /// <remarks>
    /// Regras aplicadas (no <c>Perfumaria.Application.Services.PedidoService</c>): cliente e
    /// produtos devem existir; o preço unitário é obtido como snapshot do preço de venda
    /// atual do produto; o estoque de cada produto é debitado e não pode ficar negativo.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CriarAsync([FromBody] PedidoRequestDto dto)
    {
        var criado = await _pedidoService.CriarAsync(dto, HttpContext.TraceIdentifier);
        return CreatedAtAction(nameof(ObterPorIdAsync), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza o status de um pedido (ex.: Confirmado, Enviado, Entregue, Cancelado).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatusAsync(Guid id, [FromBody] StatusPedido status)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id)
            ?? throw new ResourceNotFoundException(nameof(Pedido), id);

        pedido.Status = status;
        await _pedidoRepository.AtualizarAsync(pedido);

        var atualizado = await _pedidoRepository.ObterComItensAsync(id);
        return Ok(atualizado!.ToResponseDto());
    }
}
