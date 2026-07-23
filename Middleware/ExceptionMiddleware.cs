using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using EMS.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace EMS.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Global Exception Handler caught: {Message}", ex.Message);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var message = exception.Message;
            object? details = null;

            // Map standard C# exceptions to appropriate status codes
            if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception is InvalidOperationException || exception is ArgumentException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                details = exception.StackTrace;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                Status = "Error",
                Message = message,
                Details = details
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
