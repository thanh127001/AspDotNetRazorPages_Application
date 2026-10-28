using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

public class AboutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index");
}
