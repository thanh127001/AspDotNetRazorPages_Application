using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Setting;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;

    public IndexModel(IAppCoreService core)
    {
        _core = core;
    }

    public List<AppSettingModel> Settings { get; private set; } = new();
    public AppSettingModel? SelectedSetting { get; private set; }
    public string Mode { get; private set; } = "List";
    public List<string> Messages { get; private set; } = new();

    [BindProperty]
    public AppSettingModel Input { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    public async Task<IActionResult> OnGetAsync(string? mode, string? id, CancellationToken cancellationToken)
    {
        Mode = (mode ?? "List").ToLowerInvariant() switch
        {
            "edit" => "Edit",
            "details" => "Details",
            _ => "List"
        };

        if (Mode == "List")
        {
            var result = await _core.GetSettingsAsync(cancellationToken);
            if (result.Success)
            {
                Settings = result.Data ?? new();
            }
            else
            {
                Messages = result.Messages;
            }
            return Page();
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToPage();
        }

        var settingResult = await _core.GetSettingAsync(id, cancellationToken);
        if (!settingResult.Success || settingResult.Data is null)
        {
            Notice = string.Join(" ", settingResult.Messages);
            return RedirectToPage();
        }

        SelectedSetting = settingResult.Data;
        Input = settingResult.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken cancellationToken)
    {
        Mode = "Edit";
        var result = await _core.UpdateSettingAsync(Input, cancellationToken);
        if (!result.Success)
        {
            Messages = result.Messages;
            return Page();
        }
        Notice = string.Join(" ", result.Messages);
        return RedirectToPage();
    }

}
