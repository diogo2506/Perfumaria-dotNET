namespace Perfumaria.Domain.Common;

/// <summary>
/// Marca uma entidade de domínio que possui uma chave primária simples do tipo <typeparamref name="TKey"/>.
/// Usada como restrição de tipo pelo repositório genérico (<c>IRepository&lt;TEntity, TKey&gt;</c>),
/// evitando o uso de repositórios genéricos com entidades sem identidade clara.
/// </summary>
/// <typeparam name="TKey">Tipo da chave primária (ex.: <see cref="int"/> ou <see cref="Guid"/>).</typeparam>
public interface IEntity<TKey>
{
    TKey Id { get; set; }
}
