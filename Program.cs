using Application.Data;
using Application.Services;
using Microsoft.EntityFrameworkCore;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<AppRuntimeService>();
builder.Services.AddSingleton<AppDatabaseService>();
builder.Services.AddScoped<IAppCoreService, AppCoreService>();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
{
    var runtimeState = services.GetRequiredService<AppRuntimeService>();
    if (!runtimeState.IsInitialized)
        throw new InvalidOperationException("Application database is not initialized.");
    options.UseSqlite(AppDatabaseService.ConnectionString(runtimeState.AccessCode!));
});

var app = builder.Build();
var runtime = app.Services.GetRequiredService<AppRuntimeService>();
if (runtime.AccessCode is { } startupCode)
{
    await app.Services.GetRequiredService<AppDatabaseService>().InitializeAsync(startupCode);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve wwwroot static assets before the initialization gate.
app.UseStaticFiles();
app.UseRouting();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var staticAssetRequest = path.StartsWithSegments("/_framework") ||
                             path.StartsWithSegments("/_content") ||
                             path.Value?.EndsWith(".css", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".js", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".map", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) == true ||
                             path.Value?.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) == true;

    if (!runtime.IsInitialized &&
        !path.StartsWithSegments("/Initiation") &&
        !staticAssetRequest)
    {
        context.Response.Redirect("/Initiation");
        return;
    }

    await next(context);
});

app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();
