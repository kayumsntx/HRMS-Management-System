using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Models;
using System.Net;
using System.Text.Json;

namespace HRMS.API.Extensions
{
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
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                NotFoundException => HttpStatusCode.NotFound,          // 404
                BadRequestException => HttpStatusCode.BadRequest,      // 400
                Application.Common.Exceptions.ValidationException => HttpStatusCode.BadRequest, // 400
                UnauthorizedException => HttpStatusCode.Unauthorized,  // 401
                _ => HttpStatusCode.InternalServerError                // 500
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<string>.FailResponse(exception.Message);

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
