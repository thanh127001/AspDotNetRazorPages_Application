using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppFileCacheService _fileCache;
    public IndexModel(IAppCoreService core, IAppFileCacheService fileCache)
    {
        _core = core;
        _fileCache = fileCache;
    }

    public List<AppFileModel> Files { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();

    [BindProperty] public IFormFile? Upload { get; set; }
    [BindProperty] public int Order { get; set; }
    [BindProperty] public string Id { get; set; } = string.Empty;
    [BindProperty] public string FileName { get; set; } = string.Empty;
    [TempData] public string? Notice { get; set; }

    public async Task OnGetAsync(CancellationToken cancellationToken) => await LoadAsync(cancellationToken);

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetFilesAsync(cancellationToken);
        if (result.Success) Files = result.Data ?? new();
        else Messages = result.Messages;
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0 || Upload.Length > 1024 * 1024)
        {
            Messages.Add("Choose a PNG, JPG, TXT or PDF file up to 1 MB.");
            await LoadAsync(cancellationToken);
            return Page();
        }

        await using var stream = new MemoryStream();
        await Upload.CopyToAsync(stream, cancellationToken);
        var result = await _core.CreateFileAsync(new AppFileModel
        {
            FileName = Upload.FileName,
            FileContent = stream.ToArray(),
            Order = Order
        }, cancellationToken);

        if (!result.Success)
        {
            Messages = result.Messages;
            await LoadAsync(cancellationToken);
            return Page();
        }

        await _fileCache.SyncAsync(result.Data!, cancellationToken);
        Notice = "File uploaded successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
    {
        var original = await _core.GetFileAsync(Id, cancellationToken);
        var result = await _core.UpdateFileAsync(Id, FileName, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            await LoadAsync(cancellationToken);
            return Page();
        }
        if (original.Success && original.Data is not null &&
            !string.Equals(original.Data.FileName, FileName, StringComparison.Ordinal))
            await _fileCache.DeleteAsync(Id, original.Data.FileName);
        await _fileCache.SyncAsync(Id, cancellationToken);
        Notice = "File renamed successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken)
    {
        var original = await _core.GetFileAsync(Id, cancellationToken);
        var result = await _core.DeleteFileAsync(Id, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            await LoadAsync(cancellationToken);
            return Page();
        }
        if (original.Success && original.Data is not null)
            await _fileCache.DeleteAsync(Id, original.Data.FileName);
        Notice = "File deleted successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadAsync(string id, CancellationToken cancellationToken)
    {
        var result = await _core.GetFileAsync(id, cancellationToken);
        if (!result.Success || result.Data is null) return NotFound();
        return File(result.Data.FileContent, result.Data.FileType, result.Data.FileName);
    }
}
