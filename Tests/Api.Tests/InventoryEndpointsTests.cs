using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryHub.Api.Tests;

public class InventoryEndpointsTests
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Health_Returns_Healthy()
    {
        using var factory = new InventoryHubFactory();
        var response = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_Returns_Paged_CamelCase_Envelope()
    {
        using var factory = new InventoryHubFactory();
        var body = await factory.CreateClient().GetStringAsync("/products?page=1&pageSize=2");
        using var doc = JsonDocument.Parse(body);

        Assert.True(doc.RootElement.GetProperty("success").GetBoolean());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("page").GetInt32());
        Assert.Equal(2, data.GetProperty("pageSize").GetInt32());
        Assert.Equal(6, data.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, data.GetProperty("items").GetArrayLength());
        Assert.False(string.IsNullOrWhiteSpace(doc.RootElement.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task GetProducts_Search_Filters_By_Name()
    {
        using var factory = new InventoryHubFactory();
        var body = await factory.CreateClient().GetStringAsync("/products?search=note&page=1&pageSize=20");
        using var doc = JsonDocument.Parse(body);

        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(1, data.GetProperty("totalCount").GetInt32());
        Assert.Equal("Notebook", data.GetProperty("items")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetProduct_ById_NotFound_Returns_Fail_Envelope()
    {
        using var factory = new InventoryHubFactory();
        var response = await factory.CreateClient().GetAsync("/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.False(envelope.GetProperty("success").GetBoolean());
        Assert.Contains("999", envelope.GetProperty("error").GetString());
    }

    [Fact]
    public async Task CreateProduct_Returns_Created_Envelope()
    {
        using var factory = new InventoryHubFactory();
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/products", new
            {
                name = "Webcam",
                categoryId = 1,
                quantity = 7
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var envelope = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(envelope.GetProperty("success").GetBoolean());
        Assert.Equal("Webcam", envelope.GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public async Task CreateProduct_UnknownCategory_Returns_BadRequest()
    {
        using var factory = new InventoryHubFactory();
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/products", new
            {
                name = "Ghost",
                categoryId = 99,
                quantity = 1
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidDto_Returns_ValidationProblem()
    {
        using var factory = new InventoryHubFactory();
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/products", new
            {
                name = "",
                categoryId = 1,
                quantity = -1
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(problem.TryGetProperty("errors", out _));
    }

    [Fact]
    public async Task Update_Then_Get_Reflects_Change()
    {
        using var factory = new InventoryHubFactory();
        var client = factory.CreateClient();

        var put = await client.PutAsJsonAsync("/products/1", new
        {
            name = "Laptop Pro",
            categoryId = 1,
            quantity = 9
        });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var envelope = await client.GetFromJsonAsync<JsonElement>("/products/1", Json);
        Assert.Equal("Laptop Pro", envelope.GetProperty("data").GetProperty("name").GetString());
    }

    [Fact]
    public async Task Delete_Removes_Product()
    {
        using var factory = new InventoryHubFactory();
        var client = factory.CreateClient();

        var delete = await client.DeleteAsync("/products/2");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/products/2")).StatusCode);
    }

    [Fact]
    public async Task GetCategories_Returns_Seeded_Envelope()
    {
        using var factory = new InventoryHubFactory();
        var envelope = await factory.CreateClient().GetFromJsonAsync<JsonElement>("/categories", Json);

        Assert.True(envelope.GetProperty("success").GetBoolean());
        Assert.Equal(3, envelope.GetProperty("data").GetArrayLength());
    }
}
