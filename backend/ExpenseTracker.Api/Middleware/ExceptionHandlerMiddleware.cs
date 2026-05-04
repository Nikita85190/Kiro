using System.Text.Json;
using ExpenseTracker.Api.Exceptions;

namespace ExpenseTracker.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, "NOT_FOUND", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status409Conflict, "CONFLICT", ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteErrorResponse(context, StatusCodes.Status422UnprocessableEntity, "BUSINESS_RULE_VIOLATION", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new ErrorResponse(errorCode, message, null);
        var json = JsonSerializer.Serialize(body, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private record ErrorResponse(string Error, string Message, object? Details);
}
