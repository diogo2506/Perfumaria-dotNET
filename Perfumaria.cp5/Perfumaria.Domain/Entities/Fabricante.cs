using Perfumaria.Domain.Common;

namespace Perfumaria.Domain.Entities;

// Casa/maison perfumista responsável pela fabricação do produto
public class Fabricante : IEntity<int>
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Cnpj { get; set; }
    public string? PaisOrigem { get; set; }
    public string? Website { get; set; }

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
