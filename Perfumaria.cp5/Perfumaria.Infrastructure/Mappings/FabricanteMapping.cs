using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class FabricanteMapping : IEntityTypeConfiguration<Fabricante>
{
    public void Configure(EntityTypeBuilder<Fabricante> builder)
    {
        builder.ToTable("Fabricantes");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(f => f.Cnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(f => f.PaisOrigem)
            .HasMaxLength(60);

        builder.Property(f => f.Website)
            .HasMaxLength(200);

        builder.HasIndex(f => f.Cnpj).IsUnique();
    }
}
