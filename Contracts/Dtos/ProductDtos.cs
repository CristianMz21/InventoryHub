using System.ComponentModel.DataAnnotations;

using InventoryHub.Contracts;

namespace InventoryHub.Contracts.Dtos;

public record ProductResponse(int Id, string Name, int CategoryId, string CategoryName, int Quantity);

public record ProductRequest(
    [property: Required, StringLength(Limits.NameMaxLength)] string Name,
    [property: Range(Limits.CategoryIdMin, int.MaxValue)] int CategoryId,
    [property: Range(Limits.QuantityMin, int.MaxValue)] int Quantity);
