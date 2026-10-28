using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Content;

public class DetailsModel : PageModel
{
    private readonly IAppCoreService _core;
    public DetailsModel(IAppCoreService core) => _core = core;
    public AppContentModel? SelectedContent { get; private set; }
    [TempData] public string? Notice { get; set; }

    public async Task<IActionResult> OnGetAsync(string? id, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id)) return RedirectToPage("./Index");
        var result = await _core.GetContentAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Notice = string.Join(" ", result.Messages);
            return RedirectToPage("./Index");
        }
        SelectedContent = result.Data;
        return Page();
    }
}
