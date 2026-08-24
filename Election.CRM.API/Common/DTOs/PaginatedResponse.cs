namespace Election.CRM.API.Common.DTOs;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    
    // Calcula el total de páginas automáticamente
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    
    // Indica si hay página siguiente o anterior
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}