using Microsoft.EntityFrameworkCore;
using MinhaApi.Data; // Importa o AppDbContext do arquivo separado
using MinhaApi.Converters;
using FluentValidation;
using MinhaApi.Validators;
using MinhaApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURANDO SWAGGER PARA DOCUMENTAÇÃO AUTOMÁTICA E TESTE NO BROWSER SEM POSTMAN
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// REGISTRO DOS VALIDADORES DO FLUENTVALIDATION (PRODUTO CREATE, PRODUTO UPDATE, CATEGORIA UPDATE)
builder.Services.AddValidatorsFromAssemblyContaining<ProdutoCreateValidator>();

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

// Ativa o MIDDLEWARE para tratamente global de erros (ExceptionMiddleware)
app.UseMiddleware<MinhaApi.Middleware.ExceptionMiddleware>();

// Ativa o Swagger para uso de documentação e testes via interface web
app.UseSwagger();
app.UseSwaggerUI();

// REGISTRO DOS ENDPOINTS
app.MapProdutoEndpoints();
app.MapCategoriaEndpoints();
app.MapDashboardEndpoints();

app.Run();