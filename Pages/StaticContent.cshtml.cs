using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace Application.Pages;

public class StaticContentModel : AppPageModel
{
    private readonly IWebHostEnvironment _environment;

    public StaticContentModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetContentAsync()
    {
        const string folder = "pdfeditor-main";

        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "AppData",
            "Files",
            folder,
            "index.html"
        );

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        var html = await System.IO.File.ReadAllTextAsync(filePath);

        var baseUrl = $"{Request.PathBase}/files/{folder}/";

        var baseTag = $"<base href=\"{baseUrl}\">";

        // Remove existing base tags
        html = Regex.Replace(
            html,
            @"<base\b[^>]*>",
            "",
            RegexOptions.IgnoreCase
        );

        // Insert base tag after opening head tag
        html = Regex.Replace(
            html,
            @"<head\b[^>]*>",
            match => match.Value + Environment.NewLine + baseTag,
            RegexOptions.IgnoreCase
        );

        return Content(html, "text/html; charset=utf-8");
    }
}