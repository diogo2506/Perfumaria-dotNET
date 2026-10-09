using Perfumaria.Domain.Common;

namespace Perfumaria.Domain.Entities;

public class Cliente : IEntity<Guid>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Nome { get; set; }
    public required string CpfCnpj { get; set; }
    public required string Email { get; set; }
    public required string Telefone { get; set; }
    public string? Endereco { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
