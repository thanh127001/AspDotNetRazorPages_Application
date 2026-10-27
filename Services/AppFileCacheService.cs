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
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return "/files/" + Uri.EscapeDataString(id) + extension;
    }

    private string GetPath(string id, string fileName)
    {
        // File names are generated from database IDs, never from untrusted upload names.
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (extension is not (".png" or ".jpg" or ".jpeg" or ".txt" or ".pdf"))
            throw new InvalidOperationException("Unsupported file extension.");
        if (string.IsNullOrEmpty(id) || id.Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_'))
            throw new InvalidOperationException("Invalid file ID.");
        return Path.Combine(_directory, id + extension);
    }

    public async Task SyncAsync(string id, CancellationToken cancellationToken = default)
    {
        var file = await _db.AppFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (file is null) return;
        Directory.CreateDirectory(_directory);
        var path = GetPath(file.Id, file.FileName);
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
        var path = GetPath(id, fileName);
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
            var path = GetPath(file.Id, file.FileName);
            if (!System.IO.File.Exists(path) || new FileInfo(path).Length != file.FileSize)
                await SyncAsync(file.Id, cancellationToken);
        }
    }
}
