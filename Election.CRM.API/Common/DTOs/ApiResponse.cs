namespace Election.CRM.API.Common.DTOs;

public class ApiResponse<T>
{
    public string Status { get; set; } = "success";
    public string StatusName { get; set; } = "OK";
    public int HttpCode { get; set; } = 200;
    public string Message { get; set; } = "Operación exitosa";
    public T? Datos { get; set; }
    public object? Extra { get; set; }

    // Reúsa la lógica que ya tenías en ControllerExtensions.Error
    public static ApiResponse<object> BuildError(string message, int statusCode, object? extra = null)
    {
        return new ApiResponse<object>
        {
            Status = "error",
            StatusName = GetStatusName(statusCode),
            HttpCode = statusCode,
            Message = message,
            Datos = null,
            Extra = extra
        };
    }

    // Reúsa la lógica que ya tenías en ControllerExtensions.Success
    public static ApiResponse<T> BuildSuccess(T? data, string message, int statusCode)
    {
        return new ApiResponse<T>
        {
            Status = "success",
            StatusName = GetStatusName(statusCode),
            HttpCode = statusCode,
            Message = message,
            Datos = data
        };
    }

    private static string GetStatusName(int code) => code switch
    {
        200 => "OK",
        201 => "Created",
        204 => "No Content",
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Content",
        429 => "Too Many Requests",
        500 => "Internal Server Error",
        _ => "Unknown Status"
    };
}