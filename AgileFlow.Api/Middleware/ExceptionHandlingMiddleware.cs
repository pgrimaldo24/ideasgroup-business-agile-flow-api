using System.Net;
using System.Text.Json;
using AgileFlow.Application.Common;
using AgileFlow.Domain.Common;

namespace AgileFlow.Api.Middleware;

/// <summary>
/// Traduce excepciones de Domain/Application a respuestas HTTP consistentes,
/// evitando try/catch repetido en cada controller y evitando filtrar stack
/// traces o detalles internos al cliente.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                UnauthorizedAppException => (HttpStatusCode.Unauthorized, ex.Message),
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                DomainException => (HttpStatusCode.Conflict, ex.Message),
                AppException => (HttpStatusCode.BadRequest, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
        }
    }
}
