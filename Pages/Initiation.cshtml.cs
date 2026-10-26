using System.ComponentModel.DataAnnotations;
using Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

public class InitiationModel : PageModel
{
    private readonly AppRuntime _runtime;
    private readonly AppDatabase _database;

    public InitiationModel(AppRuntime runtime, AppDatabase database)
    {
        _runtime = runtime;
        _database = database;
    }

    [BindProperty, Required]
    [Display(Name = "Access Code")]
    public string AccessCode { get; set; } = string.Empty;

    public IActionResult OnGet() => _runtime.IsInitialized ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (_runtime.IsInitialized) return RedirectToPage("/Index");
        if (!ModelState.IsValid) return Page();
        try
        {
            await _database.InitializeAsync(AccessCode, cancellationToken);
            return RedirectToPage("/Index");
        }
        catch (Exception ex) when (ex is SqliteException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "Could not open or initialize the database. Check the access code and SQLCipher configuration.");
            return Page();
        }
    }
}
