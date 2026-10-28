using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Content;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    public IndexModel(IAppCoreService core) => _core = core;
    public List<AppContentModel> Contents { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    [TempData] public string? Notice { get; set; }
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetContentsAsync(cancellationToken);
        if (result.Success) Contents = result.Data ?? new();
        else Messages = result.Messages;
    }
}
