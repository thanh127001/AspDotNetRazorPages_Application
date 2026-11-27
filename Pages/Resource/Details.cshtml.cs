using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class DetailsModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public DetailsModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    public AppResourceModel? SelectedResource { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetResourceAsync(id ?? string.Empty, cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        SelectedResource = result.Data;
        return Page();
    }
}
