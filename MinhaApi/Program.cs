using Microsoft.EntityFrameworkCore;
using MinhaApi.Models; // Importa a classe Categoria do arquivo separado
using MinhaApi.Data; // Importa o AppDbContext do arquivo separado
using MinhaApi.Converters;
using MinhaApi.DTOs; // Importa o DecimalConverter do arquivo separado

var builder = WebApplication.CreateBuilder(args);

// CONFIGURANDO SWAGGER PARA DOCUMENTAÇÃO AUTOMÁTICA
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// CONFIGURAÇÕES GLOBAIS DE SERIALIZAÇÃO JSON
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => 
{
    // Isso evita que o JSON tente entrar em um loop infinito 
    // entre Produto -> Categoria -> Produto
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    
    // Força 2 casas decimais em todos os decimais da API
    options.SerializerOptions.Converters.Add(new DecimalConverter());
});

// 1. REGISTRO DO BANCO: Avisa ao .NET para usar o SQL Server com a nossa frase de conexão
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/produtos", async (ProdutoCreateRequest dados, AppDbContext db) =>
{
    // 1. Validação básica (usando os dados do DTO)
    if (string.IsNullOrWhiteSpace(dados.Nome) || dados.Preco <= 0 || dados.CategoriaId <= 0)
    {
        return Results.BadRequest("Dados inválidos.");
    }

    // 2. Transformamos o DTO na classe de Banco (Produto)
    var novoProduto = new Produto
    {
        Nome = dados.Nome,
        Preco = dados.Preco,
        CategoriaId = dados.CategoriaId
        // O Id e o objeto Categoria ficam nulos aqui, 
        // o Entity Framework resolve isso ao salvar.
    };

    try 
    {
        db.Produtos.Add(novoProduto);
        await db.SaveChangesAsync();
        return Results.Created($"/produtos/{novoProduto.Id}", novoProduto);
    }
    catch (Exception ex) 
    {
        return Results.Problem("Erro ao salvar: " + ex.Message);
    }
});

app.MapGet("/produtos", async (AppDbContext db) => 
{
    try 
    {
        // Tenta executar a operação normal
        var lista = await db.Produtos
        .Include(p => p.Categoria)
        .Select(p => new ProdutoResponse
        {
            Id = p.Id,
            Nome = p.Nome,
            Preco = p.Preco,
            CategoriaNome = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
        })
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
    try 
    {
        // 1. Base da consulta
        var consulta = db.Produtos.AsQueryable();

        // 2. Filtros (Nome, CategoriaId, CategoriaNome)
        if (!string.IsNullOrEmpty(nome))
            consulta = consulta.Where(p => p.Nome.Contains(nome));

        if (categoriaId.HasValue)
            consulta = consulta.Where(p => p.CategoriaId == categoriaId);

        // Ao usar o ! após p.Categoria, estamos dizendo ao C# que "confia" que Categoria não será nula aqui, porque o Include já garantiu que ela será carregada junto com os produtos.
        if (!string.IsNullOrEmpty(categoriaNome))
            consulta = consulta.Where(p => p.Categoria!.Nome.Contains(categoriaNome));

        // 3. Contagem e Cálculos
        var totalItens = await consulta.CountAsync();
        
        if (tamanho <= 0) tamanho = 10;
        if (pagina <= 0) pagina = 1;

        int totalPaginas = (int)Math.Ceiling((double)totalItens / tamanho);
        int pular = (pagina - 1) * tamanho;

        // 4. Execução da busca e Soma
        var dados = await consulta
        .Skip(pular)
        .Take(tamanho)
        .Select(p => new ProdutoResponse 
            {
                Id = p.Id,
                Nome = p.Nome,
                Preco = p.Preco,
                CategoriaNome = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
            })
        .ToListAsync();
        
        var valorTotalEstoque = await consulta.SumAsync(p => (decimal?)p.Preco) ?? 0;

        return Results.Ok(new {
            TotalItensFiltrados = totalItens,
            ValorTotalDoEstoque = valorTotalEstoque,
            TotalDePaginas = totalPaginas,
            PaginaAtual = pagina,
            Produtos = dados
        });
    }
    catch (Exception ex)
    {
        // Log do erro no console do VS Code para debugar
        Console.WriteLine($"Erro na busca: {ex.Message}");
        
        // Retorna um erro amigável para quem chamou a API (Postman/Python)
        return Results.Problem("Ocorreu um erro interno ao processar a busca. Verifique os logs.");
    }
});

app.MapPut("/produtos/{id}", async (AppDbContext db, int id, ProdutoUpdateRequest dados) =>
{
    if (dados.Preco <= 0)
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

        produtoNoBanco.Preco = dados.Preco;
        produtoNoBanco.CategoriaId = dados.CategoriaId;

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
app.MapPost("/categorias", async (AppDbContext db, Categoria categoria) =>
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

app.MapGet("/dashboard/resumo", async (AppDbContext db) =>
{
    try
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
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no Dashboard: {ex.Message}");
        return Results.Problem("Erro ao gerar o dashboard. Verifique os logs do servidor.");
    }
});

app.Run();