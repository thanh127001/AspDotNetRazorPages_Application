using Application.Models;

namespace Application.Services;

public interface IAppRuntimeService
{
    string? AccessCode { get; }
    bool IsInitialized { get; }
    bool IsInitiationLocked { get; }
    int RemainingInitiationAttempts { get; }
    void RegisterFailedInitiation();
    void SetAccessCode(string code);
    bool AreSettingsLoaded { get; }
    void SetSettings(IEnumerable<AppSettingModel> settings);
    string GetSettingValue(string? code);
    void SetSettingValue(string? code, string? value);
    void SetContents(IEnumerable<AppContentModel> contents);
    string GetContentId(string? code);
}
