using System.Net;
using System.Text.Json;

using Election.CRM.API.Common.Exceptions;

namespace Election.CRM.API.Common.Middleware;

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
        catch (ValidationException ex)
        {
            // LOG INTERNO: Registramos el error de validación de DTO
            _logger.LogWarning("Error de validación de datos: {Errors}", string.Join(", ", ex.Errors.SelectMany(e => e.Value)));
            
            // RESPUESTA SEGURA AL CLIENTE
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (InvalidOperationException ex)
        {
            // LOG INTERNO: Registramos la regla de negocio violada
            _logger.LogWarning("Regla de negocio violada: {Message}", ex.Message);
            
            // RESPUESTA SEGURA AL CLIENTE (Devuelve el mensaje de la regla, pero SIN stack trace)
            await HandleBusinessRuleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            // LOG INTERNO COMPLETO: Para cualquier otro error inesperado
            _logger.LogError(ex, "Excepción no controlada en la aplicación");
            
            // RESPUESTA GENÉRICA Y SEGURA (Nunca expone detalles internos)
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            statusCode = 400,
            message = "Datos inválidos. Verifique los campos e inténtelo de nuevo.",
            errors = ex.Errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleBusinessRuleExceptionAsync(HttpContext context, InvalidOperationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new
        {
            statusCode = 400,
            message = ex.Message
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = 500,
            message = "Ocurrió un error interno en el servidor. Por favor, inténtelo más tarde."
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}