using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;


namespace MinhaApi.Endpoints;
public static class DashboardEndpoint
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/dashboard").WithTags("Dashboard");

        group.MapGet("/resumo", async (AppDbContext db) =>
        {
            // Estatisticas gerais
            var totalProdutos = await db.Produtos.CountAsync();
            var valorTotal = await db.Produtos.SumAsync(p => (decimal?)p.Preco) ?? 0;
            var mediaPreco = totalProdutos > 0 ? valorTotal/totalProdutos : 0;

            // Top 5 produtos mais caros
            var topProdutos = await db.Produtos
                .OrderByDescending(p=>p.Preco)
                .Take(5)
                .Select(p => new { p.Nome, p.Preco })
                .ToArrayAsync();

            // Resumo por categoria
            var porCategoria = await db.Categorias
                .Select(c => new
                {
                    Categoria = c.Nome,
                    Quantidade = c.Produtos != null ? c.Produtos.Count : 0
                })
                .ToListAsync();

            return Results.Ok(new
            {
                DataRelatorio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                EstoqueGeral = new
                {
                    QuantidadeTotal = totalProdutos,
                    ValorTotalGeral = valorTotal,
                    TicketMedio = mediaPreco
                },
                RankingTop5 = topProdutos,
                DistribuicaoCategoria = porCategoria
            });
            
        });
    }
}