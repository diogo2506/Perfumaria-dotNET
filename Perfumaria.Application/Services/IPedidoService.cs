using Perfumaria.Application.DTOs.Pedidos;

namespace Perfumaria.Application.Services;

/// <summary>
/// Serviço de aplicação para criação de pedidos de venda, concentrando a
/// orquestração entre Cliente, Produto e Estoque (que envolve múltiplos
/// repositórios e regras de domínio) fora do controller.
/// </summary>
public interface IPedidoService
{
    Task<PedidoResponseDto> CriarAsync(PedidoRequestDto dto, string traceId);
}
