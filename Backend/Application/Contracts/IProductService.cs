using InventoryHub.Contracts.Dtos;

namespace InventoryHub.Application.Contracts;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<List<ProductResponse>> GetAllAsync(CancellationToken ct);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, ProductRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
