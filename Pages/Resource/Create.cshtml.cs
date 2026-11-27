using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class CreateModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public CreateModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    [BindProperty]
    public AppResourceModel ResourceToCreate { get; set; } = new();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _appCoreService.CreateResourceAsync(ResourceToCreate, cancellationToken);
        SendMessages(result.Messages, result.Success ? "success" : "danger");
        if (result.Success)
        {
            return RedirectToPage("Details", new { id = result.Data });
        }
        return Page();
    }
}
