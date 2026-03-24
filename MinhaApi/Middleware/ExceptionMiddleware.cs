using System.Net;
using System.Text.Json;

namespace MinhaApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next; // para seguir o fluxo normal da requisição, para não atrapalhar o funcionamento da API, para que a API continue funcionando normalmente, mesmo que dê erro em algum lugar
        _logger = logger; //para me mandar um log do erro que aconteceu, para saber o que aconteceu e onde aconteceu, para poder corrigir depois
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //tenta seguir o fluxo normal da requisição
            await _next(context);
        }
        catch (Exception ex)
        {
            //se der erro em QLQ lugar da API:
            _logger.LogError($"Algo deu errado: {ex.Message}");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Erro interno no servidor. Tente novamente mais tarde.",
            Detailed = exception.Message
        };


        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}