using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.User;

public class IndexModel: AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public IndexModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    public List<AppUserModel> AppUserList { get; private set; } = new();
    
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetUsersAsync(cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        AppUserList = result.Data ?? new();
        return Page();
    }
}
