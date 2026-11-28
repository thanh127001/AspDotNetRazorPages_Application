using Application.Models;

namespace Application.Services;

public class AppRuntimeService : IAppRuntimeService
{
    #region All
    public async Task<bool> CacheAllAsync()
    {
        return true;
    }
    #endregion
}
