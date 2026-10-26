using Application.Common;
using Application.Data;
using Application.Services;
using Microsoft.EntityFrameworkCore;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddSingleton<AppRuntimeService>();
builder.Services.AddSingleton<AppDatabase>();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
{
    var code = services.GetRequiredService<AppRuntimeService>().AccessCode
        ?? throw new InvalidOperationException("Application is not initialized.");
    options.UseSqlite(AppDatabase.ConnectionString(code));
});

var app = builder.Build();
var runtime = app.Services.GetRequiredService<AppRuntimeService>();
if (runtime.AccessCode is { } startupCode)
{
    await app.Services.GetRequiredService<AppDatabase>().InitializeAsync(startupCode);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.Use(async (context, next) =>
{
    if (!runtime.IsInitialized &&
        !context.Request.Path.StartsWithSegments("/Initiation") &&
        !context.Request.Path.StartsWithSegments("/_framework") &&
        !context.Request.Path.StartsWithSegments("/_content"))
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
