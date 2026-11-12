using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using Election.CRM.API.Data;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.User.DTOs;
using Election.CRM.API.Features.Role;

namespace Election.CRM.API.Tests;

public class UserServicePatchTests
{
    private CrmContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CrmContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CrmContext(options);

        // Datos sintéticos: crear roles y un usuario de prueba
        context.Roles.Add(new Role { Id = 1, NameRol = "Admin" });
        context.Roles.Add(new Role { Id = 2, NameRol = "Lider" });
        context.Roles.Add(new Role { Id = 3, NameRol = "Coordinator" });

        context.Users.Add(new User
        {
            Id = 10,
            Nombre = "Usuario Prueba",
            Email = "prueba@test.com",
            Cedula = "123456789",
            Password = "hashed_password",
            Municipio = "Ciudad Prueba", // Campo obligatorio agregado
            Telefono = "555-0199",       // Campo obligatorio agregado
            IdRol = 2 // Rol Líder inicialmente
        });

        context.SaveChanges();

        return context;
    }

    private Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string roleName)
    {
        var claims = new List<Claim>
        {
            // Usamos la constante oficial de .NET para garantizar la coincidencia
            new Claim(System.Security.Claims.ClaimTypes.Role, roleName),
            new Claim(System.Security.Claims.ClaimTypes.Name, "TestUser")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

        return mockAccessor;
    }

    [Fact]
    public async Task PatchUserAsync_WhenNonAdminTriesToChangeRole_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockAccessor = CreateMockHttpContextAccessor("Lider"); 

        var userService = new UserService(
            context,
            null!, 
            mockAccessor.Object,
            null!, 
            null!, 
            null!  
        );

        var request = new PatchUserRequest
        {
            IdRol = 1 // Intenta escalar a Administrador
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => userService.PatchUserAsync(10, request)
        );
    }

    [Fact]
    public async Task PatchUserAsync_WhenAdminChangesRole_ShouldSucceed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var mockAccessor = CreateMockHttpContextAccessor("Admin");

        var userService = new UserService(
            context,
            null!,
            mockAccessor.Object,
            null!,
            null!,
            null!
        );

        var request = new PatchUserRequest
        {
            IdRol = 3 // Cambia a Coordinador
        };

        // Act
        await userService.PatchUserAsync(10, request);

        // Assert: verificar que el cambio se aplicó correctamente
        var updatedUser = await context.Users.FindAsync(10);
        Assert.NotNull(updatedUser);
        Assert.Equal(3, updatedUser.IdRol);
    }
}