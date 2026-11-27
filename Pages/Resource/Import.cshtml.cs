using System.Text.Json;
using System.Text.Encodings.Web;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Resource;

public class ImportModel : AppPageModel
{
    private readonly IAppCoreService _appCoreService;

    public ImportModel(IAppCoreService appCoreService)
    {
        _appCoreService = appCoreService;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [BindProperty]
    public string TranslationJson { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await LoadTranslationListAsync(cancellationToken);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(TranslationJson))
        {
            SendMessages(new List<string>
            {
                "JSON is required."
            }, "danger");

            return Page();
        }

        List<AppResourceModel>? input;

        try
        {
            input = JsonSerializer.Deserialize<List<AppResourceModel>>(
                TranslationJson,
                _jsonOptions);
        }
        catch (JsonException)
        {
            SendMessages(new List<string>
            {
                "Invalid JSON format."
            }, "danger");

            return Page();
        }

        if (input == null || input.Count == 0)
        {
            SendMessages(new List<string>
            {
                "Translation list cannot be empty."
            }, "danger");

            return Page();
        }

        var result = await _appCoreService.UpsertResourceListAsync(
            input,
            cancellationToken);

        SendMessages(result.Messages, result.Success ? "success" : "danger");

        if (result.Success)
        {
            ModelState.Clear();

            await LoadTranslationListAsync(cancellationToken);
        }

        return Page();
    }

    private async Task LoadTranslationListAsync(CancellationToken cancellationToken)
    {
        var result = await _appCoreService.GetResourcesAsync(cancellationToken);

        if (!result.Success)
        {
            SendMessages(result.Messages, "danger");
            return;
        }

        if(result.Data != null && result.Data.Count == 0)
        {
            result.Data = new List<AppResourceModel>()
            {
                new AppResourceModel(){Code = "Title.Home", Culture = "en", Value = "Home"},
                new AppResourceModel(){Code = "Title.Home", Culture = "vi", Value = "Trang chủ"},
                new AppResourceModel(){Code = "Title.Home", Culture = "zh", Value = "首页"},
            };
        }

        var models = (result.Data ?? new())
            .Select(x => new
            {
                x.Code,
                x.Culture,
                x.Value
            })
            .ToList();
            

        TranslationJson = JsonSerializer.Serialize(models, _jsonOptions);
    }
}