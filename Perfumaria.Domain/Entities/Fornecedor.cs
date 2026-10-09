using Perfumaria.Domain.Common;

namespace Perfumaria.Domain.Entities;

public class Fornecedor : IEntity<int>
{
    public int Id { get; set; }
    public required string RazaoSocial { get; set; }
    public required string Cnpj { get; set; }
    public required string Telefone { get; set; }
    public string? Email { get; set; }
    public string? Cidade { get; set; }

    public ICollection<ProdutoFornecedor> ProdutoFornecedores { get; set; } = new List<ProdutoFornecedor>();
}
