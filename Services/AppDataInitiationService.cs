using Application.Models;

namespace Application.Services;

public sealed class AppDataInitiationService : IAppDataInitiationService
{
    private readonly IAppCoreService _core;
    private readonly IAppRuntimeService _runtime;
    private readonly IAppFileCacheService _fileCache;

    public AppDataInitiationService(IAppCoreService core, IAppRuntimeService runtime, IAppFileCacheService fileCache)
    {
        _core = core;
        _runtime = runtime;
        _fileCache = fileCache;
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
        await InitializeContentsAsync(cancellationToken);
        var contents = await _core.GetContentsAsync(cancellationToken);
        if (!contents.Success || contents.Data is null)
            throw new InvalidOperationException(string.Join(" ", contents.Messages));
        _runtime.SetContents(contents.Data);
        var settings = await _core.GetSettingsAsync(cancellationToken);
        if (!settings.Success || settings.Data is null)
            throw new InvalidOperationException(string.Join(" ", settings.Messages));
        _runtime.SetSettings(settings.Data);
        await _fileCache.RestoreMissingAsync(cancellationToken);
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
    private async Task InitializeContentsAsync(CancellationToken cancellationToken)
    {
        var result = await _core.GetContentsAsync(cancellationToken);
        if (!result.Success)
            throw new InvalidOperationException(string.Join(" ", result.Messages));

        var defaults = new[]
        {
            new AppContentModel { Code = "home", Title = "Home", Content = "<p>Welcome to our website.</p>", Status = 1, Order = 1 },
            new AppContentModel { Code = "privacy", Title = "Privacy", Content = "<p>Privacy policy content.</p>", Status = 1, Order = 2 },
            new AppContentModel { Code = "about", Title = "About", Content = "<p>About this website.</p>", Status = 1, Order = 3 }
        };

        var existing = result.Data ?? new List<AppContentModel>();
        var initialContents = existing.Count == 0
            ? defaults.Concat(new[]
            {
                new AppContentModel { Code = "sample-content-1", Title = "Sample Content 1", Content = "<p>Sample content 1.</p>", Status = 1, Order = 4 },
                new AppContentModel { Code = "sample-content-2", Title = "Sample Content 2", Content = "<p>Sample content 2.</p>", Status = 1, Order = 5 },
                new AppContentModel { Code = "sample-content-3", Title = "Sample Content 3", Content = "<p>Sample content 3.</p>", Status = 1, Order = 6 }
            })
            : defaults.AsEnumerable();

        foreach (var item in initialContents)
        {
            if (existing.Any(x => string.Equals(x.Code, item.Code, StringComparison.OrdinalIgnoreCase)))
                continue;
            var created = await _core.CreateContentAsync(item, cancellationToken);
            if (!created.Success)
                throw new InvalidOperationException(string.Join(" ", created.Messages));
        }
    }

}
