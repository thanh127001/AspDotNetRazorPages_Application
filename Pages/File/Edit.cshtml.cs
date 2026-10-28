using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class EditModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppFileCacheService _fileCache;
    public EditModel(IAppCoreService core, IAppFileCacheService fileCache)
    {
        _core = core;
        _fileCache = fileCache;
    }
    [BindProperty] public string Id { get; set; } = string.Empty;
    [BindProperty] public string FileName { get; set; } = string.Empty;
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetFileAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        Id = result.Data.Id;
        FileName = result.Data.FileName;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var original = await _core.GetFileAsync(Id, cancellationToken);
        var result = await _core.UpdateFileAsync(Id, FileName, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            return Page();
        }
        if (original.Success && original.Data is not null &&
            !string.Equals(original.Data.FileName, FileName, StringComparison.Ordinal))
            await _fileCache.DeleteAsync(Id, original.Data.FileName);
        await _fileCache.SyncAsync(Id, cancellationToken);
        Notice = "File renamed successfully.";
        return RedirectToPage("./Index");
    }
}
