using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TSQR.ToolLibrary.WebApi.Controllers.Dtos;

namespace TSQR.ToolLibrary.WebApi.Middleware;

/// <summary>
/// Global exception handler. Catches unhandled exceptions from the pipeline,
/// logs them, and returns a structured <see cref="ErrorResponse"/> with HTTP 500.
/// In Development, re-throws so <c>UseDeveloperExceptionPage</c> renders details.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IHostEnvironment env,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _env = env;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!_env.IsDevelopment())
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            var body = JsonSerializer.Serialize(
                new ErrorResponse("InternalServerError", "An unexpected error occurred."),
                JsonOptions);
            await context.Response.WriteAsync(body);
        }
    }
}