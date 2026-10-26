namespace Application.Services;

public sealed class AppRuntimeService
{
    private readonly object _sync = new();
    private string? _accessCode;
    private bool _isInitialized;

    public string? AccessCode { get { lock (_sync) return _accessCode; } }
    public bool IsInitialized { get { lock (_sync) return _isInitialized; } }

    public AppRuntimeService(IConfiguration configuration)
    {
        var configuredCode = configuration["AccessCode"];
        _accessCode = string.IsNullOrWhiteSpace(configuredCode) ? null : configuredCode;
    }

    public void SetAccessCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Access code is required.", nameof(code));
        lock (_sync)
        {
            _accessCode = code;
            _isInitialized = true;
        }
    }
}
