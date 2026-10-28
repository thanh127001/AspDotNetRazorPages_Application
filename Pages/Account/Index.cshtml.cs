using System.Security.Cryptography;
using System.Text;
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

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public LoginInput Login { get; set; } = new();

    [BindProperty]
    public PasswordInput Password { get; set; } = new();

    [TempData]
    public string? Notice { get; set; }

    [TempData]
    public string? LoginCaptchaCode { get; set; }

    [BindProperty]
    public string CaptchaInput { get; set; } = string.Empty;

    public string CaptchaImageBase64 { get; private set; } = string.Empty;

    private void GenerateCaptcha()
    {
        var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        LoginCaptchaCode = code;
        var seed = RandomNumberGenerator.GetInt32(1000, 9999);
        var digits = string.Concat(code.Select((digit, index) =>
        {
            var x = 20 + index * 31 + RandomNumberGenerator.GetInt32(-3, 4);
            var y = 46 + RandomNumberGenerator.GetInt32(-8, 9);
            var angle = RandomNumberGenerator.GetInt32(-22, 23);
            var fontSize = RandomNumberGenerator.GetInt32(33, 42);
            return $"""<text x="{x}" y="{y}" transform="rotate({angle} {x} {y})" font-family="monospace" font-size="{fontSize}" font-weight="bold" fill="#374151">{digit}</text>""";
        }));
        var noise = string.Concat(Enumerable.Range(0, 14).Select(_ =>
        {
            var x1 = RandomNumberGenerator.GetInt32(0, 220);
            var y1 = RandomNumberGenerator.GetInt32(0, 70);
            var x2 = RandomNumberGenerator.GetInt32(0, 220);
            var y2 = RandomNumberGenerator.GetInt32(0, 70);
            return $"""<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="#9ca3af" stroke-width="1" opacity=".7"/>""";
        }));
        var svg = $"""
            <svg xmlns="http://www.w3.org/2000/svg" width="220" height="70" viewBox="0 0 220 70">
              <defs>
                <filter id="distort"><feTurbulence type="fractalNoise" baseFrequency="0.035" numOctaves="3" seed="{seed}" result="noise"/><feDisplacementMap in="SourceGraphic" in2="noise" scale="10"/></filter>
              </defs>
              <rect width="220" height="70" fill="#e5e7eb"/>
              <g filter="url(#distort)">{digits}</g>
              {noise}
              <path d="M0 18 Q55 68 110 22 T220 47 M0 51 Q80 0 150 55 T220 14 M0 39 Q70 8 140 46 T220 30" fill="none" stroke="#6b7280" stroke-width="1.5" opacity=".8"/>
            </svg>
            """;
        CaptchaImageBase64 = "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));
    }

    public IActionResult OnPostRefreshCaptcha()
    {
        if (User.Identity?.IsAuthenticated == true) return BadRequest();
        GenerateCaptcha();
        Response.Headers.CacheControl = "no-store";
        return new JsonResult(new { image = CaptchaImageBase64 });
    }

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
            GenerateCaptcha();
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
        var expectedCaptcha = TempData[nameof(LoginCaptchaCode)] as string;
        GenerateCaptcha(); // One-time challenge; regenerate on every failed login.
        if (string.IsNullOrEmpty(expectedCaptcha) || !string.Equals(CaptchaInput.Trim(), expectedCaptcha, StringComparison.Ordinal))
        {
            Messages.Add("Invalid verification code. Please try the new code.");
            return Page();
        }

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

        return !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl)
            ? LocalRedirect(ReturnUrl)
            : RedirectToPage();
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
