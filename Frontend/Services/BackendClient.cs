using System.Net;
using System.Net.Http.Json;

using InventoryHub.Contracts;
using InventoryHub.Contracts.Dtos;

namespace InventoryHub.Frontend.Services;

/// <summary>
/// Typed HttpClient: única puerta front -> back (Actividad 1).
/// - Desenvuelve el envelope ApiResponse{T} (Actividad 3).
/// - Reintenta 3 veces con backoff en 5xx/timeout (Actividad 2).
/// - Propaga CancellationToken para cancelar búsquedas viejas (Actividad 4).
/// </summary>
public class BackendClient(HttpClient http, ILogger<BackendClient> logger)
{
    private const int MaxAttempts = 3;

    public async Task<PagedResult<ProductResponse>> GetProductsPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var url = $"{ApiRoutes.Products}?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var envelope = await GetWithRetryAsync<ApiResponse<PagedResult<ProductResponse>>>(url, ct);
        if (envelope is { Success: true, Data: not null })
            return envelope.Data;

        throw new BackendApiException(envelope?.Error ?? "Empty product list response.");
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var envelope = await GetWithRetryAsync<ApiResponse<List<CategoryResponse>>>(ApiRoutes.Categories, ct);
        if (envelope is { Success: true, Data: not null })
            return envelope.Data;

        throw new BackendApiException(envelope?.Error ?? "Empty categories response.");
    }

    public async Task<ProductResponse> CreateProductAsync(ProductRequest request, CancellationToken ct = default)
    {
        using var response = await ExecuteWithRetryAsync(
            c => http.PostAsJsonAsync(ApiRoutes.Products, request, c), ct);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<ProductResponse>>(ct);
        return envelope is { Success: true, Data: not null }
            ? envelope.Data
            : throw new BackendApiException(envelope?.Error ?? "Create failed.");
    }

    public async Task UpdateProductAsync(int id, ProductRequest request, CancellationToken ct = default)
    {
        using var response = await ExecuteWithRetryAsync(
            c => http.PutAsJsonAsync($"{ApiRoutes.Products}/{id}", request, c), ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new BackendApiException($"Product {id} not found.");
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        using var response = await ExecuteWithRetryAsync(
            c => http.DeleteAsync($"{ApiRoutes.Products}/{id}", c), ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new BackendApiException($"Product {id} not found.");
        response.EnsureSuccessStatusCode();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CategoryRequest request, CancellationToken ct = default)
    {
        using var response = await ExecuteWithRetryAsync(
            c => http.PostAsJsonAsync(ApiRoutes.Categories, request, c), ct);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryResponse>>(ct);
        return envelope is { Success: true, Data: not null }
            ? envelope.Data
            : throw new BackendApiException(envelope?.Error ?? "Create category failed.");
    }

    // --- Resiliencia ---

    private async Task<T?> GetWithRetryAsync<T>(string url, CancellationToken ct)
    {
        using var response = await ExecuteWithRetryAsync(c => http.GetAsync(url, c), ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(ct);
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation, CancellationToken ct)
    {
        Exception? lastError = null;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var response = await operation(ct);
                // Reintenta solo 5xx y timeout; 4xx vuelve directo (no tiene sentido reintentar).
                if ((int)response.StatusCode < 500)
                    return response;

                response.Dispose();
                lastError = new BackendApiException($"Backend returned {(int)response.StatusCode}.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                lastError = ex;
                logger.LogWarning(ex, "Backend call failed (attempt {Attempt}/{Max}). Retrying...", attempt, MaxAttempts);
            }

            if (attempt < MaxAttempts)
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct);
        }

        throw lastError ?? new BackendApiException("Backend unreachable after retries.");
    }
}

public sealed class BackendApiException(string message) : Exception(message);
