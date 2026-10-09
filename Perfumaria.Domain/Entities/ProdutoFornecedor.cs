namespace Perfumaria.Domain.Entities;

// Tabela associativa N:N com dados extras (cotação de custo por fornecedor)
public class ProdutoFornecedor
{
    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;

    public decimal PrecoCusto { get; set; }
    public DateTime DataUltimaCotacao { get; set; }
    public int? PrazoEntregaDias { get; set; }
}
