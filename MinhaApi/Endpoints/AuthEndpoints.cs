using MinhaApi.Services;

namespace MinhaApi.Endpoints;

// 1. Defina o modelo (DTO) aqui fora. 
// O ASP.NET Core verá esse objeto e entenderá: "Isso vem do BODY do JSON!"
public record LoginRequest(string Username, string Password);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes
            .MapGroup("/auth")
            .WithTags("Autenticação");

        // 2. Troque (string username, string password) pelo objeto LoginRequest
        group.MapPost("/login", (LoginRequest login, TokenService tokenService) =>
        {
            // 3. Agora você acessa os dados através do objeto 'login'
            if (login.Username != "admin" || login.Password != "123456")
            {
                return Results.Unauthorized();
            }

            var token = tokenService.GerarToken(login.Username);

            return Results.Ok(new
            {
                mensagem = "Login bem-sucedido!",
                token = token
            });
        });
    }
}