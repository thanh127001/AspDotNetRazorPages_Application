using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class IndexModel: AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public IndexModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    public List<AppResourceModel> AppTranslationList { get; private set; } = new();
    
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetResourcesAsync(cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        AppTranslationList = result.Data ?? new();
        return Page();
    }
}
