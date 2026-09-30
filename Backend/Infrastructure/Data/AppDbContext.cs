using InventoryHub.Domain.Entities;

using Microsoft.EntityFrameworkCore;

namespace InventoryHub.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            // Índices para búsqueda y join por categoría (Actividad 4: optimización).
            e.HasIndex(p => p.Name);
            e.HasIndex(p => p.CategoryId);
            e.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed mínimo para que el revisor vea datos al primer run.
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Office" },
            new Category { Id = 3, Name = "Groceries" });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", CategoryId = 1, Quantity = 12 },
            new Product { Id = 2, Name = "Mouse", CategoryId = 1, Quantity = 50 },
            new Product { Id = 3, Name = "Keyboard", CategoryId = 1, Quantity = 35 },
            new Product { Id = 4, Name = "Notebook", CategoryId = 2, Quantity = 100 },
            new Product { Id = 5, Name = "Pen", CategoryId = 2, Quantity = 200 },
            new Product { Id = 6, Name = "Coffee", CategoryId = 3, Quantity = 40 });
    }
}
