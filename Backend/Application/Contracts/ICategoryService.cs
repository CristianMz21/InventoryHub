using InventoryHub.Contracts.Dtos;

namespace InventoryHub.Application.Contracts;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct);
    Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct);
    Task<bool> UpdateAsync(int id, CategoryRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
