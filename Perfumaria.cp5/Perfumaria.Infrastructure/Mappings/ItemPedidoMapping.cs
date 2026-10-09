using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class ItemPedidoMapping : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.Quantidade).IsRequired();

        builder.Property(i => i.PrecoUnitario)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.DescontoPercentual)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        // N:1 → Pedido (obrigatório)
        builder.HasOne(i => i.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        // N:1 → Produto (obrigatório)
        builder.HasOne(i => i.Produto)
            .WithMany(p => p.ItensPedido)
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
