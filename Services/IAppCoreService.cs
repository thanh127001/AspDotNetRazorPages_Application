using Application.Models;

namespace Application.Services;

public interface IAppCoreService
{
    Task<AppResultModel<List<AppUserModel>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AppResultModel<AppUserModel?>> GetUserAsync(string id, CancellationToken cancellationToken = default);
    Task<AppResultModel<string>> CreateUserAsync(AppUserModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> DeleteUserAsync(string id, CancellationToken cancellationToken = default);

    Task<AppResultModel<List<AppResourceModel>>> GetResourcesAsync(CancellationToken cancellationToken = default);
    Task<AppResultModel<AppResourceModel?>> GetResourceAsync(string id, CancellationToken cancellationToken = default);
    Task<AppResultModel<string>> CreateResourceAsync(AppResourceModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> UpdateResourceAsync(AppResourceModel input, CancellationToken cancellationToken = default);
    Task<AppResultModel<bool>> DeleteResourceAsync(string id, CancellationToken cancellationToken = default);
    Task<AppResultModel<List<string>>> UpsertResourceListAsync(List<AppResourceModel> input, CancellationToken cancellationToken = default);
    Task<string> GetResourceValueAsync(string code, string language);
}
