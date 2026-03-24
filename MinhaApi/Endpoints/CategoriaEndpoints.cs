using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MinhaApi.Data;
using MinhaApi.Models;
using MinhaApi.DTOs;

namespace MinhaApi.Endpoints;

public static class CategoriaEndpoints
{
    public static void MapCategoriaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/categorias").WithTags("Categorias");

        group.MapPost("/", async (AppDbContext db, IValidator<CategoriaCreateRequest> validator, CategoriaCreateRequest dados) => {
            var validationResult = await validator.ValidateAsync(dados);
            if (!validationResult.IsValid) return Results.ValidationProblem(validationResult.ToDictionary());

            var nova = new Categoria { Nome = dados.Nome };
            db.Categorias.Add(nova);
            await db.SaveChangesAsync();
            return Results.Created($"/categorias/{nova.Id}", nova);
        });

        group.MapGet("/", async (AppDbContext db) => await db.Categorias.ToListAsync());

        group.MapPut("/{id}", async (AppDbContext db, int id, CategoriaUpdateRequest dados) => {
            var categoria = await db.Categorias.FindAsync(id);
            if (categoria == null) return Results.NotFound();
            categoria.Nome = dados.Nome;
            await db.SaveChangesAsync();
            return Results.Ok(categoria);
        });

        group.MapDelete("/{id}", async (AppDbContext db, int id) => {
            var categoria = await db.Categorias.FindAsync(id);
            if (categoria == null) return Results.NotFound();
            if (await db.Produtos.AnyAsync(p => p.CategoriaId == id)) return Results.Conflict("Categoria possui produtos.");

            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}