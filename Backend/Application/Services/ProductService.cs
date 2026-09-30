using InventoryHub.Application.Contracts;
using InventoryHub.Application.Exceptions;
using InventoryHub.Contracts;
using InventoryHub.Contracts.Dtos;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Services;

public class ProductService(IProductRepository repository, ICategoryRepository categoryRepository) : IProductService
{
    public async Task<PagedResult<ProductResponse>> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        // Clamp defensivo: evita Skip negativo y full-table scan por pageSize gigante.
        page = Math.Max(page, Limits.PageMin);
        pageSize = Math.Clamp(pageSize, Limits.PageSizeMin, Limits.PageSizeMax);
        search = search?.Trim();
        if (search?.Length > Limits.SearchMaxLength)
            search = search[..Limits.SearchMaxLength];

        var (totalCount, items) = await repository.GetPagedAsync(page, pageSize, search, ct);

        var dtos = items.Select(MapToResponse).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResult<ProductResponse>(dtos, page, pageSize, totalCount, totalPages);
    }

    public async Task<List<ProductResponse>> GetAllAsync(CancellationToken ct)
    {
        var products = await repository.GetAllAsync(ct);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(id, ct);
        return product is null ? null : MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken ct)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, ct);

        var product = new Product
        {
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            Quantity = request.Quantity
        };

        var created = await repository.CreateAsync(product, ct);
        var saved = await repository.GetByIdAsync(created.Id, ct);
        return MapToResponse(saved ?? created);
    }

    public async Task<bool> UpdateAsync(int id, ProductRequest request, CancellationToken ct)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, ct);

        var product = new Product
        {
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            Quantity = request.Quantity
        };
        return await repository.UpdateAsync(id, product, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        await repository.DeleteAsync(id, ct);

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        if (!await categoryRepository.ExistsAsync(categoryId, ct))
            throw new InvalidRequestException($"Category {categoryId} does not exist.");
    }

    private static ProductResponse MapToResponse(Product p) =>
        new(p.Id, p.Name, p.CategoryId, p.Category?.Name ?? string.Empty, p.Quantity);
}
