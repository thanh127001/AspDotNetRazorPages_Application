using Application.Models;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Application.Pages;

public class IndexModel : AppPageModel
{

    public IActionResult OnGetAsync()
    {
        return Page();
    }
}
