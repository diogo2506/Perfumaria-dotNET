using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Interfaces.Repositories;

public interface IEstoqueRepository : IRepository<Estoque, int>
{
    Task<Estoque?> ObterPorProdutoAsync(Guid produtoId);
    Task<IEnumerable<Estoque>> ObterAbaixoDoMinimoAsync();
}
