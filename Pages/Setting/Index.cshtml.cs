using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.Setting;
public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    public IndexModel(IAppCoreService core) => _core = core;
    public List<AppSettingModel> Settings { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    [TempData] public string? Notice { get; set; }
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetSettingsAsync(cancellationToken);
        if(result.Success) Settings = result.Data ?? new();
        else Messages = result.Messages;
    }
}
