using ChartForge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ChartForge.Web.Services;
using ChartForge.Infrastructure.Services;
using ChartForge.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        }
    )
);

builder.Services.AddHttpClient<IChatStreamService, N8nChatStreamService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["N8N:WebhookUrl"] ?? "http://localhost:5015");
});

builder.Services.AddScoped<ChatStateService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<ChartForge.Web.Components.App>()
    .AddInteractiveServerRenderMode();

// loading the NotFound page should typically work without this as long as we have the MapRazorComponents above
// however, the middleware for IsDevelopment is basically intercepting that so we need to add this redirect
// i also added the @page directive to the NotFound.razor page to make sure it can be accessed directly as well
app.MapFallback(() => Results.Redirect("/not-found"));
app.Run();