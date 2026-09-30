using InventoryHub.Contracts.Dtos;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Contracts;

public interface IProductRepository
{
    Task<(int TotalCount, List<Product> Items)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct);
    Task<List<Product>> GetAllAsync(CancellationToken ct);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<Product> CreateAsync(Product product, CancellationToken ct);
    Task<bool> UpdateAsync(int id, Product product, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
