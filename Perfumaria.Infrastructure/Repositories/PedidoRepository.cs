using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Domain.Entities;
using Perfumaria.Domain.Enums;
using Perfumaria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Perfumaria.Infrastructure.Repositories;

public class PedidoRepository : Repository<Pedido, Guid>, IPedidoRepository
{
    public PedidoRepository(PerfumariaDbContext context) : base(context) { }

    public async Task<Pedido?> ObterComItensAsync(Guid pedidoId)
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

    public async Task<IEnumerable<Pedido>> ObterPorClienteAsync(Guid clienteId)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.ClienteId == clienteId)
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();

    public async Task<IEnumerable<Pedido>> ObterPorStatusAsync(StatusPedido status)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.Status == status)
            .Include(p => p.Cliente)
            .ToListAsync();
}
