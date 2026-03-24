using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MinhaApi.Data;
using MinhaApi.Models;
using MinhaApi.DTOs;

namespace MinhaApi.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/produtos").WithTags("Produtos");

        group.MapPost("/", async (AppDbContext db, IValidator<ProdutoCreateRequest> validator, ProdutoCreateRequest dados) =>
        {
            var validationResult = await validator.ValidateAsync(dados);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            var novoProduto = new Produto { Nome = dados.Nome, Preco = dados.Preco, CategoriaId = dados.CategoriaId };
            db.Produtos.Add(novoProduto);
            await db.SaveChangesAsync();
            return Results.Created($"/produtos/{novoProduto.Id}", novoProduto);
        });

        group.MapGet("/", async (AppDbContext db) => 
        {
            return await db.Produtos
                .Include(p => p.Categoria)
                .Select(p => new ProdutoResponse {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    CategoriaNome = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
                }).ToListAsync();
        });

        group.MapGet("/{id}", async (AppDbContext db, int id) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            return produto != null ? Results.Ok(produto) : Results.NotFound("Produto não existe.");
        });

        group.MapGet("/busca", async ([AsParameters] ProdutoGetRequest filtros, IValidator<ProdutoGetRequest> validator, AppDbContext db) =>
        {
            var validationResult = await validator.ValidateAsync(filtros);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            var consulta = db.Produtos.AsQueryable();
            if (!string.IsNullOrEmpty(filtros.Nome)) consulta = consulta.Where(p => p.Nome.Contains(filtros.Nome));
            if (filtros.CategoriaId.HasValue) consulta = consulta.Where(p => p.CategoriaId == filtros.CategoriaId);
            if (!string.IsNullOrEmpty(filtros.CategoriaNome)) consulta = consulta.Where(p => p.Categoria!.Nome.Contains(filtros.CategoriaNome));

            var totalItens = await consulta.CountAsync();
            var valorTotal = await consulta.SumAsync(p => (decimal?)p.Preco) ?? 0;
            int pular = (filtros.Pagina - 1) * filtros.Tamanho;

            var dados = await consulta.Skip(pular).Take(filtros.Tamanho)
                .Select(p => new ProdutoResponse {
                    Id = p.Id, Nome = p.Nome, Preco = p.Preco,
                    CategoriaNome = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
                }).ToListAsync();

            return Results.Ok(new { TotalItensFiltrados = totalItens, ValorTotalDoEstoque = valorTotal, Produtos = dados });
        });

        group.MapPut("/{id}", async (AppDbContext db, int id, ProdutoUpdateRequest dados, IValidator<ProdutoUpdateRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(dados);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            var produtoNoBanco = await db.Produtos.FindAsync(id);
            if (produtoNoBanco == null) return Results.NotFound();

            produtoNoBanco.Preco = dados.Preco;
            produtoNoBanco.CategoriaId = dados.CategoriaId;
            await db.SaveChangesAsync();
            return Results.Ok(produtoNoBanco);
        });

        group.MapDelete("/{id}", async (AppDbContext db, int id) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null) return Results.NotFound();
            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}