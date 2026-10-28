using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.User;
public class CreateModel : PageModel
{
    private readonly IAppCoreService _core;
    public CreateModel(IAppCoreService core) => _core = core;
    [BindProperty] public AppUserModel Input { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _core.CreateUserAsync(Input, cancellationToken);
        if(!result.Success) { Messages = result.Messages; return Page(); }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
