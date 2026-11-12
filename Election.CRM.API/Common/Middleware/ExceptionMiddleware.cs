using System.Net;
using System.Text.Json;
using Election.CRM.API.Common.DTOs;
using Election.CRM.API.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

        var response = new ApiResponse<object>
        {
            Status = "error",
            StatusName = GetStatusName(400),
            HttpCode = 400,
            Message = "Datos inválidos. Verifique los campos e inténtelo de nuevo.",
            Datos = null,
            Extra = ex.Errors // Aquí van los detalles de validación de FluentValidation
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    private static Task HandleBusinessRuleExceptionAsync(HttpContext context, InvalidOperationException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new ApiResponse<object>
        {
            Status = "error",
            StatusName = GetStatusName(400),
            HttpCode = 400,
            Message = ex.Message,
            Datos = null,
            Extra = null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new ApiResponse<object>
        {
            Status = "error",
            StatusName = GetStatusName(500),
            HttpCode = 500,
            Message = "Ocurrió un error interno en el servidor. Por favor, inténtelo más tarde.",
            Datos = null,
            Extra = null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    private static string GetStatusName(int code) => code switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Content",
        500 => "Internal Server Error",
        _ => "Error"
    };
}