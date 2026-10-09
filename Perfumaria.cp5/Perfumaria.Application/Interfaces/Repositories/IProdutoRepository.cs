using Perfumaria.Domain.Entities;

namespace Perfumaria.Application.Interfaces.Repositories;

public interface IProdutoRepository : IRepository<Produto, Guid>
{
    Task<Produto?> ObterPorSkuAsync(string sku);
    Task<IEnumerable<Produto>> ObterPorCategoriaAsync(int categoriaId);
    Task<IEnumerable<Produto>> ObterComEstoqueBaixoAsync();

    /// <summary>
    /// Obtém um produto rastreado (tracking habilitado) com o Estoque carregado,
    /// usado em fluxos que precisam ler e depois debitar a quantidade em estoque
    /// (ex.: criação de pedido).
    /// </summary>
    Task<Produto?> ObterParaVendaAsync(Guid id);

    /// <summary>
    /// Obtém uma página de produtos (CP5), cortada no banco com <c>Skip</c>/<c>Take</c>
    /// sobre o <c>IQueryable</c> e ordenada de forma estável (por <c>Nome</c>, com
    /// <c>Id</c> como critério de desempate), além do total de itens para o cálculo
    /// de <c>totalPages</c>. Usada exclusivamente pela listagem da versão 2.0 da API.
    /// </summary>
    /// <param name="page">Página solicitada (1-based), já validada em Application.</param>
    /// <param name="pageSize">Tamanho da página (1 a 100), já validado em Application.</param>
    /// <param name="categoriaId">Filtro opcional por categoria.</param>
    Task<(IReadOnlyList<Produto> Items, int TotalItems)> ObterPaginadoAsync(
        int page, int pageSize, int? categoriaId = null);
}
