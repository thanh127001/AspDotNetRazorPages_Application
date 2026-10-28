using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

[Authorize(Policy = "AdministratorOnly")]
public class AdministrationModel : PageModel
{
    public void OnGet() { }
}
