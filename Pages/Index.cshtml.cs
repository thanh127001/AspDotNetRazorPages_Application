using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;

    public IndexModel(IAppCoreService core, IAppRuntimeService runtime)
    {
        _core = core;
        _runtime = runtime;
    }

    public AppContentModel? Content { get; private set; }
    public string ContentCode { get; private set; } = "home";

    public async Task OnGetAsync(string? code, CancellationToken cancellationToken)
    {
        ContentCode = string.IsNullOrWhiteSpace(code) ? "Home" : code.Trim();

        var id = _runtime.GetContentId(ContentCode);
        if (string.IsNullOrEmpty(id)) return;

        var result = await _core.GetContentAsync(id, cancellationToken);
        if (result.Success && result.Data?.Status == 1)
            Content = result.Data;
    }
}
