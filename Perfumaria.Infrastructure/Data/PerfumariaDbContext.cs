using Perfumaria.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Perfumaria.Infrastructure.Data;

public class PerfumariaDbContext : DbContext
{
    public PerfumariaDbContext(DbContextOptions<PerfumariaDbContext> options)
        : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Estoque> Estoques => Set<Estoque>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<ProdutoFornecedor> ProdutoFornecedores => Set<ProdutoFornecedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PerfumariaDbContext).Assembly);
    }
}
