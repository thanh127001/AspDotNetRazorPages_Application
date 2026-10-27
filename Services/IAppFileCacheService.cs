namespace Application.Services;

public interface IAppFileCacheService
{
    Task SyncAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string fileName);
    Task RestoreMissingAsync(CancellationToken cancellationToken = default);
}
