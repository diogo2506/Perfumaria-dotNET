using Perfumaria.Domain.Common;

namespace Perfumaria.Application.Interfaces.Repositories;

/// <summary>
/// Repositório genérico (CP3) para operações de CRUD mínimas, aplicável a qualquer
/// entidade de domínio que exponha uma chave simples via <see cref="IEntity{TKey}"/>.
/// Registrado na DI como <c>AddScoped(typeof(IRepository&lt;,&gt;), typeof(Repository&lt;,&gt;))</c>.
/// Convive com repositórios específicos por agregado (ex.: <c>IProdutoRepository</c>)
/// quando o agregado precisa de consultas além do CRUD básico.
/// </summary>
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> ObterPorIdAsync(TKey id);
    Task<IEnumerable<TEntity>> ObterTodosAsync();
    Task AdicionarAsync(TEntity entity);
    Task AtualizarAsync(TEntity entity);
    Task RemoverAsync(TKey id);
    Task<bool> ExisteAsync(TKey id);
}
