using System.Security.Claims;
using Application.Data;
using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Pages.Account;
public class IndexModel : AppPageModel
{
    private readonly IAppCoreService _core;
    // Nhận core từ hệ thống đăng ký dịch vụ để khởi tạo các field của IndexModel.
    public IndexModel(IAppCoreService core)
    {
        _core = core;
    }

    public AppUserModel? CurrentUser { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        // Kiểm tra trạng thái đăng nhập của User trước khi cho phép thực hiện thao tác hoặc chuyển trang.
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("./Login");
        }

        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = string.IsNullOrWhiteSpace(id) ? null : await _core.GetUserAsync(id, cancellationToken);
        if (result is null || !result.Success || result.Data is null || result.Data.Status != 1)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("./Login");
        }

        CurrentUser = result.Data;
        return Page();
    }
}
