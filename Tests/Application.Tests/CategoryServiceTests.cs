using InventoryHub.Application.Services;
using InventoryHub.Contracts.Dtos;
using InventoryHub.Domain.Entities;

namespace InventoryHub.Application.Tests;

public class CategoryServiceTests
{
    [Fact]
    public async Task GetAllAsync_Maps_Entities_To_Responses()
    {
        var repository = new FakeCategoryRepository();
        repository.Seed(new Category
        {
            Id = 1,
            Name = "Office"
        });
        var service = new CategoryService(repository);

        var result = await service.GetAllAsync(CancellationToken.None);

        var single = Assert.Single(result);
        Assert.Equal(new CategoryResponse(1, "Office"), single);
    }

    [Fact]
    public async Task CreateAsync_Trims_Name()
    {
        var service = new CategoryService(new FakeCategoryRepository());

        var created = await service.CreateAsync(new CategoryRequest("  Toys  "), CancellationToken.None);

        Assert.Equal("Toys", created.Name);
        Assert.True(created.Id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Missing()
    {
        var service = new CategoryService(new FakeCategoryRepository());

        Assert.Null(await service.GetByIdAsync(42, CancellationToken.None));
    }

    [Fact]
    public async Task Update_And_Delete_Flow()
    {
        var service = new CategoryService(new FakeCategoryRepository());
        var created = await service.CreateAsync(new CategoryRequest("Temp"), CancellationToken.None);

        Assert.True(await service.UpdateAsync(created.Id, new CategoryRequest("Renamed"), CancellationToken.None));
        Assert.Equal("Renamed", (await service.GetByIdAsync(created.Id, CancellationToken.None))!.Name);
        Assert.True(await service.DeleteAsync(created.Id, CancellationToken.None));
        Assert.Null(await service.GetByIdAsync(created.Id, CancellationToken.None));
    }
}
