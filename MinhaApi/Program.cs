using Microsoft.EntityFrameworkCore;
using MinhaApi.Data;
using MinhaApi.Converters;
using FluentValidation;
using MinhaApi.Validators;
using MinhaApi.Endpoints;
using System.Text;
using Scalar.AspNetCore; // Importante: dotnet add package Scalar.AspNetCore

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAÇÃO JWT
var jwtKey = builder.Configuration["JwtSettings:Key"] 
            ?? throw new InvalidOperationException("A chave JWT não foi configurada!");
var key = Encoding.ASCII.GetBytes(jwtKey);

// 2. NOVO OPENAPI (.NET 10) - Substitui o AddSwaggerGen
builder.Services.AddOpenApi(options =>
{
    // Adicionamos o transformador que criamos para o "Cadeado"
    options.AddDocumentTransformer<SecurityTransformer>();
});

// 3. REGISTROS DO FLUENTVALIDATION
builder.Services.AddValidatorsFromAssemblyContaining<ProdutoCreateValidator>();

// 4. CONFIGURAÇÕES JSON
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => 
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new DecimalConverter());
});

// 5. BANCO DE DADOS E SERVIÇOS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<MinhaApi.Services.TokenService>();


// 1. Configura como a API valida o Token (Quem é você?)
builder.Services.AddAuthentication(options => 
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options => 
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// 2. Registra o serviço de regras de acesso (Você pode entrar?)
builder.Services.AddAuthorization();

// --- FIM DO BUILDER ---
var app = builder.Build();

// 6. MIDDLEWARES
app.UseMiddleware<MinhaApi.Middleware.ExceptionMiddleware>();

// 7. INTERFACE DA API (Novo Padrão .NET 10)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Gera o JSON do contrato
    app.MapScalarApiReference(); // Interface moderna em /scalar/v1
}

// 8. SEGURANÇA E ROTAS
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProdutoEndpoints();
app.MapCategoriaEndpoints();
app.MapDashboardEndpoints();

app.Run();