using System.Net;
using System.Text.Json;

namespace UrlShortener.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext context, Exception exception)
    {
        var response = context.Response;

        response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = new
            {
                message = exception.Message,
                type = exception.GetType().Name,
                status = response.StatusCode
            }
        });

        return response.WriteAsync(result);
    }
}