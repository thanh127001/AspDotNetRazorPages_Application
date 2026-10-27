namespace Application.Models;

public class AppFileModel
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int FileSize { get; set; } = 0;
    public byte[] FileContent { get; set; } = new byte[0];
    public int Order { get; set; } = 0;
}
