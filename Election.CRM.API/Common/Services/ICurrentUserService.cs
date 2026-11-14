namespace Election.CRM.API.Common.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    string RoleName { get; }
    bool IsAdmin { get; }
    
    // Tu método existente, lo mantenemos por compatibilidad
    void ValidateIsAdmin(); 
}