using Application.Models;

namespace Application.Services;

public sealed class AppDataInitiationService : IAppDataInitiationService
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;

    public AppDataInitiationService(IAppCoreService core, IAppRuntimeService runtime)
    {
        _core = core;
        _runtime = runtime;
    }

    public async Task InitializeAsync(
        string administratorPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(administratorPassword))
        {
            throw new ArgumentException(
                "Administrator password is required.",
                nameof(administratorPassword));
        }

        await InitializeAdministratorAsync(administratorPassword, cancellationToken);
        await InitializeSettingsAsync(cancellationToken);
        var settings = await _core.GetSettingsAsync(cancellationToken);
        if (!settings.Success || settings.Data is null)
            throw new InvalidOperationException(string.Join(" ", settings.Messages));
        _runtime.SetSettings(settings.Data);
    }

    private async Task InitializeAdministratorAsync(
        string administratorPassword,
        CancellationToken cancellationToken)
    {
        var result = await _core.GetUsersAsync(cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(" ", result.Messages));
        }

        if (result.Data is { Count: > 0 })
        {
            return;
        }

        var created = await _core.CreateUserAsync(new AppUserModel
        {
            Username = "administrator",
            Password = administratorPassword,
            Role = 1,
            Status = 1
        }, cancellationToken);

        if (!created.Success)
        {
            throw new InvalidOperationException(string.Join(" ", created.Messages));
        }
    }

    private async Task InitializeSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetSettingsAsync(cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(" ", result.Messages));
        }

        var settings = new[]
        {
            new AppSettingModel { Code = "app-name", Name = "App Name", Value = "Application", Order = 1 },
            new AppSettingModel { Code = "app-description", Name = "App Description", Value = "Application web tools and services.", Order = 2 },
            new AppSettingModel { Code = "app-icon", Name = "App Icon", Value = "", Order = 3 },
            new AppSettingModel { Code = "app-favicon", Name = "App Favicon", Value = "", Order = 4 },
            new AppSettingModel { Code = "app-copyright", Name = "App Copyright", Value = "© Application. All rights reserved.", Order = 5 }
        };

        var existing = result.Data ?? new List<AppSettingModel>();
        foreach (var setting in settings)
        {
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.Code, setting.Code, StringComparison.OrdinalIgnoreCase));

            if (current is null)
            {
                var created = await _core.CreateSettingAsync(setting, cancellationToken);
                if (!created.Success)
                    throw new InvalidOperationException(string.Join(" ", created.Messages));
            }
            else if ((setting.Code is "app-description" or "app-copyright")
                     && string.IsNullOrWhiteSpace(current.Value))
            {
                current.Value = setting.Value;
                var updated = await _core.UpdateSettingAsync(current, cancellationToken);
                if (!updated.Success)
                    throw new InvalidOperationException(string.Join(" ", updated.Messages));
            }
        }
    }
}
