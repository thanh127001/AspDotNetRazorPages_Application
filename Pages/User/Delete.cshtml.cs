using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.User;
public class DeleteModel : PageModel
{
    private readonly IAppCoreService _core;
    public DeleteModel(IAppCoreService core) => _core = core;
    public AppUserModel? SelectedUser { get; private set; }
    [TempData] public string? Notice { get; set; }
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetUserAsync(id, cancellationToken);
        if(!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedUser = result.Data;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(string? id, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.DeleteUserAsync(id, cancellationToken);
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
