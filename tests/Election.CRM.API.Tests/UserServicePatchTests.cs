using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using Election.CRM.API.Data;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.User.DTOs;
using Election.CRM.API.Features.User.Validators;
using Election.CRM.API.Features.Role;
using Election.CRM.API.Common.Services; // <-- Agregado para ICurrentUserService

namespace Election.CRM.API.Tests;

public class UserServicePatchTests
{
    private CrmContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<CrmContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new CrmContext(options);

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
            Municipio = "Ciudad Prueba",
            Telefono = "555-0199",
            IdRol = 2 
        });

        context.SaveChanges();

        return context;
    }

    /// <summary>
    /// HELPER: Crea un mock de ICurrentUserService con los valores deseados.
    /// Esto reemplaza la necesidad de mockear IHttpContextAccessor y Claims.
    /// </summary>
    private Mock<ICurrentUserService> CreateMockCurrentUser(int userId, string roleName, bool isAdmin)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(x => x.UserId).Returns(userId);
        mock.Setup(x => x.RoleName).Returns(roleName);
        mock.Setup(x => x.IsAdmin).Returns(isAdmin);
        
        // Si el test llama a ValidateIsAdmin() y no es admin, lanzamos la excepción
        if (!isAdmin)
        {
            mock.Setup(x => x.ValidateIsAdmin())
                .Throws(new UnauthorizedAccessException("Solo los administradores pueden realizar esta operación"));
        }
        
        return mock;
    }

    [Fact]
    public async Task PatchUserAsync_WhenNonAdminTriesToChangeRole_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        // Simulamos un usuario "Lider" (NO admin) con ID 10
        var mockCurrentUser = CreateMockCurrentUser(userId: 10, roleName: "Lider", isAdmin: false);

        // Mock del validador (ya no necesitamos configurar ValidateIsAdminAsync aquí,
        // porque la lógica de "no es admin" la maneja el mock de ICurrentUserService)
        var mockValidator = new Mock<UserValidator>(context, mockCurrentUser.Object);

        var userService = new UserService(
            context,
            mockValidator.Object,
            mockCurrentUser.Object, // <-- Tercer parámetro: ICurrentUserService (NO null!)
            null!, 
            null!, 
            null!  
        );

        var request = new PatchUserRequest
        {
            IdRol = 1 // Intenta escalar a Administrador
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => userService.PatchUserAsync(10, request)
        );
        
        Assert.Equal("Solo un Administrador puede modificar el rol de un usuario.", exception.Message);
    }

    [Fact]
    public async Task PatchUserAsync_WhenAdminChangesRole_ShouldSucceed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        // Simulamos un usuario "Admin" con ID 10
        var mockCurrentUser = CreateMockCurrentUser(userId: 10, roleName: "Admin", isAdmin: true);

        // Mock del validador
        var mockValidator = new Mock<UserValidator>(context, mockCurrentUser.Object);

        var userService = new UserService(
            context,
            mockValidator.Object,
            mockCurrentUser.Object, // <-- Tercer parámetro: ICurrentUserService (NO null!)
            null!,
            null!,
            null!
        );

        var request = new PatchUserRequest
        {
            IdRol = 3 // Cambia legítimamente a Coordinator
        };

        // Act
        await userService.PatchUserAsync(10, request);

        // Assert
        var updatedUser = await context.Users.FindAsync(10);
        Assert.NotNull(updatedUser);
        Assert.Equal(3, updatedUser.IdRol);
    }
}