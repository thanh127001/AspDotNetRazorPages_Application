namespace Application.Services;

public sealed class AppRuntimeService
{
    private readonly object _sync = new();
    private string? _accessCode;
    public string? AccessCode { get { lock (_sync) return _accessCode; } }
    public bool IsInitialized => !string.IsNullOrWhiteSpace(AccessCode);

    public AppRuntimeService(IConfiguration configuration)
    {
        _accessCode = string.IsNullOrWhiteSpace(configuration["AccessCode"])
            ? null : configuration["AccessCode"];
    }

    public void SetAccessCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Access code is required.", nameof(code));
        lock (_sync) _accessCode = code;
    }
}
