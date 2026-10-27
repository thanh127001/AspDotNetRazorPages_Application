namespace Application.Data;

public class AppUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Role { get; set; }
}
