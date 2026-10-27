namespace Application.Services;

public interface IAppDatabaseService
{
    Task InitializeAsync(string code, CancellationToken cancellationToken = default);
}
