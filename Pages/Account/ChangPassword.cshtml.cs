using System.Security.Claims;
using Application.Data;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
namespace Application.Pages.Account;
public class ChangPasswordModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly AppDbContext _db;
    public ChangPasswordModel(IAppCoreService core, AppDbContext db) { _core = core; _db = db; }
    [BindProperty] public PasswordInput Password { get; set; } = new();
    [TempData] public string? Notice { get; set; }
    public List<string> Messages { get; } = new();
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Page() : RedirectToPage("./Login");
    public sealed class PasswordInput
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6)] public string NewPassword { get; set; } = string.Empty;
        [Required] public string ConfirmPassword { get; set; } = string.Empty;
    }
    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("./Login");
        }

        
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToPage("./Login");
        }

        if (string.IsNullOrEmpty(Password.CurrentPassword) ||
            string.IsNullOrEmpty(Password.NewPassword) ||
            Password.NewPassword.Length < 6)
        {
            Messages.Add("Current password and a new password of at least 6 characters are required.");
            return Page();
        }

        if (Password.NewPassword != Password.ConfirmPassword)
        {
            Messages.Add("New password confirmation does not match.");
            return Page();
        }

        var entity = await _db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Status == 1, cancellationToken);
        if (entity is null || entity.Password != Password.CurrentPassword)
        {
            Messages.Add("Current password is incorrect.");
            return Page();
        }

        var result = await _core.GetUserAsync(id, cancellationToken);
        if (!result.Success || result.Data is null)
        {
            Messages.AddRange(result.Messages);
            return Page();
        }

        var input = result.Data;
        input.Password = Password.NewPassword;
        var update = await _core.UpdateUserAsync(input, cancellationToken);
        if (!update.Success)
        {
            Messages.AddRange(update.Messages);
            return Page();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Notice = "Password changed successfully. Please log in again.";
        return RedirectToPage("./Login");
    }
}
