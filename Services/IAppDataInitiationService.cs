namespace Application.Services;

public interface IAppDataInitiationService
{
    Task InitializeAsync(
        string administratorPassword,
        CancellationToken cancellationToken = default);
}
