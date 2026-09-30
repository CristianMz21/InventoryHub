using InventoryHub.Frontend.Components;
using InventoryHub.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<BackendClient>(client =>
{
    var backendBaseUrl = builder.Configuration["Backend:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Backend:BaseUrl is not configured. See docs/INTEGRATION-TROUBLESHOOTING.md #1.");
    client.BaseAddress = new Uri(backendBaseUrl);
    // Timeout total por request; el retry con backoff vive en BackendClient.
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
