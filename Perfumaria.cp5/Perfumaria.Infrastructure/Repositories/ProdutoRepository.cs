using Perfumaria.Application.Interfaces.Repositories;
using Perfumaria.Domain.Entities;
using Perfumaria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Perfumaria.Infrastructure.Repositories;

public class ProdutoRepository : Repository<Produto, Guid>, IProdutoRepository
{
    public ProdutoRepository(PerfumariaDbContext context) : base(context) { }

    public async Task<Produto?> ObterPorSkuAsync(string sku)
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fabricante)
            .FirstOrDefaultAsync(p => p.CodigoSku == sku);

    public async Task<IEnumerable<Produto>> ObterPorCategoriaAsync(int categoriaId)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.CategoriaId == categoriaId)
            .Include(p => p.Fabricante)
            .ToListAsync();

    public async Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync()
        => await _dbSet
            .AsNoTracking()
            .Include(p => p.Estoque)
            .Where(p => p.Estoque != null && p.Estoque.QuantidadeAtual < p.Estoque.QuantidadeMinima)
            .ToListAsync();

    public async Task<Produto?> ObterParaVendaAsync(Guid id)
        => await _dbSet
            .Include(p => p.Estoque)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<(IReadOnlyList<Produto> Items, int TotalItems)> ObterPaginadoAsync(
        int page, int pageSize, int? categoriaId = null)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Fabricante)
            .AsQueryable();

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);

        // Ordenação estável obrigatória (CP5): Nome + Id como critério de desempate,
        // garantindo que a página 1 e a página 2 nunca se sobreponham.
        return await ObterPaginadoAsync(query, q => q.OrderBy(p => p.Nome).ThenBy(p => p.Id), page, pageSize);
    }
}
