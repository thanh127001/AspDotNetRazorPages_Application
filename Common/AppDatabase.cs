using Application.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Common;

public sealed class AppDatabase
{
    private readonly IServiceScopeFactory _scopes;
    private readonly AppRuntime _runtime;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AppDatabase(IServiceScopeFactory scopes, AppRuntime runtime)
    {
        _scopes = scopes;
        _runtime = runtime;
    }

    public static string DatabasePath => Path.Combine(AppContext.BaseDirectory, "AppData", "Application.db");

    public static string ConnectionString(string code) => new SqliteConnectionStringBuilder
    {
        DataSource = DatabasePath,
        Password = code
    }.ToString();

    public async Task InitializeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Access code is required.", nameof(code));
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (_runtime.IsInitialized && _runtime.AccessCode != code)
                throw new InvalidOperationException("Application is already initialized.");

            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
            using var scope = _scopes.CreateScope();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(ConnectionString(code)).Options;
            await using var db = new AppDbContext(options);
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                await using var command = db.Database.GetDbConnection().CreateCommand();
                command.CommandText = "PRAGMA cipher_version;";
                var version = await command.ExecuteScalarAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(version?.ToString()))
                    throw new InvalidOperationException("SQLCipher is not available.");
                await db.Database.MigrateAsync(cancellationToken);
            }
            finally { await db.Database.CloseConnectionAsync(); }
            _runtime.SetAccessCode(code);
        }
        finally { _gate.Release(); }
    }
}
