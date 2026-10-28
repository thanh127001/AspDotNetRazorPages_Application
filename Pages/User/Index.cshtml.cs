using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.User;
public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    public IndexModel(IAppCoreService core) => _core = core;
    public List<AppUserModel> Users { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    [TempData] public string? Notice { get; set; }
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetUsersAsync(cancellationToken);
        if(result.Success) Users = result.Data ?? new();
        else Messages = result.Messages;
    }
}
