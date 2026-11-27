using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class EditModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;
    private readonly IAppRuntimeService _appRuntimeService;

    public EditModel(IAppCoreService appCoreService, IAppRuntimeService appRuntimeService)
    {
        _appCoreService = appCoreService;
        _appRuntimeService = appRuntimeService;
    }

    [BindProperty]
    public AppResourceModel? ResourceToEdit { get; set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetResourceAsync(id ?? string.Empty, cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        ResourceToEdit = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _appCoreService.UpdateResourceAsync(ResourceToEdit ?? new AppResourceModel(), cancellationToken);
        SendMessages(result.Messages, result.Success ? "success" : "danger");
        return Page();
    }
}