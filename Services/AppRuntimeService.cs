using Application.Models;

namespace Application.Services;

public sealed class AppRuntimeService : IAppRuntimeService
{
    private readonly object _sync = new();
    private Dictionary<string, string> _settings = new(StringComparer.OrdinalIgnoreCase);
    private bool _settingsLoaded;
    private string? _accessCode;
    private bool _isInitialized;
    private int _failedInitiationAttempts;
    private const int MaxInitiationAttempts = 3;

    public bool AreSettingsLoaded { get { lock (_sync) return _settingsLoaded; } }
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
    public void SetSettings(IEnumerable<AppSettingModel> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var setting in settings)
        {
            if (!string.IsNullOrWhiteSpace(setting.Code))
                snapshot[setting.Code.Trim()] = setting.Value ?? string.Empty;
        }

        lock (_sync)
        {
            _settings = snapshot;
            _settingsLoaded = true;
        }
    }

    public string GetSettingValue(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return string.Empty;

        lock (_sync)
            return _settings.TryGetValue(code.Trim(), out var value) ? value : string.Empty;
    }

    public void SetSettingValue(string? code, string? value)
    {
        if (string.IsNullOrWhiteSpace(code))
            return;

        lock (_sync)
        {
            if (_settingsLoaded)
                _settings[code.Trim()] = value ?? string.Empty;
        }
    }

}
