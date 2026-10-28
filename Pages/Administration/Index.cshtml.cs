using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages.Administration;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
