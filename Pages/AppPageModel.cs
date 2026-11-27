using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Application.Pages;

public abstract class AppPageModel : PageModel
{
    public void SetTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ViewData.Remove("Title");
            return;
        }

        ViewData["Title"] = title.Trim();
    }

    protected void SendMessages(List<string> messages, string messageType)
    {
        var filteredMessages = messages.Where(message => !string.IsNullOrWhiteSpace(message)).ToList();
        if (filteredMessages.Count == 0)
        {
            return;
        }

        var type = messageType?.Trim().ToLowerInvariant();
        if (type is not ("success" or "info" or "warning" or "danger"))
        {
            type = "info";
        }

        TempData["Messages"] = JsonSerializer.Serialize(new { type, messages = filteredMessages });
    }
}
