using Application.Models;

namespace Application.Services;

public interface IAppCoreService
{
    Task<AppResultModel<List<AppSettingModel>>> GetSettingsAsync();
    Task<AppResultModel<AppSettingModel>> GetSettingAsync(string id);
    Task<AppResultModel<AppSettingModel>> CreateSettingAsync(AppSettingModel model);
    Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel model);
    Task<AppResultModel<bool>> DeleteSettingAsync(string id);

    Task<AppResultModel<List<AppUserModel>>> GetUsersAsync();
    Task<AppResultModel<AppUserModel>> GetUserAsync(string id);
    Task<AppResultModel<AppUserModel>> CreateUserAsync(AppUserModel model);
    Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel model);
    Task<AppResultModel<bool>> DeleteUserAsync(string id);
}
