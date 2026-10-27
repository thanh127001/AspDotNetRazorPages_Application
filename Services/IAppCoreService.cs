using Application.Models;

namespace Application.Services;

public interface IAppCoreService
{
    Task<List<AppSettingModel>> GetSettingsAsync();
    Task<AppSettingModel?> GetSettingAsync(string id);
    Task<AppSettingModel> CreateSettingAsync(AppSettingModel model);
    Task<bool> UpdateSettingAsync(AppSettingModel model);
    Task<bool> DeleteSettingAsync(string id);

    Task<List<AppUserModel>> GetUsersAsync();
    Task<AppUserModel?> GetUserAsync(string id);
    Task<AppUserModel> CreateUserAsync(AppUserModel model);
    Task<bool> UpdateUserAsync(AppUserModel model);
    Task<bool> DeleteUserAsync(string id);
}
