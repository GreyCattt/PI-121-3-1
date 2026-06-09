using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Exceptions;

namespace PL.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Сталася непередбачена помилка сервера.";

            switch (exception)
            {
                case EntityNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;

                case AuctionValidationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    if (exception.Message.Contains("вже існує", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        message = exception.Message;
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        message = exception.Message;
                    }
                    break;
            }

            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}