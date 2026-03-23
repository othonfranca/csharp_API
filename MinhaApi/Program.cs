using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => 
{
    // Isso evita que o JSON tente entrar em um loop infinito 
    // entre Produto -> Categoria -> Produto
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 1. REGISTRO DO BANCO: Avisa ao .NET para usar o SQL Server com a nossa frase de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
{
    // 1. Validação: O nome não pode ser vazio e o preço deve ser maior que zero
    if (string.IsNullOrWhiteSpace(produto.Nome) || produto.Preco <= 0 || produto.CategoriaId <= 0)
    {
        return Results.BadRequest("Dados inválidos: O nome é obrigatório, o preço deve ser maior que zero e a categoria é obrigatória.");
    }

    try 
    {
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();
        return Results.Created($"/produtos/{produto.Id}", produto);
    }
    catch (Exception ex) 
    {
        return Results.Problem("Erro ao salvar no banco: " + ex.Message);
    }
});

app.MapGet("/produtos", async (AppDbContext db) => 
{
    try 
    {
        // Tenta executar a operação normal
        var lista = await db.Produtos
        .Include(p => p.Categoria)
        .ToListAsync();
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

// Agora o GET aceita Nome (opcional), Página e Tamanho
app.MapGet("/produtos/busca", async (string? nome, int? categoriaId, string? categoriaNome, int pagina, int tamanho, AppDbContext db) =>
{
    // 1. Iniciamos a consulta incluindo a Categoria
    var consulta = db.Produtos.Include(p => p.Categoria).AsQueryable();

    // 2. Filtro por Nome do Produto
    if (!string.IsNullOrEmpty(nome))
    {
        consulta = consulta.Where(p => p.Nome.Contains(nome));
    }

    // 3. NOVO: Filtro por ID da Categoria
    if (categoriaId.HasValue)
    {
        consulta = consulta.Where(p => p.CategoriaId == categoriaId);
    }

    // 4. NOVO: Filtro por Nome da Categoria
    if (!string.IsNullOrEmpty(categoriaNome))
    {
        // Buscamos produtos onde o nome da categoria vinculada contenha o texto
        consulta = consulta.Where(p => p.Categoria.Nome.Contains(categoriaNome));
    }

    // 5. Paginação e Contagem (Igual ao anterior)
    var totalItens = await consulta.CountAsync();
    
    if (tamanho <= 0) tamanho = 10;
    if (pagina <= 0) pagina = 1;

    int totalPaginas = (int)Math.Ceiling((double)totalItens / tamanho);
    int pular = (pagina - 1) * tamanho;

    var dados = await consulta.Skip(pular).Take(tamanho).ToListAsync();

    return Results.Ok(new {
        TotalGeral = totalItens,
        TotalDePaginas = totalPaginas,
        PaginaAtual = pagina,
        Dados = dados
    });
});

app.MapPut("/produtos/{id}", async (AppDbContext db, int id, Produto produtoAlterado) =>
{
    if (string.IsNullOrWhiteSpace(produtoAlterado.Nome) || produtoAlterado.Preco <= 0)
    {
        return Results.BadRequest("Dados inválidos: O nome é obrigatório e o preço deve ser maior que zero.");
    }

    try
    {
        var produtoNoBanco = await db.Produtos.FindAsync(id);

        if (produtoNoBanco == null)
        {
            return Results.NotFound($"Produto com id {id} não encontrado para atualização.");
        }

        produtoNoBanco.Nome = produtoAlterado.Nome;
        produtoNoBanco.Preco = produtoAlterado.Preco;
        produtoNoBanco.CategoriaId = produtoAlterado.CategoriaId;

        await db.SaveChangesAsync();

        return Results.Ok(produtoNoBanco);

    }
    catch (Exception ex)
    {
        return Results.Problem("Erro ao atualizar produto: " + ex.Message);
    }
});

app.MapDelete("/produtos/{id}", async (AppDbContext db, int id) =>
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

// Rota para cadastrar novas categorias
app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
{
    // Validação simples: não aceita nome vazio
    if (string.IsNullOrWhiteSpace(categoria.Nome))
    {
        return Results.BadRequest("O nome da categoria é obrigatório.");
    }

    db.Categorias.Add(categoria);
    await db.SaveChangesAsync();

    // Retorna 201 Created e o objeto que foi criado (com o ID gerado pelo banco)
    return Results.Created($"/categorias/{categoria.Id}", categoria);
});

// Rota extra: Útil para você ver quais IDs já existem
app.MapGet("/categorias", async (AppDbContext db) => 
    await db.Categorias.ToListAsync());

app.Run();
public class Produto 
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    // Chave Estrangeira (O ID da Categoria no SQL)
    public int CategoriaId { get; set; }
    // Propriedade de Navegação (Para o C# conseguir ler os dados da categoria)
    public Categoria? Categoria { get; set; }
}

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    // Um "atalho" para o Entity Framework saber que uma categoria tem muitos produtos
    public List<Produto> Produtos { get; set; } = new();
}