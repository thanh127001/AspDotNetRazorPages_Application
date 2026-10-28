using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    public IndexModel(IAppCoreService core) => _core = core;
    public List<AppFileModel> Files { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    [TempData] public string? Notice { get; set; }
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetFilesAsync(cancellationToken);
        if (result.Success) Files = result.Data ?? new();
        else Messages = result.Messages;
    }
    public async Task<IActionResult> OnGetDownloadAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var result = await _core.GetFileAsync(id, cancellationToken);
        if (!result.Success || result.Data is null) return NotFound();
        return File(result.Data.FileContent, result.Data.FileType, result.Data.FileName);
    }
}
