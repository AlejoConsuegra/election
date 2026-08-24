using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;
using Election.CRM.API.Common.Extensions;
using Election.CRM.API.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.AddKestrelConfiguration();

builder.Services.AddDbContext<CrmContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        /* ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")) */
         ServerVersion.Parse("8.0.46")
    ));

builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration();
builder.Services.AddApplicationServices();
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Election.CRM.API.Data.CrmContext>();
    
    // Aplica las migraciones pendientes a la base de datos al iniciar
    // Esto es lo correcto para entornos de producción y monografías
    dbContext.Database.Migrate();
}


await app.SeedDatabaseAsync();
app.UseApplicationMiddleware();
app.UseMiddleware<ExceptionMiddleware>();
app.UseServerHeaderRemoval();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();