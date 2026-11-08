using System.Text.Json;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Middleware;

/// <summary>
/// Global exception handler. Maps optimistic-concurrency conflicts to HTTP 409,
/// and catches other unhandled exceptions, logs them, and returns a structured
/// <see cref="ErrorResponse"/> with HTTP 500. In Development, non-concurrency
/// exceptions are re-thrown so <c>UseDeveloperExceptionPage</c> renders details.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    IHostEnvironment env,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ConcurrencyConflictException ex)
        {
            logger.LogWarning(
                ex,
                "Optimistic concurrency conflict on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json; charset=utf-8";
            var body = JsonSerializer.Serialize(
                new ErrorResponse("ConcurrencyConflict", ex.Message),
                JsonOptions);
            await context.Response.WriteAsync(body);
        }
        catch (Exception ex) when (!env.IsDevelopment())
        {
            logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            var body = JsonSerializer.Serialize(
                new ErrorResponse("InternalServerError", "An unexpected error occurred."),
                JsonOptions);
            await context.Response.WriteAsync(body);
        }
    }
}
