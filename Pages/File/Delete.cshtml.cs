using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class DeleteModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppFileCacheService _fileCache;
    public DeleteModel(IAppCoreService core, IAppFileCacheService fileCache)
    {
        _core = core;
        _fileCache = fileCache;
    }
    public AppFileModel? SelectedFile { get; private set; }
    [TempData] public string? Notice { get; set; }
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetFileAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedFile = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var original = await _core.GetFileAsync(id, cancellationToken);
        var result = await _core.DeleteFileAsync(id, cancellationToken);
        if (!result.Success)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        if (original.Success && original.Data is not null)
            await _fileCache.DeleteAsync(id, original.Data.FileName);
        Notice = "File deleted successfully.";
        return RedirectToPage("./Index");
    }
}
