using Election.CRM.API.Data;
using Election.CRM.API.Features.Role.DTOs;
using Election.CRM.API.Features.Role.Validators;
using Microsoft.EntityFrameworkCore;

namespace Election.CRM.API.Features.Role;

public class RoleService
{
    private readonly CrmContext _context;
    private readonly RoleValidator _validator;

    public RoleService(CrmContext context, RoleValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        return await _context.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                NameRol = r.NameRol
            })
            .ToListAsync();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles
            .Where(r => r.Id == id)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                NameRol = r.NameRol
            })
            .FirstOrDefaultAsync();

        if(role == null)
            throw new KeyNotFoundException($"El role con ID {id} no existe");

        return role;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {        
        await _validator.ValidateCreateAsync(request.NameRol);

        var role = new Role
        {
            NameRol = request.NameRol
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return (await GetRoleByIdAsync(role.Id))!;
    }

    public async Task UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        await _validator.ValidateUpdateAsync(id, request.NameRol);

        var role = await _context.Roles.FindAsync(id);
        if (role == null) return;

        role.NameRol = request.NameRol;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoleAsync(int id)
    {
        await _validator.ValidateDeleteAsync(id);
        
        var role = await _context.Roles.FindAsync(id);
        if (role == null) return;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }
}