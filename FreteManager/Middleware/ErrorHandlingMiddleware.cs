using FreteManager.Exceptions;
using FreteManager.Models;
using System.Net;
using System.Text.Json;

namespace FreteManager.Middleware
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpStatusCode statusCode;
            ApiError response = new ApiError
            {
                TraceId = context.TraceIdentifier,
                Instance = context.Request.Path
            };

            // Determinar o tipo de exceção e configurar a resposta adequadamente
            switch (exception)
            {
                case EntityNotFoundException ex:
                    statusCode = HttpStatusCode.NotFound;
                    response.Title = "Entidade não encontrada";
                    response.Detail = ex.Message;
                    response.Code = ex.ErrorCode;
                    _logger.LogWarning(ex, ex.Message);
                    break;

                case BusinessRuleViolationException ex:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Violação de regra de negócio";
                    response.Detail = ex.Message;
                    response.Code = ex.ErrorCode;
                    _logger.LogWarning(ex, ex.Message);
                    break;

                case UnauthorizedOperationException ex:
                    statusCode = HttpStatusCode.Forbidden;
                    response.Title = "Operação não autorizada";
                    response.Detail = ex.Message;
                    response.Code = ex.ErrorCode;
                    _logger.LogWarning(ex, ex.Message);
                    break;

                case DataIntegrityException ex:
                    statusCode = HttpStatusCode.BadRequest;
                    response.Title = "Erro de integridade de dados";
                    response.Detail = ex.Message;
                    response.Code = ex.ErrorCode;
                    _logger.LogError(ex, ex.Message);
                    break;

                case System.Data.SqlTypes.SqlNullValueException ex:
                    statusCode = HttpStatusCode.InternalServerError;
                    response.Title = "Erro de valor nulo no banco de dados";
                    response.Detail = "Um valor que deveria existir no banco de dados é nulo. Contate o suporte.";
                    response.Code = "DATA_NULL_ERROR";
                    _logger.LogError(ex, "Erro de valor nulo ao acessar o banco de dados: {Message}", ex.Message);
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    response.Title = "Erro interno do servidor";
                    response.Detail = "Ocorreu um erro inesperado. Por favor, tente novamente ou contate o suporte.";
                    response.Code = "INTERNAL_SERVER_ERROR";
                    _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
                    break;
            }

            response.Status = (int)statusCode;

            if (statusCode == HttpStatusCode.InternalServerError &&
                context.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) is Microsoft.AspNetCore.Hosting.IWebHostEnvironment env &&
                env.EnvironmentName != "Development")
            {
                response.Detail = "Ocorreu um erro interno. Por favor, contate o suporte.";
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            );
        }
    }
}