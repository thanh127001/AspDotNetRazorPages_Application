using Application.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class AppFileCacheService : IAppFileCacheService
{
    private readonly AppDbContext _db;
    private readonly string _directory;

    public AppFileCacheService(AppDbContext db, IWebHostEnvironment environment)
    {
        _db = db;
        _directory = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "files");
    }

    public static string GetUrl(string id, string fileName)
    {
        return "/files/" + Uri.EscapeDataString(fileName);
    }

    private string GetPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName) ||
            fileName is "." or ".." || fileName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0)
            throw new InvalidOperationException("Invalid file name.");
        return Path.Combine(_directory, fileName);
    }

    public async Task SyncAsync(string id, CancellationToken cancellationToken = default)
    {
        var file = await _db.AppFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (file is null) return;
        Directory.CreateDirectory(_directory);
        var path = GetPath(file.FileName);
        if (System.IO.File.Exists(path))
        {
            var info = new FileInfo(path);
            if (info.Length == file.FileSize) return;
        }
        var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await System.IO.File.WriteAllBytesAsync(temp, file.FileContent, cancellationToken);
            System.IO.File.Move(temp, path, true);
        }
        finally
        {
            if (System.IO.File.Exists(temp)) System.IO.File.Delete(temp);
        }
    }

    public Task DeleteAsync(string id, string fileName)
    {
        var path = GetPath(fileName);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        return Task.CompletedTask;
    }

    public async Task RestoreMissingAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var files = await _db.AppFiles.AsNoTracking()
            .Select(x => new { x.Id, x.FileName, x.FileSize })
            .ToListAsync(cancellationToken);
        foreach (var file in files)
        {
            var path = GetPath(file.FileName);
            if (!System.IO.File.Exists(path) || new FileInfo(path).Length != file.FileSize)
                await SyncAsync(file.Id, cancellationToken);
        }
    }
}
