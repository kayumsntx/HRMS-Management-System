using System.Net;
using System.Text.Json;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Middlewares;

public class ExceptionMiddleware
{
    // camelCase, same as the controllers' JSON ("success", "message", "data")
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // the client closed the connection: nothing to answer, nothing to log as an error
            context.Response.StatusCode = 499;
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, message, errors) = Map(ex, context.TraceIdentifier);

            if (status >= 500)
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning("Request failed with {Status}: {Message}", status, message);

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";

            var body = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
        }
    }

    // Only messages of known, intentional exceptions reach the client. Everything else becomes a generic 500.
    private static (int Status, string Message, object? Errors) Map(Exception ex, string traceId)
    {
        switch (ex)
        {
            case NotFoundException:
                return ((int)HttpStatusCode.NotFound, ex.Message, null);

            case BadRequestException:
                return ((int)HttpStatusCode.BadRequest, ex.Message, null);

            case HRMS.Application.Common.Exceptions.ValidationException validation:
                return ((int)HttpStatusCode.BadRequest, validation.Message, validation.Errors);

            case UnauthorizedException:
                return ((int)HttpStatusCode.Unauthorized, ex.Message, null);

            case BadHttpRequestException bad:
                return (bad.StatusCode, bad.Message, null);   // e.g. 413 request body too large

            case DbUpdateConcurrencyException:
                return ((int)HttpStatusCode.Conflict, "The record was changed by someone else. Please reload and try again.", null);

            case DbUpdateException { InnerException: Microsoft.Data.SqlClient.SqlException sql } when sql.Number is 2601 or 2627:
                return ((int)HttpStatusCode.Conflict, "A record with the same unique value already exists.", null);

            case DbUpdateException { InnerException: Microsoft.Data.SqlClient.SqlException sql } when sql.Number == 547:
                return ((int)HttpStatusCode.Conflict, "This record is linked to other data and cannot be changed or deleted.", null);

            default:
                return ((int)HttpStatusCode.InternalServerError, $"An unexpected error occurred. Reference: {traceId}", null);
        }
    }
}
