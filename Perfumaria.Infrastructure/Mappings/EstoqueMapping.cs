using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class EstoqueMapping : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoques");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.QuantidadeAtual).IsRequired();
        builder.Property(e => e.QuantidadeMinima).IsRequired();

        builder.Property(e => e.Localizacao)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.UltimaAtualizacao).IsRequired();

        // 1:1 → Produto (obrigatório)
        builder.HasOne(e => e.Produto)
            .WithOne(p => p.Estoque)
            .HasForeignKey<Estoque>(e => e.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProdutoId).IsUnique();
    }
}
