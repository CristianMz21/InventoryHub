using System.Text.Json;

using InventoryHub.Api.Endpoints;
using InventoryHub.Api.Exceptions;
using InventoryHub.Application.Contracts;
using InventoryHub.Application.Services;
using InventoryHub.Infrastructure.Data;
using InventoryHub.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// JSON consistente: camelCase en toda la API (Actividad 3).
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

// Performance: cache de respuestas GET (Actividad 4).
builder.Services.AddOutputCache();
builder.Services.AddHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<InvalidRequestExceptionHandler>();
builder.Services.AddValidation();

// CORS abierto solo para demo/revisión (front separado en el futuro).
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Proyecto escolar sin migraciones EF: EnsureCreated crea el esquema + seed
    // de AppDbContext.OnModelCreating al primer arranque.
    // En producción usarías: dotnet ef migrations add InitialCreate + Database.Migrate().
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseCors();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks(InventoryHub.Contracts.ApiRoutes.Health);
app.MapCategoryEndpoints();
app.MapProductEndpoints();

app.Run();
