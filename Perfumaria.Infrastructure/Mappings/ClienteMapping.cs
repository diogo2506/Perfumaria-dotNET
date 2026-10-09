using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class ClienteMapping : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CpfCnpj)
            .IsRequired()
            .HasMaxLength(18);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Telefone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Endereco)
            .HasMaxLength(300);

        builder.Property(c => c.DataCadastro).IsRequired();

        builder.HasIndex(c => c.CpfCnpj).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
    }
}
