using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Acceloka.Api.Infrastructure.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            catch (Exception exception)
            {
                var requestPath = context.Request.Path;
                var requestMethod = context.Request.Method;

                _logger.LogError(exception, 
                    "Unhandled exception occurred. Path: {RequestPath}, Method: {RequestMethod}", 
                    requestPath, 
                    requestMethod);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Instance = context.Request.Path,
                Title = "An error occurred while processing your request.",
            };

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Status = StatusCodes.Status400BadRequest;
                    response.Title = "One or more validation errors occurred.";
                    response.Detail = "One or more validation errors occurred.";
                    response.Extensions["errors"] = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());
                    break;

                case KeyNotFoundException keyNotFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    response.Status = StatusCodes.Status404NotFound;
                    response.Title = "Resource not found.";
                    response.Detail = keyNotFoundException.Message;
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Status = StatusCodes.Status401Unauthorized;
                    response.Title = "Unauthorized.";
                    response.Detail = unauthorizedException.Message;
                    break;

                case InvalidOperationException invalidOpException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Status = StatusCodes.Status400BadRequest;
                    response.Title = "Business rule violation.";
                    response.Detail = invalidOpException.Message;
                    break;

                case Microsoft.EntityFrameworkCore.DbUpdateException dbException:
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    response.Status = StatusCodes.Status409Conflict;
                    response.Title = "Database conflict occurred.";
                    response.Detail = "A conflict occurred while saving data. Please retry.";
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Status = StatusCodes.Status500InternalServerError;
                    response.Title = "Internal server error.";
                    response.Detail = "An unexpected error occurred. Please contact support.";
                    break;
            }

            var result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(result);
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
        {
            Errors = errors;
        }

        public IEnumerable<FluentValidation.Results.ValidationFailure> Errors { get; }
    }
}
