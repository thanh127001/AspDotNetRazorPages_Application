using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

public class PrivacyModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");
}
