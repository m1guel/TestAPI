using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TestAPI.Application.DTOs;
using TestAPI.Domain.Exceptions;

namespace TestAPI.Application.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
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
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                ErrorCodeFaultException faultEx => CreateErrorResponse(
                    (int)faultEx.ErrorCode,
                    faultEx.Message),

                UnauthorizedException unauthorizedEx => CreateErrorResponse(
                    (int)HttpStatusCode.Unauthorized,
                    unauthorizedEx.Message),

                _ => CreateErrorResponse(
                    (int)HttpStatusCode.InternalServerError,
                    exception.Message)
            };

            context.Response.StatusCode = (int)errorResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }

        private static ErrorResponseDto CreateErrorResponse(
            int statusCode,
            string message)
        {
            return new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}