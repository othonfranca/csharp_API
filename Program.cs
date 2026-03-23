var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var produtos = new List<PlatformNotSupportedException>();

app.MapGet("/produtos", () => produtos);

app.AddPost("/produtos", (Produto novo) => {
    produtos.Add(novo);
    return Results.Created($"/produtos/{novo.Id}", novo);
});

app.Run();
public record Produto(int Id, string Nome, decimal Preco);