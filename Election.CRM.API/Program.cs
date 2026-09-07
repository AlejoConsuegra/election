using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Election.CRM.API.Data;
using Election.CRM.API.Common.Extensions;
using Election.CRM.API.Common.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.AddKestrelConfiguration();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CrmContext>(options =>
    options.UseMySql(
        connectionString,
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
builder.Services.ConfigureApiBehavior();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Election.CRM.API.Data.CrmContext>();
    
    dbContext.Database.Migrate();
}


await app.SeedDatabaseAsync();
app.UseMiddleware<ExceptionMiddleware>();
app.UseApplicationMiddleware();
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