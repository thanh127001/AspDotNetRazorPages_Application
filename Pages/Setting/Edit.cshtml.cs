using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.Setting;
public class EditModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;
    public EditModel(IAppCoreService core, IAppRuntimeService runtime) { _core = core; _runtime = runtime; }
    [BindProperty] public AppSettingModel Input { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; private set; } = new();
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetSettingAsync(id, cancellationToken);
        if(!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        Input = result.Data;
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var result = await _core.UpdateSettingAsync(Input, cancellationToken);
        if(!result.Success) { Messages = result.Messages; return Page(); }
        var saved = await _core.GetSettingAsync(Input.Id, cancellationToken);
        if(saved.Success && saved.Data is not null) _runtime.SetSettingValue(saved.Data.Code, saved.Data.Value);
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage("./Index");
    }
}
