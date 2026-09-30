using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Contracts;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<Category> CreateAsync(Category category, CancellationToken ct);
    Task<bool> UpdateAsync(int id, Category category, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
