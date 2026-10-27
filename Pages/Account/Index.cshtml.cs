using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application.Data;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Application.Pages.Account;

public class IndexModel : PageModel
{
    private readonly IAppCoreService _core;
    private readonly AppDbContext _db;

    public IndexModel(IAppCoreService core, AppDbContext db)
    {
        _core = core;
        _db = db;
    }

    public AppUserModel? CurrentUser { get; private set; }
    public string Mode { get; private set; } = "Index";
    public List<string> Messages { get; private set; } = new();

    [BindProperty]
    public LoginInput Login { get; set; } = new();

    [BindProperty]
    public PasswordInput Password { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    public sealed class LoginInput
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class PasswordInput
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(string? mode, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            Mode = "Login";
            return Page();
        }

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(id))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage();
        }

        var result = await _core.GetUserAsync(id, cancellationToken);
        if (!result.Success || result.Data is null || result.Data.Status != 1)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage();
        }

        CurrentUser = result.Data;
        Mode = string.Equals(mode, "ChangePassword", StringComparison.OrdinalIgnoreCase)
            ? "ChangePassword"
            : "Index";
        return Page();
    }

    public async Task<IActionResult> OnPostLoginAsync(CancellationToken cancellationToken)
    {
        Mode = "Login";
        if (string.IsNullOrWhiteSpace(Login.Username) || string.IsNullOrEmpty(Login.Password))
        {
            Messages.Add("Username and password are required.");
            return Page();
        }

        // Existing project stores passwords as plaintext. No changes to AppCoreService.
        var username = Login.Username.Trim();
        var entity = await _db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username && x.Status == 1, cancellationToken);

        if (entity is null || entity.Password != Login.Password)
        {
            Messages.Add("Invalid username or password.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, entity.Id),
            new(ClaimTypes.Name, entity.Username),
            new(ClaimTypes.Role, entity.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        Mode = "ChangePassword";
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(id))
        {
            return Challenge();
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
        return RedirectToPage();
    }
}
