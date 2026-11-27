namespace Application.Services;

public interface IAppRuntimeService
{
    Task<bool> CacheAllAsync();
}
