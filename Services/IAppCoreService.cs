using Application.Models;

namespace Application.Services;

public interface IAppCoreService
{
    Task<AppResult<List<AppSettingModel>>> GetSettingsAsync();
    Task<AppResult<AppSettingModel>> GetSettingAsync(string id);
    Task<AppResult<AppSettingModel>> CreateSettingAsync(AppSettingModel model);
    Task<AppResult<bool>> UpdateSettingAsync(AppSettingModel model);
    Task<AppResult<bool>> DeleteSettingAsync(string id);

    Task<AppResult<List<AppUserModel>>> GetUsersAsync();
    Task<AppResult<AppUserModel>> GetUserAsync(string id);
    Task<AppResult<AppUserModel>> CreateUserAsync(AppUserModel model);
    Task<AppResult<bool>> UpdateUserAsync(AppUserModel model);
    Task<AppResult<bool>> DeleteUserAsync(string id);
}
