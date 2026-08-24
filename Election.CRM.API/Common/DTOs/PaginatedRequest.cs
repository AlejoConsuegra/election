namespace Election.CRM.API.Common.DTOs;

public class PaginatedRequest
{
    /// <summary>
    /// Número de página a consultar (inicia en 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Cantidad de registros por página
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Término de búsqueda general (opcional)
    /// </summary>
    public string? SearchTerm { get; set; }
}