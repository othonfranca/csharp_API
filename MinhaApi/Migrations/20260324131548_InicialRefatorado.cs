using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MinhaApi.Migrations
{
    /// <inheritdoc />
    public partial class InicialRefatorado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Eletrônicos" },
                    { 2, "Eletrodomésticos" },
                    { 3, "Informática" },
                    { 4, "Móveis" }
                });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "CategoriaId", "Nome", "Preco" },
                values: new object[,]
                {
                    { 1, 1, "iPhone 15", 5999.00m },
                    { 2, 1, "Samsung Galaxy S23", 3800.00m },
                    { 3, 1, "Fone Bluetooth Sony", 1200.00m },
                    { 4, 1, "Carregador Rápido", 150.00m },
                    { 5, 1, "Smartwatch Apple", 2500.00m },
                    { 6, 2, "Geladeira Brastemp", 4200.00m },
                    { 7, 2, "Micro-ondas LG", 890.00m },
                    { 8, 2, "Fogão 4 Bocas", 1100.00m },
                    { 9, 2, "Máquina de Lavar 12kg", 2300.00m },
                    { 10, 2, "Aspirador Robô", 1500.00m },
                    { 11, 3, "Notebook Dell i7", 5200.00m },
                    { 12, 3, "Mouse Gamer Razer", 450.00m },
                    { 13, 3, "Monitor LG 29' UltraWide", 1300.00m },
                    { 14, 3, "Teclado Mecânico RGB", 350.00m },
                    { 15, 3, "Webcam Full HD", 280.00m },
                    { 16, 4, "Cadeira Gamer", 1200.00m },
                    { 17, 4, "Mesa de Escritório L", 650.00m },
                    { 18, 4, "Estante de Livros", 300.00m },
                    { 19, 4, "Luminária de Mesa", 85.00m },
                    { 20, 4, "Suporte Articulado Monitor", 190.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
