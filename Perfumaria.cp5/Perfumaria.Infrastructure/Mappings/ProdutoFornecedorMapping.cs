using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Perfumaria.Infrastructure.Mappings;

public class ProdutoFornecedorMapping : IEntityTypeConfiguration<ProdutoFornecedor>
{
    public void Configure(EntityTypeBuilder<ProdutoFornecedor> builder)
    {
        builder.ToTable("ProdutoFornecedores");

        // Chave primária composta
        builder.HasKey(pf => new { pf.ProdutoId, pf.FornecedorId });

        builder.Property(pf => pf.PrecoCusto)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(pf => pf.DataUltimaCotacao).IsRequired();

        builder.Property(pf => pf.PrazoEntregaDias);

        // N:N → Produto
        builder.HasOne(pf => pf.Produto)
            .WithMany(p => p.ProdutoFornecedores)
            .HasForeignKey(pf => pf.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        // N:N → Fornecedor
        builder.HasOne(pf => pf.Fornecedor)
            .WithMany(f => f.ProdutoFornecedores)
            .HasForeignKey(pf => pf.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
