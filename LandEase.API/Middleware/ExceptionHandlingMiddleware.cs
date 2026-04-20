using FluentValidation;
using LandEase.API.Models;
using LandEase.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace LandEase.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message, errorCode, errors) = ex switch
        {
            AppException appEx => (
                appEx.StatusCode,
                appEx.Message,
                appEx.ErrorCode,
                appEx.Errors?.ToList()),

            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                "validation_error",
                validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList()),

            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "A database conflict occurred.",
                "db_conflict",
                (List<string>?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                _env.IsDevelopment() || _env.IsEnvironment("Testing")
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.",
                "internal_error",
                (List<string>?)null)
        };

        if (statusCode >= 500)
        {
            _logger.LogError(ex, "Unhandled exception (traceId: {TraceId})", traceId);
        }
        else
        {
            _logger.LogInformation(ex, "Request failed (traceId: {TraceId})", traceId);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(
            message,
            errors: errors,
            errorCode: errorCode,
            traceId: traceId);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

