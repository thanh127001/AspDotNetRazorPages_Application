namespace Application.Services;

public interface IAppDataInitializerServiceService
{
    Task InitializeAsync(
        string administratorPassword,
        CancellationToken cancellationToken = default);
}
