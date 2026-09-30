using InventoryHub.Application.Contracts;
using InventoryHub.Contracts;
using InventoryHub.Contracts.Dtos;

namespace InventoryHub.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.Categories);

        group.MapGet("", async (ICategoryService service, HttpContext http, CancellationToken ct) =>
        {
            var items = await service.GetAllAsync(ct);
            return Results.Ok(ApiResponse<List<CategoryResponse>>.Ok(items, http.TraceIdentifier));
        }).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(60)));

        group.MapGet("/{id:int}", async (int id, ICategoryService service, HttpContext http, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is CategoryResponse category
                ? Results.Ok(ApiResponse<CategoryResponse>.Ok(category, http.TraceIdentifier))
                : Results.NotFound(ApiResponse<CategoryResponse>.Fail($"Category {id} not found.", http.TraceIdentifier)));

        group.MapPost("", async (CategoryRequest request, ICategoryService service, HttpContext http, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created(
                $"{ApiRoutes.Categories}/{created.Id}",
                ApiResponse<CategoryResponse>.Ok(created, http.TraceIdentifier));
        });

        group.MapPut("/{id:int}", async (int id, CategoryRequest request, ICategoryService service, HttpContext http, CancellationToken ct) =>
            await service.UpdateAsync(id, request, ct)
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail($"Category {id} not found.", http.TraceIdentifier)));

        group.MapDelete("/{id:int}", async (int id, ICategoryService service, HttpContext http, CancellationToken ct) =>
            await service.DeleteAsync(id, ct)
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail($"Category {id} not found.", http.TraceIdentifier)));

        return app;
    }
}
