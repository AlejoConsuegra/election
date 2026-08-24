using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Election.CRM.API.Data;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.Role;

namespace Election.CRM.API.Common.Extensions;

public static class SeedExtensions
{
    /// <summary>
    /// Ejecuta el seed inicial de la base de datos.
    /// Crea el SuperAdmin si no existe.
    /// 
    /// CREDENCIALES DEL CREADOR (SuperAdmin inicial):
    /// - Cédula: 1000000000
    /// - Contraseña: Admin2026!
    /// - Rol: Admin (ID 1)
    /// 
    /// Este usuario es el único con permisos para crear otros administradores
    /// y gestionar el sistema. Debe cambiarse la contraseña después del primer login.
    /// </summary>
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmContext>();

        // PASO 1: Crear roles si no existen (idempotente)
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new() { NameRol = "Admin" },
                new() { NameRol = "Lider" },
                new() { NameRol = "Coordinator" }
            };
            
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
            
            Console.WriteLine("✅ Roles creados exitosamente");
        }

        // PASO 2: Obtener el rol Admin (IdRol = 1)
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.NameRol == "Admin");
        
        if (adminRole == null)
        {
            Console.WriteLine("❌ Error: El rol 'Admin' no existe");
            return;
        }

        // Validar si ya existe el SuperAdmin por cédula
        var superAdminExists = await context.Users.AnyAsync(u => u.Cedula == "1000000000");

        // Solo crear si NO existe (silencioso si ya existe)
        if (!superAdminExists)
        {
            var superAdmin = new User
            {
                Nombre = "Super Admin",
                Email = "admin@election.com",
                Telefono = "0000000000",
                Municipio = "Sede Central",
                Cedula = "1000000000",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin2026!"),
                IdRol = 1
            };

            context.Users.Add(superAdmin);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ SuperAdmin creado exitosamente");
        }
        // Si ya existe, NO se hace nada (sin mensaje)
    }
}