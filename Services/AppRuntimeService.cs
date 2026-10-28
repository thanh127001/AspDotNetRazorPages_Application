using Application.Models;

namespace Application.Services;

public sealed class AppRuntimeService : IAppRuntimeService
{
    private readonly object _sync = new();
    private Dictionary<string, string> _settings = new(StringComparer.OrdinalIgnoreCase);
    private bool _settingsLoaded;
    private Dictionary<string, string> _contentIds = new(StringComparer.OrdinalIgnoreCase);
    private List<AppContentModel> _navigationContents = new();
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

    public void SetContents(IEnumerable<AppContentModel> contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var content in contents)
        {
            if (!string.IsNullOrWhiteSpace(content.Code) && !string.IsNullOrWhiteSpace(content.Id))
                snapshot[content.Code.Trim()] = content.Id;
        }
        var navigation = contents
            .Where(x => !string.IsNullOrWhiteSpace(x.Code) && x.Status == 1 &&
                !new[] { "home", "privacy", "about" }.Contains(x.Code.Trim(), StringComparer.OrdinalIgnoreCase))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Title)
            .Select(x => new AppContentModel { Id = x.Id, Code = x.Code.Trim(), Title = x.Title, Icon = x.Icon, Order = x.Order, Status = x.Status })
            .ToList();
        lock (_sync) { _contentIds = snapshot; _navigationContents = navigation; }
    }

    public IReadOnlyList<AppContentModel> GetNavigationContents()
    {
        lock (_sync) return _navigationContents.ToList();
    }

    public string GetContentId(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        lock (_sync)
            return _contentIds.TryGetValue(code.Trim(), out var id) ? id : string.Empty;
    }
}
