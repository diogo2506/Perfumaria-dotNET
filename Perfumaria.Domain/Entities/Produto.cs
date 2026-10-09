using Perfumaria.Domain.Common;
using Perfumaria.Domain.Enums;

namespace Perfumaria.Domain.Entities;

public class Produto : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nome { get; set; }
    public string? Descricao { get; set; }
    public required string CodigoSku { get; set; }
    public decimal PrecoVenda { get; set; }

    // Volume do frasco, em mililitros (ex.: 50, 100, 200)
    public decimal VolumeMl { get; set; }

    public TipoFragancia TipoFragancia { get; set; }

    // Notas olfativas predominantes (ex.: "Baunilha, Âmbar e Sândalo")
    public string? NotaOlfativa { get; set; }

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int FabricanteId { get; set; }
    public Fabricante Fabricante { get; set; } = null!;

    public Estoque? Estoque { get; set; }
    public ICollection<ProdutoFornecedor> ProdutoFornecedores { get; set; } = new List<ProdutoFornecedor>();
    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
}
