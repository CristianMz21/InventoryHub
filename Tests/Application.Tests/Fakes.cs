using InventoryHub.Application.Contracts;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Tests;

/// <summary>
/// In-memory fakes: no mocking framework needed, behavior stays explicit.
/// </summary>
internal sealed class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = [];
    private readonly FakeCategoryRepository? _categories;

    public int LastPage
    {
        get; private set;
    }
    public int LastPageSize
    {
        get; private set;
    }
    public string? LastSearch
    {
        get; private set;
    }

    public FakeProductRepository(FakeCategoryRepository? categories = null) => _categories = categories;

    public void Seed(params Product[] products) => _products.AddRange(products);

    public Task<(int TotalCount, List<Product> Items)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        LastPage = page;
        LastPageSize = pageSize;
        LastSearch = search;
        var items = _products.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        AttachCategories(items);
        return Task.FromResult((_products.Count, items));
    }

    public Task<List<Product>> GetAllAsync(CancellationToken ct)
    {
        var items = _products.ToList();
        AttachCategories(items);
        return Task.FromResult(items);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is not null)
        {
            AttachCategories([product]);
        }

        return Task.FromResult(product);
    }

    public Task<Product> CreateAsync(Product product, CancellationToken ct)
    {
        product.Id = _products.Count == 0 ? 1 : _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<bool> UpdateAsync(int id, Product product, CancellationToken ct)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id);
        if (existing is null)
        {
            return Task.FromResult(false);
        }

        existing.Name = product.Name;
        existing.CategoryId = product.CategoryId;
        existing.Quantity = product.Quantity;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        Task.FromResult(_products.RemoveAll(p => p.Id == id) > 0);

    // Simula el Include(p => p.Category) del repositorio EF real.
    private void AttachCategories(List<Product> items)
    {
        if (_categories is null)
        {
            return;
        }

        foreach (var item in items)
        {
            item.Category = _categories.Find(item.CategoryId);
        }
    }
}

internal sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = [];

    public void Seed(params Category[] categories) => _categories.AddRange(categories);

    internal Category? Find(int id) => _categories.FirstOrDefault(c => c.Id == id);

    public Task<List<Category>> GetAllAsync(CancellationToken ct) => Task.FromResult(_categories.ToList());

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        Task.FromResult(_categories.Any(c => c.Id == id));

    public Task<Category> CreateAsync(Category category, CancellationToken ct)
    {
        category.Id = _categories.Count == 0 ? 1 : _categories.Max(c => c.Id) + 1;
        _categories.Add(category);
        return Task.FromResult(category);
    }

    public Task<bool> UpdateAsync(int id, Category category, CancellationToken ct)
    {
        var existing = _categories.FirstOrDefault(c => c.Id == id);
        if (existing is null)
        {
            return Task.FromResult(false);
        }

        existing.Name = category.Name;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct) =>
        Task.FromResult(_categories.RemoveAll(c => c.Id == id) > 0);
}
