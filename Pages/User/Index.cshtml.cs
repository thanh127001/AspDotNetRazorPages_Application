using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.User;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;

    public IndexModel(IAppCoreService core)
    {
        _core = core;
    }

    public List<AppUserModel> Users { get; private set; } = new();
    public AppUserModel? SelectedUser { get; private set; }
    public string Mode { get; private set; } = "List";
    public List<string> Messages { get; private set; } = new();

    [BindProperty]
    public AppUserModel Input { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    public async Task<IActionResult> OnGetAsync(string? mode, string? id, CancellationToken cancellationToken)
    {
        Mode = (mode ?? "List").ToLowerInvariant() switch
        {
            "create" => "Create",
            "edit" => "Edit",
            "details" => "Details",
            "delete" => "Delete",
            _ => "List"
        };

        if (Mode == "List")
        {
            var result = await _core.GetUsersAsync(cancellationToken);
            if (result.Success)
            {
                Users = result.Data ?? new();
            }
            else
            {
                Messages = result.Messages;
            }
            return Page();
        }

        if (Mode == "Create")
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToPage();
        }

        var userResult = await _core.GetUserAsync(id, cancellationToken);
        if (!userResult.Success || userResult.Data is null)
        {
            Notice = string.Join(" ", userResult.Messages);
            return RedirectToPage();
        }

        SelectedUser = userResult.Data;
        Input = userResult.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        Mode = "Create";
        var result = await _core.CreateUserAsync(Input, cancellationToken);
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
        Mode = "Edit";
        var result = await _core.UpdateUserAsync(Input, cancellationToken);
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
        var result = await _core.DeleteUserAsync(id, cancellationToken);
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage();
    }
}
