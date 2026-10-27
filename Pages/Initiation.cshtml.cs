using System.ComponentModel.DataAnnotations;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Application.Pages;

public class InitiationModel : PageModel
{
    private readonly IAppRuntimeService _runtime;
    private readonly IAppDatabaseService _database;
    private readonly IAppDataInitiationService _dataInitiation;

    public InitiationModel(IAppRuntimeService runtime, IAppDatabaseService database, IAppDataInitiationService dataInitiation)
    {
        _runtime = runtime;
        _database = database;
        _dataInitiation = dataInitiation;
    }

    [BindProperty, Required]
    [Display(Name = "Access Code")]
    public string AccessCode { get; set; } = string.Empty;

    public bool IsLocked => _runtime.IsInitiationLocked;

    public IActionResult OnGet() => _runtime.IsInitialized ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (_runtime.IsInitialized) return RedirectToPage("/Index");
        if (IsLocked) return Page();
        if (!ModelState.IsValid) return Page();

        try
        {
            await _database.InitializeAsync(AccessCode, cancellationToken);
            await _dataInitiation.InitializeAsync("12345678", cancellationToken);
            return RedirectToPage("/Index");
        }
        catch (Exception ex) when (ex is SqliteException or InvalidOperationException)
        {
            _runtime.RegisterFailedInitiation();
            if (!IsLocked)
            {
                ModelState.AddModelError(string.Empty,
                    $"Could not initialize the database. {_runtime.RemainingInitiationAttempts} attempt(s) remaining.");
            }
            return Page();
        }
    }
}
