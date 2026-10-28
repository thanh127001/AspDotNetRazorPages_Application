using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Content;

public class DeleteModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;
    public DeleteModel(IAppCoreService core, IAppRuntimeService runtime)
    {
        _core = core;
        _runtime = runtime;
    }
    public AppContentModel? SelectedContent { get; private set; }
    [TempData] public string? Notice { get; set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetContentAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedContent = result.Data;
        return Page();
    }
    private async Task RefreshContentCacheAsync(CancellationToken cancellationToken)
    {
        var contents = await _core.GetContentsAsync(cancellationToken);
        if (!contents.Success || contents.Data is null)
            throw new InvalidOperationException("Unable to refresh content cache: " + string.Join(" ", contents.Messages));
        _runtime.SetContents(contents.Data);
    }

    public async Task<IActionResult> OnPostAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.DeleteContentAsync(id, cancellationToken);
        if (result.Success)
        {
            await RefreshContentCacheAsync(cancellationToken);
        }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
