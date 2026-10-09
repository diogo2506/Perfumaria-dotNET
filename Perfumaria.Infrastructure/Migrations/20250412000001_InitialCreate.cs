using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Perfumaria.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Categorias",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categorias", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Clientes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                CpfCnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                DataCadastro = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Clientes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Fabricantes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                PaisOrigem = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                Website = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Fabricantes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Fornecedores",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: false),
                Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                Cidade = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Fornecedores", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Pedidos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                NumeroPedido = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                DataPedido = table.Column<DateTime>(type: "TEXT", nullable: false),
                Status = table.Column<int>(type: "INTEGER", nullable: false),
                ValorTotal = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                ClienteId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Pedidos", x => x.Id);
                table.ForeignKey(
                    name: "FK_Pedidos_Clientes_ClienteId",
                    column: x => x.ClienteId,
                    principalTable: "Clientes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Produtos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                CodigoSku = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                PrecoVenda = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                VolumeMl = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                TipoFragancia = table.Column<int>(type: "INTEGER", nullable: false),
                NotaOlfativa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                FabricanteId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Produtos", x => x.Id);
                table.ForeignKey(
                    name: "FK_Produtos_Categorias_CategoriaId",
                    column: x => x.CategoriaId,
                    principalTable: "Categorias",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Produtos_Fabricantes_FabricanteId",
                    column: x => x.FabricanteId,
                    principalTable: "Fabricantes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Estoques",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                QuantidadeAtual = table.Column<int>(type: "INTEGER", nullable: false),
                QuantidadeMinima = table.Column<int>(type: "INTEGER", nullable: false),
                Localizacao = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                UltimaAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Estoques", x => x.Id);
                table.ForeignKey(
                    name: "FK_Estoques_Produtos_ProdutoId",
                    column: x => x.ProdutoId,
                    principalTable: "Produtos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ItensPedido",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                PrecoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                DescontoPercentual = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                PedidoId = table.Column<Guid>(type: "TEXT", nullable: false),
                ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ItensPedido", x => x.Id);
                table.ForeignKey(
                    name: "FK_ItensPedido_Pedidos_PedidoId",
                    column: x => x.PedidoId,
                    principalTable: "Pedidos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ItensPedido_Produtos_ProdutoId",
                    column: x => x.ProdutoId,
                    principalTable: "Produtos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ProdutoFornecedores",
            columns: table => new
            {
                ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                FornecedorId = table.Column<int>(type: "INTEGER", nullable: false),
                PrecoCusto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                DataUltimaCotacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                PrazoEntregaDias = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProdutoFornecedores", x => new { x.ProdutoId, x.FornecedorId });
                table.ForeignKey(
                    name: "FK_ProdutoFornecedores_Fornecedores_FornecedorId",
                    column: x => x.FornecedorId,
                    principalTable: "Fornecedores",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ProdutoFornecedores_Produtos_ProdutoId",
                    column: x => x.ProdutoId,
                    principalTable: "Produtos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Índices únicos
        migrationBuilder.CreateIndex(name: "IX_Categorias_Nome",        table: "Categorias",        column: "Nome",        unique: true);
        migrationBuilder.CreateIndex(name: "IX_Clientes_CpfCnpj",       table: "Clientes",          column: "CpfCnpj",     unique: true);
        migrationBuilder.CreateIndex(name: "IX_Clientes_Email",         table: "Clientes",          column: "Email",       unique: true);
        migrationBuilder.CreateIndex(name: "IX_Fabricantes_Cnpj",       table: "Fabricantes",       column: "Cnpj",        unique: true);
        migrationBuilder.CreateIndex(name: "IX_Fornecedores_Cnpj",      table: "Fornecedores",      column: "Cnpj",        unique: true);
        migrationBuilder.CreateIndex(name: "IX_Produtos_CodigoSku",     table: "Produtos",          column: "CodigoSku",   unique: true);
        migrationBuilder.CreateIndex(name: "IX_Produtos_CategoriaId",   table: "Produtos",          column: "CategoriaId");
        migrationBuilder.CreateIndex(name: "IX_Produtos_FabricanteId",  table: "Produtos",          column: "FabricanteId");
        migrationBuilder.CreateIndex(name: "IX_Estoques_ProdutoId",     table: "Estoques",          column: "ProdutoId",   unique: true);
        migrationBuilder.CreateIndex(name: "IX_Pedidos_ClienteId",      table: "Pedidos",           column: "ClienteId");
        migrationBuilder.CreateIndex(name: "IX_Pedidos_NumeroPedido",   table: "Pedidos",           column: "NumeroPedido", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ItensPedido_PedidoId",   table: "ItensPedido",       column: "PedidoId");
        migrationBuilder.CreateIndex(name: "IX_ItensPedido_ProdutoId",  table: "ItensPedido",       column: "ProdutoId");
        migrationBuilder.CreateIndex(name: "IX_ProdutoFornecedores_FornecedorId", table: "ProdutoFornecedores", column: "FornecedorId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ItensPedido");
        migrationBuilder.DropTable(name: "ProdutoFornecedores");
        migrationBuilder.DropTable(name: "Estoques");
        migrationBuilder.DropTable(name: "Pedidos");
        migrationBuilder.DropTable(name: "Produtos");
        migrationBuilder.DropTable(name: "Clientes");
        migrationBuilder.DropTable(name: "Fornecedores");
        migrationBuilder.DropTable(name: "Fabricantes");
        migrationBuilder.DropTable(name: "Categorias");
    }
}
