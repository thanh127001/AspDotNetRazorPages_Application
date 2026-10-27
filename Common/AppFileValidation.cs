using System.Text;

namespace Application.Common;

public static class AppFileValidation
{
    public const int MaxFileSize = 1024 * 1024;

    public static bool TryGetFileType(string? fileName, byte[]? content, out string fileType)
    {
        fileType = string.Empty;
        if (string.IsNullOrWhiteSpace(fileName) || content is null ||
            content.Length == 0 || content.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        ReadOnlySpan<byte> data = content;
        switch (extension)
        {
            case ".png" when data.Length >= 8 &&
                data[..8].SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10}):
                fileType = "image/png";
                return true;
            case ".jpg" or ".jpeg" when data.Length >= 4 &&
                data[0] == 0xFF && data[1] == 0xD8 &&
                data[^2] == 0xFF && data[^1] == 0xD9:
                fileType = "image/jpeg";
                return true;
            case ".pdf" when data.Length >= 5 &&
                data[..5].SequenceEqual("%PDF-"u8):
                fileType = "application/pdf";
                return true;
            case ".txt":
                try
                {
                    var text = new UTF8Encoding(false, true).GetString(content);
                    if (text.Contains('\0')) return false;
                    fileType = "text/plain";
                    return true;
                }
                catch (DecoderFallbackException) { return false; }
        }
        return false;
    }
}
