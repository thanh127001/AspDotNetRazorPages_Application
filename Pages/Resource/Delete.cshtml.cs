using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class DeleteModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public DeleteModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    public AppResourceModel? ResourceToDelete { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetResourceAsync(id ?? string.Empty, cancellationToken);
        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
        }
        ResourceToDelete = result.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id, CancellationToken cancellationToken)
    {
        var result = await _appCoreService.DeleteResourceAsync(id ?? string.Empty, cancellationToken);
        SendMessages(result.Messages, result.Success ? "success" : "danger");
        if (result.Success)
        {
            return RedirectToPage("Index");
        }
        return await OnGetAsync(id, cancellationToken);
    }
}
