using Perfumaria.Domain.Common;

namespace Perfumaria.Domain.Entities;

// Ex.: Perfumaria Masculina, Feminina, Unissex, Infantil, Nichos
public class Categoria : IEntity<int>
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? Descricao { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
