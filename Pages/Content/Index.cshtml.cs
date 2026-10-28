using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Content;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;

    public IndexModel(IAppCoreService core) => _core = core;

    public List<AppContentModel> Contents { get; private set; } = new();
    public AppContentModel? SelectedContent { get; private set; }
    public string Action { get; private set; } = "List";
    public List<string> Messages { get; private set; } = new();

    [BindProperty]
    public AppContentModel Input { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    public async Task<IActionResult> OnGetAsync(string? action, string? id, CancellationToken cancellationToken)
    {
        Action = (action ?? "List").ToLowerInvariant() switch
        {
            "create" => "Create",
            "edit" => "Edit",
            "details" => "Details",
            "delete" => "Delete",
            _ => "List"
        };

        if (Action == "List")
        {
            var result = await _core.GetContentsAsync(cancellationToken);
            if (result.Success) Contents = result.Data ?? new();
            else Messages = result.Messages;
            return Page();
        }

        if (Action == "Create") return Page();
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage();

        var resultById = await _core.GetContentAsync(id, cancellationToken);
        if (!resultById.Success || resultById.Data is null)
        {
            Notice = string.Join(" ", resultById.Messages);
            return RedirectToPage();
        }

        SelectedContent = resultById.Data;
        Input = resultById.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        Action = "Create";
        var result = await _core.CreateContentAsync(Input, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            return Page();
        }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
    {
        Action = "Edit";
        var result = await _core.UpdateContentAsync(Input, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            return Page();
        }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, CancellationToken cancellationToken)
    {
        var result = await _core.DeleteContentAsync(id, cancellationToken);
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage();
    }
}
