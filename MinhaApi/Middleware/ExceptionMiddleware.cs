using System.Net;
using System.Text.Json;
using Azure.Core;

namespace MinhaApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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