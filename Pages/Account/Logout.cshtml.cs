using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Application.Pages.Account;
public class LogoutModel : PageModel
{
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? Page() : RedirectToPage("./Login");
    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated != true) return RedirectToPage("./Login");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("./Login");
    }
}