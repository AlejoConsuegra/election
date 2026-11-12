using Microsoft.AspNetCore.Mvc;
using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Common.Extensions;

public static class ControllerExtensions
{
    public static IActionResult Success<T>(this ControllerBase controller, T? data, string message = "Operación exitosa", int statusCode = 200)
    {
        var response = ApiResponse<T>.BuildSuccess(data, message, statusCode);
        return controller.StatusCode(statusCode, response);
    }

    public static IActionResult Error(this ControllerBase controller, string message, int statusCode = 400, object? extra = null)
    {
        var response = ApiResponse<object>.BuildError(message, statusCode, extra);
        return controller.StatusCode(statusCode, response);
    }
}