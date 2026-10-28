using Application.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class AppDatabaseService : IAppDatabaseService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IAppRuntimeService _runtime;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public AppDatabaseService(IServiceScopeFactory scopes, IAppRuntimeService runtime)
    {
        _scopes = scopes;
        _runtime = runtime;
    }

    private static string _databasePath = Path.Combine(AppContext.BaseDirectory, "AppData", "Application.db");

    public static string DatabasePath => _databasePath;

    public static void ConfigureDatabasePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Database path is required.", nameof(path));
        _databasePath = Path.GetFullPath(path);
    }

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
                // Ensure the new BLOB table exists for databases created before AppFiles.
                await db.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE IF NOT EXISTS "AppFiles" (
                        "Id" TEXT NOT NULL PRIMARY KEY,
                        "FileName" TEXT NOT NULL,
                        "FileType" TEXT NOT NULL,
                        "FileSize" INTEGER NOT NULL,
                        "FileContent" BLOB NOT NULL,
                        "Order" INTEGER NOT NULL
                    );
                    """, cancellationToken);
            }
            finally { await db.Database.CloseConnectionAsync(); }
            _runtime.SetAccessCode(code);
        }
        finally { _gate.Release(); }
    }
}
