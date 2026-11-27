using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.User;

public class DetailsModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public DetailsModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    public AppUserModel? AppUserModel { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetUserAsync(id ?? string.Empty, cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        AppUserModel = result.Data;
        return Page();
    }
}
