using InventoryHub.Application.Contracts;
using InventoryHub.Domain.Entities;
using InventoryHub.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<(int TotalCount, List<Product> Items)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        // AsNoTracking: lectura sin change-tracker = menos memoria y CPU.
        var query = db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%"));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (totalCount, items);
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken ct) =>
        await db.Products.AsNoTracking().Include(p => p.Category).OrderBy(p => p.Id).ToListAsync(ct);

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct) =>
        await db.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<bool> UpdateAsync(int id, Product product, CancellationToken ct)
    {
        var existing = await db.Products.FindAsync([id], ct);
        if (existing is null)
            return false;

        existing.Name = product.Name;
        existing.CategoryId = product.CategoryId;
        existing.Quantity = product.Quantity;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var existing = await db.Products.FindAsync([id], ct);
        if (existing is null)
            return false;

        db.Products.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
