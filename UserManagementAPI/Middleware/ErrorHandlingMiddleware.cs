using System.Net;
using System.Text.Json;

namespace UserManagementAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new ErrorResponse();

            // Set status code and message based on exception type
            switch (ex)
            {
                case KeyNotFoundException:
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.Message = "The requested resource was not found.";
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access.";
                    break;

                case ArgumentException:
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Message = "Invalid request data.";
                    break;

                default:
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Message = "An internal server error occurred.";
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.StatusCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private class ErrorResponse
        {
            public string Message { get; set; } = string.Empty;
            public HttpStatusCode StatusCode { get; set; }
        }
    }
}