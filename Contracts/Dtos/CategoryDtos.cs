using System.ComponentModel.DataAnnotations;

using InventoryHub.Contracts;

namespace InventoryHub.Contracts.Dtos;

public record CategoryResponse(int Id, string Name);

public record CategoryRequest(
    [property: Required, StringLength(Limits.NameMaxLength)] string Name);
