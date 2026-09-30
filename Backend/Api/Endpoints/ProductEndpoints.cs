using InventoryHub.Application.Contracts;
using InventoryHub.Contracts;
using InventoryHub.Contracts.Dtos;

namespace InventoryHub.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.Products);

        // GET /products?page=1&pageSize=20&search=lap -> ApiResponse<PagedResult<ProductResponse>>
        // CacheOutput 30s: la lista es lectura frecuente, escritura infrecuente.
        group.MapGet("", async (
                int? page,
                int? pageSize,
                string? search,
                IProductService service,
                HttpContext http,
                CancellationToken ct) =>
            {
                var result = await service.GetPagedAsync(
                    page ?? Limits.PageMin,
                    pageSize ?? Limits.PageSizeDefault,
                    search, ct);
                return Results.Ok(ApiResponse<PagedResult<ProductResponse>>.Ok(result, http.TraceIdentifier));
            })
            .CacheOutput(p => p.Expire(TimeSpan.FromSeconds(30)))
            .WithSummary("List products (paged, searchable)");

        group.MapGet("/{id:int}", async (int id, IProductService service, HttpContext http, CancellationToken ct) =>
            await service.GetByIdAsync(id, ct) is ProductResponse product
                ? Results.Ok(ApiResponse<ProductResponse>.Ok(product, http.TraceIdentifier))
                : Results.NotFound(ApiResponse<ProductResponse>.Fail($"Product {id} not found.", http.TraceIdentifier)));

        group.MapPost("", async (ProductRequest request, IProductService service, HttpContext http, CancellationToken ct) =>
        {
            var created = await service.CreateAsync(request, ct);
            return Results.Created(
                $"{ApiRoutes.Products}/{created.Id}",
                ApiResponse<ProductResponse>.Ok(created, http.TraceIdentifier));
        });

        group.MapPut("/{id:int}", async (int id, ProductRequest request, IProductService service, HttpContext http, CancellationToken ct) =>
            await service.UpdateAsync(id, request, ct)
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail($"Product {id} not found.", http.TraceIdentifier)));

        group.MapDelete("/{id:int}", async (int id, IProductService service, HttpContext http, CancellationToken ct) =>
            await service.DeleteAsync(id, ct)
                ? Results.NoContent()
                : Results.NotFound(ApiResponse<object>.Fail($"Product {id} not found.", http.TraceIdentifier)));

        return app;
    }
}
