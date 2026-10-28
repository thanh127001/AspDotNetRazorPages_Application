using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.Setting;
public class DetailsModel : PageModel
{
    private readonly IAppCoreService _core;
    public DetailsModel(IAppCoreService core) => _core = core;
    public AppSettingModel? SelectedSetting { get; private set; }
    [TempData] public string? Notice { get; set; }
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetSettingAsync(id, cancellationToken);
        if(!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedSetting = result.Data;
        return Page();
    }
}
