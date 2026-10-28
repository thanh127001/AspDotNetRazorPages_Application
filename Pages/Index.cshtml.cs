using Application.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Application.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public AppContent? Content { get; private set; }
    public string ContentCode { get; private set; } = "home";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ContentCode = Request.Path.Value?.TrimEnd('/').ToLowerInvariant() switch
        {
            "/privacy" => "privacy",
            "/about" => "about",
            _ => "home"
        };
        Content = await _db.AppContents.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == ContentCode && x.Status == 1, cancellationToken);
    }
}
