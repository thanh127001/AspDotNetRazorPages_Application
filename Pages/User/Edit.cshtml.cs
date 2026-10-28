using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.User;
public class EditModel : PageModel
{
    private readonly IAppCoreService _core;
    public EditModel(IAppCoreService core) => _core = core;
    [BindProperty] public AppUserModel Input { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetUserAsync(id, cancellationToken);
        if(!result.Success || result.Data is null) { Notice = string.Join(" ", result.Messages); return RedirectToPage("./Index"); }
        Input = result.Data;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _core.UpdateUserAsync(Input, cancellationToken);
        if(!result.Success) { Messages = result.Messages; return Page(); }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
