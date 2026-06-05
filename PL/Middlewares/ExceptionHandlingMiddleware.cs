using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Exceptions; // Підключаємо наші кастомні виключення

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
                // Пропускаємо запит далі по конвеєру до контролерів
                await _next(context);
            }
            catch (Exception ex)
            {
                // Якщо десь у BLL чи DAL виникла помилка, ми її "ловимо"
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Вказуємо, що відповідь буде у форматі JSON
            context.Response.ContentType = "application/json";

            // За замовчуванням: 500 (Внутрішня помилка сервера)
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Сталася непередбачена помилка сервера.";

            // Розподіляємо статуси залежно від типу нашої помилки
            switch (exception)
            {
                case EntityNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; // 404
                    message = exception.Message;
                    break;

                case AuctionValidationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest; // 400
                    message = exception.Message;
                    break;

                case UnauthorizedAccessException:
                    if (exception.Message.Contains("вже існує", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict; // 409
                        message = exception.Message;
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized; // 401
                        message = exception.Message;
                    }
                    break;
            }

            // Формуємо красивий JSON
            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}