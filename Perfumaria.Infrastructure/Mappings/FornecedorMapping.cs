using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class FornecedorMapping : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("Fornecedores");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.RazaoSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Cnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(f => f.Telefone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(f => f.Email)
            .HasMaxLength(150);

        builder.Property(f => f.Cidade)
            .HasMaxLength(100);

        builder.HasIndex(f => f.Cnpj).IsUnique();
    }
}
