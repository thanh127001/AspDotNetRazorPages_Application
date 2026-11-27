using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages.Account;
public class LogoutModel : AppPageModel
{
    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true ? Page() : RedirectToPage("./Login");
    }
    public async Task<IActionResult> OnPostAsync()
    {
        // Kiểm tra trạng thái đăng nhập của User trước khi cho phép thực hiện thao tác hoặc chuyển trang.
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("./Login");
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("./Login");
    }
}
