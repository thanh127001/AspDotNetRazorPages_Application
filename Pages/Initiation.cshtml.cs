using System.ComponentModel.DataAnnotations;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Application.Pages;

public class InitiationModel : PageModel
{
    private readonly AppRuntimeService _runtime;
    private readonly AppDatabaseService _database;

    public InitiationModel(AppRuntimeService runtime, AppDatabaseService database)
    {
        _runtime = runtime;
        _database = database;
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
