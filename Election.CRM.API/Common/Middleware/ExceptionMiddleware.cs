using System.Net;
using System.Text.Json;
using Election.CRM.API.Common.Exceptions;
using Election.CRM.API.Features.Votante.DTOs;

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
        catch (CedulaNoRegistradaException ex)
        {
            _logger.LogWarning(ex, "Cédula no registrada en el sistema");
            await HandleCedulaNoRegistradaAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió una excepción no controlada");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Manejo especial para CedulaNoRegistradaException.
    /// Devuelve un JSON estructurado con sugerencia de registrar como ocasional.
    /// </summary>
    private static Task HandleCedulaNoRegistradaAsync(HttpContext context, CedulaNoRegistradaException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var response = new CedulaNoRegistradaResponse
        {
            Codigo = "CEDULA_NO_REGISTRADA",
            Mensaje = ex.Message,
            Sugerencia = new SugerenciaRegistro
            {
                Accion = "REGISTRAR_COMO_OCASIONAL",
                Mensaje = "Puede registrar a esta persona como votante ocasional",
                Endpoint = "/api/Ocasionale",
                DatosPrellenados = new Dictionary<string, object>
                {
                    { "cedula", ex.Cedula }
                }
            }
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }

    /// <summary>
    /// Manejo genérico de excepciones (patrón uniforme existente).
    /// </summary>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error interno en el servidor")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}