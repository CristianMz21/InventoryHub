using InventoryHub.Application.Contracts;
using InventoryHub.Domain.Entities;
using InventoryHub.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        await db.Categories.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<bool> ExistsAsync(int id, CancellationToken ct) =>
        await db.Categories.AsNoTracking().AnyAsync(c => c.Id == id, ct);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<bool> UpdateAsync(int id, Category category, CancellationToken ct)
    {
        var existing = await db.Categories.FindAsync([id], ct);
        if (existing is null)
            return false;

        existing.Name = category.Name;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var existing = await db.Categories.FindAsync([id], ct);
        if (existing is null)
            return false;

        db.Categories.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
