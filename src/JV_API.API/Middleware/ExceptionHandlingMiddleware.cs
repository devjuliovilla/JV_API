using System.Net;
using System.Text.Json;
using JV_API.Shared.DTOs;
using JV_API.Shared.Exceptions;

namespace JV_API.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            Shared.Exceptions.ValidationException validationEx => new ApiErrorResponseDto
            {
                Title = "Validation Error",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = validationEx.Message,
                Errors = validationEx.Errors
            },
            NotFoundException => new ApiErrorResponseDto
            {
                Title = "Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = exception.Message
            },
            UnauthorizedException => new ApiErrorResponseDto
            {
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = exception.Message
            },
            ForbiddenException => new ApiErrorResponseDto
            {
                Title = "Forbidden",
                Status = (int)HttpStatusCode.Forbidden,
                Detail = exception.Message
            },
            _ => new ApiErrorResponseDto
            {
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = "An unexpected error occurred."
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
