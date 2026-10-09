using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;

namespace Perfumaria.Application.Interfaces.Repositories;

public interface IPedidoRepository : IRepository<Pedido, Guid>
{
    Task<Pedido?> ObterComItensAsync(Guid pedidoId);
    Task<IEnumerable<Pedido>> ObterPorClienteAsync(Guid clienteId);
    Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status);
}
