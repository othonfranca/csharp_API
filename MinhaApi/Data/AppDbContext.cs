using Microsoft.EntityFrameworkCore;
using MinhaApi.Models; // Importa a classe Categoria do arquivo separado

namespace MinhaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Força o tipo decimal para evitar o aviso anterior e possíveis erros de mapeamento
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasColumnType("decimal(18,2)");

        // 1. Seed de Categorias (IDs explícitos)
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Eletrônicos" },
            new Categoria { Id = 2, Nome = "Eletrodomésticos" },
            new Categoria { Id = 3, Nome = "Informática" },
            new Categoria { Id = 4, Nome = "Móveis" }
        );

        // 2. Seed de Produtos (20 itens para testar paginação)
        modelBuilder.Entity<Produto>().HasData(
            new Produto { Id = 1, Nome = "iPhone 15", Preco = 5999.00m, CategoriaId = 1 },
            new Produto { Id = 2, Nome = "Samsung Galaxy S23", Preco = 3800.00m, CategoriaId = 1 },
            new Produto { Id = 3, Nome = "Fone Bluetooth Sony", Preco = 1200.00m, CategoriaId = 1 },
            new Produto { Id = 4, Nome = "Carregador Rápido", Preco = 150.00m, CategoriaId = 1 },
            new Produto { Id = 5, Nome = "Smartwatch Apple", Preco = 2500.00m, CategoriaId = 1 },
            
            new Produto { Id = 6, Nome = "Geladeira Brastemp", Preco = 4200.00m, CategoriaId = 2 },
            new Produto { Id = 7, Nome = "Micro-ondas LG", Preco = 890.00m, CategoriaId = 2 },
            new Produto { Id = 8, Nome = "Fogão 4 Bocas", Preco = 1100.00m, CategoriaId = 2 },
            new Produto { Id = 9, Nome = "Máquina de Lavar 12kg", Preco = 2300.00m, CategoriaId = 2 },
            new Produto { Id = 10, Nome = "Aspirador Robô", Preco = 1500.00m, CategoriaId = 2 },

            new Produto { Id = 11, Nome = "Notebook Dell i7", Preco = 5200.00m, CategoriaId = 3 },
            new Produto { Id = 12, Nome = "Mouse Gamer Razer", Preco = 450.00m, CategoriaId = 3 },
            new Produto { Id = 13, Nome = "Monitor LG 29' UltraWide", Preco = 1300.00m, CategoriaId = 3 },
            new Produto { Id = 14, Nome = "Teclado Mecânico RGB", Preco = 350.00m, CategoriaId = 3 },
            new Produto { Id = 15, Nome = "Webcam Full HD", Preco = 280.00m, CategoriaId = 3 },

            new Produto { Id = 16, Nome = "Cadeira Gamer", Preco = 1200.00m, CategoriaId = 4 },
            new Produto { Id = 17, Nome = "Mesa de Escritório L", Preco = 650.00m, CategoriaId = 4 },
            new Produto { Id = 18, Nome = "Estante de Livros", Preco = 300.00m, CategoriaId = 4 },
            new Produto { Id = 19, Nome = "Luminária de Mesa", Preco = 85.00m, CategoriaId = 4 },
            new Produto { Id = 20, Nome = "Suporte Articulado Monitor", Preco = 190.00m, CategoriaId = 4 }
        );
    }
}