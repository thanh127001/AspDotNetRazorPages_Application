using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class CreateModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppFileCacheService _fileCache;
    public CreateModel(IAppCoreService core, IAppFileCacheService fileCache)
    {
        _core = core;
        _fileCache = fileCache;
    }
    [BindProperty] public IFormFile? Upload { get; set; }
    [BindProperty] public int Order { get; set; }
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Upload is null || Upload.Length == 0 || Upload.Length > 1024 * 1024)
        {
            Messages.Add("Choose a PNG, JPG, TXT or PDF file up to 1 MB.");
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
            return Page();
        }
        await _fileCache.SyncAsync(result.Data!, cancellationToken);
        Notice = "File uploaded successfully.";
        return RedirectToPage("./Index");
    }
}
