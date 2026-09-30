using InventoryHub.Application.Contracts;
using InventoryHub.Contracts.Dtos;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Services;

public class CategoryService(ICategoryRepository repository) : ICategoryService
{
    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken ct)
    {
        var categories = await repository.GetAllAsync(ct);
        return categories.Select(c => new CategoryResponse(c.Id, c.Name)).ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await repository.GetByIdAsync(id, ct);
        return category is null ? null : new CategoryResponse(category.Id, category.Name);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken ct)
    {
        var created = await repository.CreateAsync(new Category { Name = request.Name.Trim() }, ct);
        return new CategoryResponse(created.Id, created.Name);
    }

    public async Task<bool> UpdateAsync(int id, CategoryRequest request, CancellationToken ct) =>
        await repository.UpdateAsync(id, new Category { Name = request.Name.Trim() }, ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await repository.DeleteAsync(id, ct);
}
