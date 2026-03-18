using ITO.Cloud.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace ITO.Cloud.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            NotFoundException e       => (HttpStatusCode.NotFound,         e.Message,                          (IDictionary<string, string[]>?)null),
            ForbiddenException e      => (HttpStatusCode.Forbidden,        e.Message,                          null),
            ConflictException e       => (HttpStatusCode.Conflict,         e.Message,                          null),
            Application.Common.Exceptions.ValidationException e
                                      => (HttpStatusCode.UnprocessableEntity, "Error de validación.",          e.Errors),
            UnauthorizedAccessException
                                      => (HttpStatusCode.Unauthorized,     "No autorizado.",                   null),
            _                         => (HttpStatusCode.InternalServerError, "Error interno del servidor.",   null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Error no controlado: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            errors  = errors != null
                ? errors.SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}")).ToList()
                : exception.Message != message ? new List<string>() : new List<string>()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
