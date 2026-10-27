namespace Application.Services;

public sealed class AppRuntimeService : IAppRuntimeService
{
    private readonly object _sync = new();
    private string? _accessCode;
    private bool _isInitialized;
    private int _failedInitiationAttempts;
    private const int MaxInitiationAttempts = 3;

    public string? AccessCode { get { lock (_sync) return _accessCode; } }
    public bool IsInitialized { get { lock (_sync) return _isInitialized; } }
    public bool IsInitiationLocked { get { lock (_sync) return _failedInitiationAttempts >= MaxInitiationAttempts; } }
    public int RemainingInitiationAttempts { get { lock (_sync) return Math.Max(0, MaxInitiationAttempts - _failedInitiationAttempts); } }

    public AppRuntimeService(IConfiguration configuration)
    {
        var configuredCode = configuration["AccessCode"];
        _accessCode = string.IsNullOrWhiteSpace(configuredCode) ? null : configuredCode;
    }

    public void RegisterFailedInitiation()
    {
        lock (_sync)
        {
            if (!_isInitialized && _failedInitiationAttempts < MaxInitiationAttempts)
                _failedInitiationAttempts++;
        }
    }

    public void SetAccessCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Access code is required.", nameof(code));
        lock (_sync)
        {
            _accessCode = code;
            _isInitialized = true;
            _failedInitiationAttempts = 0;
        }
    }
}
