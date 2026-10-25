using Application.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

SQLitePCL.Batteries_V2.Init();

var databasePath = Path.Combine(builder.Environment.ContentRootPath, "Application.db");
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = databasePath,
    Password = builder.Configuration["AccessCode"] ?? throw new InvalidOperationException("Missing AccessCode configuration.")
}.ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.Run();
