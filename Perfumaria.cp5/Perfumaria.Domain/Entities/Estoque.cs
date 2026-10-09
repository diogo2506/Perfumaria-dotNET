using Perfumaria.Domain.Common;
using Perfumaria.Domain.Exceptions;

namespace Perfumaria.Domain.Entities;

public class Estoque : IEntity<int>
{
    public int Id { get; set; }
    public int QuantidadeAtual { get; set; }
    public int QuantidadeMinima { get; set; }
    public required string Localizacao { get; set; }
    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;

    public Guid ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    /// <summary>
    /// Debita <paramref name="quantidade"/> unidades do estoque (ex.: ao confirmar um
    /// item de pedido). Regra de negócio: a quantidade solicitada deve ser positiva e
    /// não pode exceder a quantidade atualmente disponível.
    /// </summary>
    /// <exception cref="DomainException">
    /// Lançada quando <paramref name="quantidade"/> é menor ou igual a zero, ou quando
    /// excede <see cref="QuantidadeAtual"/>.
    /// </exception>
    public void Debitar(int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade a debitar do estoque deve ser maior que zero.");

        if (quantidade > QuantidadeAtual)
        {
            throw new DomainException(
                $"Estoque insuficiente. Disponível: {QuantidadeAtual}, solicitado: {quantidade}.");
        }

        QuantidadeAtual -= quantidade;
        UltimaAtualizacao = DateTime.UtcNow;
    }
}
