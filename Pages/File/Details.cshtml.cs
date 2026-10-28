using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.File;

public class DetailsModel : PageModel
{
    private readonly IAppCoreService _core;
    public DetailsModel(IAppCoreService core) => _core = core;
    public AppFileModel? SelectedFile { get; private set; }
    [TempData] public string? Notice { get; set; }
    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetFileAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedFile = result.Data;
        return Page();
    }
}
