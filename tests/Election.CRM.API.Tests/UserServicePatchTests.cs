using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

using Election.CRM.API.Data;
using Election.CRM.API.Features.User;
using Election.CRM.API.Features.User.DTOs;
using Election.CRM.API.Features.User.Validators; // <-- Agregado para reconocer UserValidator
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

        context.Roles.Add(new Role { Id = 1, NameRol = "Admin" });
        context.Roles.Add(new Role { Id = 2, NameRol = "Lider" });
        context.Roles.Add(new Role { Id = 3, NameRol = "Coordinador" });

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

    [Fact]
    public async Task PatchUserAsync_WhenNonAdminTriesToChangeRole_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var context = CreateInMemoryContext();
        
        // Creamos un mock del UserValidator que lance la excepción cuando se llame a ValidateIsAdminAsync
        var mockValidator = new Mock<UserValidator>(context, null!);
        mockValidator.Setup(v => v.ValidateIsAdminAsync())
                     .ThrowsAsync(new UnauthorizedAccessException("No tiene permisos para modificar el rol del usuario"));

        var userService = new UserService(
            context,
            mockValidator.Object, // Inyectamos el mock del validador (ya no es null!)
            null!, 
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
        
        // Creamos un mock del UserValidator que NO lance excepción (simula que la validación de Admin fue exitosa)
        var mockValidator = new Mock<UserValidator>(context, null!);
        mockValidator.Setup(v => v.ValidateIsAdminAsync())
                     .Returns(Task.CompletedTask);

        var userService = new UserService(
            context,
            mockValidator.Object,
            null!,
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

        // Assert
        var updatedUser = await context.Users.FindAsync(10);
        Assert.NotNull(updatedUser);
        Assert.Equal(3, updatedUser.IdRol);
    }
}