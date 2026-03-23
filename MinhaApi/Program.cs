using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DO BANCO: Avisa ao .NET para usar o SQL Server com a nossa frase de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapGet("/produtos", async (AppDbContext db) => 
{
    try 
    {
        // Tenta executar a operação normal
        var lista = await db.Produtos.ToListAsync();
        // 2. Aqui você poderia adicionar qualquer lógica extra, 
        // como um log no console:
        Console.WriteLine($"Alguém consultou a lista. Total de itens: {lista.Count}");
        return Results.Ok(lista);
    }
    catch (Exception ex) 
    {
        // Se der erro (ex: banco fora do ar), o código pula para cá
        Console.WriteLine($"Erro crítico no banco: {ex.Message}");
        
        // Retorna um Erro 500 amigável para o Postman em vez de travar tudo
        return Results.Problem("Desculpe, não conseguimos conectar ao banco de dados agora.");
    }
});

app.MapPost("/produtos", async (AppDbContext db, Produto novo) => {
    db.Produtos.Add(novo);
    await db.SaveChangesAsync();
    return Results.Created($"/produtos/{novo.Id}", novo);
});

app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) => {
    // Primeiro, precisamos encontrar o produto no banco pelo ID
    var produto = await db.Produtos.FindAsync(id);

    if (produto is null)
    {
        return Results.NotFound("Produto não encontrado/inexistente. Verifique se o ID do produto está correto.");
    }

    db.Produtos.Remove(produto);
    await db.SaveChangesAsync();
    return Results.NoContent();

});

app.MapGet("/produtos/{id}", async (AppDbContext db, int id) =>
{
    try
    {
        var produto = await db.Produtos.FindAsync(id);
        if (produto != null)
        {
            return Results.Ok(produto);
        }
        else
        {
            return Results.NotFound("Produto não existe no banco.");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem("Erro ao buscar produto: " + ex.Message);
    }
});

app.MapPut("/produtos/{id}", async (int id, Produto produtoAlterado, AppDbContext db) =>
{
    try
    {
        var produtoNoBanco = await db.Produtos.FindAsync(id);

        if (produtoNoBanco == null)
        {
            return Results.NotFound($"Produto com id {id} não encontrado para atualização.");
        }

        produtoNoBanco.Nome = produtoAlterado.Nome;
        produtoNoBanco.Preco = produtoAlterado.Preco;

        await db.SaveChangesAsync();

        return Results.Ok(produtoNoBanco);

    }
    catch (Exception ex)
    {
        return Results.Problem("Erro ao atualizar produto: " + ex.Message);
    }
});

app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) =>
{
    try
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
        {
            return Results.NotFound($"Produto com id {id} não encontrado para exclusão.");
        }

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem("Erro ao excluir produto: " + ex.Message);
    }
});

app.Run();
public class Produto 
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}