namespace Application.Services;

public interface IAppRuntimeService
{
    string? AccessCode { get; }
    bool IsInitialized { get; }
    bool IsInitiationLocked { get; }
    int RemainingInitiationAttempts { get; }
    void RegisterFailedInitiation();
    void SetAccessCode(string code);
}
