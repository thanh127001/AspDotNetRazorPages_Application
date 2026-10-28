using Serilog;
using Serilog.Events;
using Application.Data;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

SQLitePCL.Batteries_V2.Init();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) => logger
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true));
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Initiation");
    options.Conventions.AllowAnonymousToPage("/About");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Account/Index");
    options.Conventions.AuthorizePage("/User/Index", "AdministratorOnly");
    options.Conventions.AuthorizePage("/Content/Index", "AdministratorOnly");
    options.Conventions.AuthorizePage("/Setting/Index", "AdministratorOnly");
    options.Conventions.AuthorizePage("/File/Index", "AdministratorOnly");
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy =>
        policy.RequireAuthenticatedUser().RequireRole("1"));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Index";
        options.AccessDeniedPath = "/Account/Index";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });
builder.Services.AddSingleton<IAppRuntimeService, AppRuntimeService>();
builder.Services.AddSingleton<IAppDatabaseService, AppDatabaseService>();
builder.Services.AddScoped<IAppCoreService, AppCoreService>();
builder.Services.AddScoped<IAppFileCacheService, AppFileCacheService>();
builder.Services.AddScoped<IAppDataInitiationService, AppDataInitiationService>();
builder.Services.AddDbContext<AppDbContext>((services, options) =>
{
    var runtimeState = services.GetRequiredService<IAppRuntimeService>();
    if (!runtimeState.IsInitialized)
        throw new InvalidOperationException("Application database is not initialized.");
    options.UseSqlite(AppDatabaseService.ConnectionString(runtimeState.AccessCode!));
});

var app = builder.Build();
var runtime = app.Services.GetRequiredService<IAppRuntimeService>();
if (runtime.AccessCode is { } startupCode)
{
    await app.Services.GetRequiredService<IAppDatabaseService>().InitializeAsync(startupCode);
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IAppDataInitiationService>()
        .InitializeAsync("12345678");
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

app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();
