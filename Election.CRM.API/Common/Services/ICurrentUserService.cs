namespace Election.CRM.API.Common.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    string RoleName { get; }
    string Nombre { get; } 
    string Email { get; }   
    bool IsAdmin { get; }
    
    void ValidateIsAdmin(); 
}