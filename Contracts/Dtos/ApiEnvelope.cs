namespace InventoryHub.Contracts.Dtos;

/// <summary>
/// Standard JSON envelope for every API response.
/// Keeps front-end parsing predictable: always { success, data, error, traceId }.
/// Serialized as camelCase (success, data, error, traceId) via HttpJsonOptions.
/// </summary>
public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Error,
    string TraceId)
{
    public static ApiResponse<T> Ok(T data, string traceId) =>
        new(true, data, null, traceId);

    public static ApiResponse<T> Fail(string error, string traceId) =>
        new(false, default, error, traceId);
}

/// <summary>
/// Paginated payload used inside <see cref="ApiResponse{T}"/>.
/// JSON shape: { items, page, pageSize, totalCount, totalPages }.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
