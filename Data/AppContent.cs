namespace Application.Data;

public class AppContent
{
    public string Id { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Group { get; set; } = 1;
    public bool Active { get; set; } = true;
    public int Order { get; set; }
}
