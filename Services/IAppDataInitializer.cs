namespace Application.Services;

public interface IAppDataInitializer
{
    Task InitializeAsync(
        string administratorPassword,
        CancellationToken cancellationToken = default);
}
