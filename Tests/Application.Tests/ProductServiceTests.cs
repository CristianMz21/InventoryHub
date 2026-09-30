using InventoryHub.Application.Exceptions;
using InventoryHub.Application.Services;
using InventoryHub.Contracts.Dtos;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Tests;

public class ProductServiceTests
{
    private static (ProductService Service, FakeProductRepository Products) CreateService(bool withCategory = true)
    {
        var categories = new FakeCategoryRepository();
        var products = new FakeProductRepository(categories);
        if (withCategory)
        {
            categories.Seed(new Category
            {
                Id = 1,
                Name = "Electronics"
            });
        }

        return (new ProductService(products, categories), products);
    }

    [Fact]
    public async Task GetPagedAsync_Clamps_Page_And_PageSize_Before_Querying()
    {
        var (service, products) = CreateService();
        products.Seed(new Product
        {
            Id = 1,
            Name = "Laptop",
            CategoryId = 1,
            Quantity = 5
        });

        await service.GetPagedAsync(page: 0, pageSize: 10_000, search: null, CancellationToken.None);

        Assert.Equal(1, products.LastPage);
        Assert.Equal(100, products.LastPageSize);
    }

    [Fact]
    public async Task GetPagedAsync_Trims_And_Truncates_Search()
    {
        var (service, products) = CreateService();

        await service.GetPagedAsync(1, 20, "  lap  ", CancellationToken.None);
        Assert.Equal("lap", products.LastSearch);

        await service.GetPagedAsync(1, 20, new string('x', 500), CancellationToken.None);
        Assert.Equal(100, products.LastSearch!.Length);
    }

    [Fact]
    public async Task GetPagedAsync_Computes_TotalPages()
    {
        var (service, products) = CreateService();
        products.Seed(
            new Product
            {
                Id = 1,
                Name = "A",
                CategoryId = 1,
                Quantity = 1
            },
            new Product
            {
                Id = 2,
                Name = "B",
                CategoryId = 1,
                Quantity = 2
            },
            new Product
            {
                Id = 3,
                Name = "C",
                CategoryId = 1,
                Quantity = 3
            });

        var result = await service.GetPagedAsync(1, 2, null, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Category_Does_Not_Exist()
    {
        var (service, _) = CreateService(withCategory: false);

        await Assert.ThrowsAsync<InvalidRequestException>(() =>
            service.CreateAsync(new ProductRequest("Ghost", 99, 1), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_Trims_Name_And_Maps_CategoryName()
    {
        var (service, _) = CreateService();

        var created = await service.CreateAsync(new ProductRequest("  Webcam  ", 1, 7), CancellationToken.None);

        Assert.Equal("Webcam", created.Name);
        Assert.Equal("Electronics", created.CategoryName);
        Assert.Equal(7, created.Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Missing()
    {
        var (service, _) = CreateService();

        Assert.Null(await service.GetByIdAsync(999, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Returns_False_When_Missing()
    {
        var (service, _) = CreateService();

        Assert.False(await service.UpdateAsync(999, new ProductRequest("X", 1, 1), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Removes_Product()
    {
        var (service, products) = CreateService();
        products.Seed(new Product
        {
            Id = 1,
            Name = "Mouse",
            CategoryId = 1,
            Quantity = 3,
            Category = new Category
            {
                Id = 1,
                Name = "Electronics"
            }
        });

        Assert.True(await service.DeleteAsync(1, CancellationToken.None));
        Assert.Null(await service.GetByIdAsync(1, CancellationToken.None));
    }
}
