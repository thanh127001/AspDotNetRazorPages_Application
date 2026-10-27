using Application.Models;

namespace Application.Services;

public interface IAppCoreService
{
    Task<AppResultModel<List<AppSettingModel>>> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<AppResultModel<AppSettingModel>> GetSettingAsync(string id, CancellationToken cancellationToken = default);
    Task<AppResultModel<string>> CreateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default);

    Task<AppResultModel<List<AppUserModel>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AppResultModel<AppUserModel>> GetUserAsync(string id, CancellationToken cancellationToken = default);
    Task<AppResultModel<string>> CreateUserAsync(AppUserModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> DeleteUserAsync(string id, CancellationToken cancellationToken = default);
}
