using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Application.Models;

public class AppContentModel
{
    public string Id { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Icon { get; set; } = null;
    public int Order { get; set; } = 0;
    public int Status { get; set; } = 0;
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }

}
