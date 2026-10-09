using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class PedidoMapping : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.NumeroPedido)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.DataPedido).IsRequired();

        builder.Property(p => p.Status).IsRequired();

        builder.Property(p => p.ValorTotal)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(p => p.Observacoes)
            .HasMaxLength(500);

        builder.HasIndex(p => p.NumeroPedido).IsUnique();

        // N:1 → Cliente (obrigatório)
        builder.HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
