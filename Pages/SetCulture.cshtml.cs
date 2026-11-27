using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Application.Pages;

public class SetCultureModel : PageModel
{
    private readonly RequestLocalizationOptions _options;

    public SetCultureModel(
        IOptions<RequestLocalizationOptions> options)
    {
        _options = options.Value;
    }

    public IActionResult OnPost(
        string culture,
        string? returnUrl)
    {
        var supported = _options.SupportedUICultures?
            .Any(x => x.Name == culture) == true;

        if (!supported)
        {
            culture = _options.DefaultRequestCulture
                .UICulture.Name;
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                IsEssential = true
            });

        return LocalRedirect(
            Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}