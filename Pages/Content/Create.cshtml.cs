using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Content;

public class CreateModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;
    public CreateModel(IAppCoreService core, IAppRuntimeService runtime)
    {
        _core = core;
        _runtime = runtime;
    }
    [BindProperty] public AppContentModel Input { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    private async Task RefreshContentCacheAsync(CancellationToken cancellationToken)
    {
        var contents = await _core.GetContentsAsync(cancellationToken);
        if (!contents.Success || contents.Data is null)
            throw new InvalidOperationException("Unable to refresh content cache: " + string.Join(" ", contents.Messages));
        _runtime.SetContents(contents.Data);
    }

    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _core.CreateContentAsync(Input, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            return Page();
        }
        await RefreshContentCacheAsync(cancellationToken);
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
