using System.Threading;
using Application.Models;

namespace Application.Services;

public class AppInitiationService : IAppInitiationService
{
    private static int _initiated = 0;

    private readonly IAppCoreService _core;
    private readonly ILogger<AppInitiationService> _logger;

    public AppInitiationService(
        IAppCoreService core,
        ILogger<AppInitiationService> logger)
    {
        _core = core;
        _logger = logger;
    }

    #region Initiation

    public async Task<bool> InitiateAsync()
    {
        if (Interlocked.CompareExchange(ref _initiated, 1, 0) != 0)
        {
            return false;
        }

        _logger.LogInformation("Application initiation started.");

        var settingsResult = await InitiateResourcesAsync();
        if (!settingsResult)
        {
            _logger.LogError("Application initiation failed during resource initiation.");
            return false;
        }

        var usersResult = await InitiateUsersAsync();
        if (!usersResult)
        {
            _logger.LogError("Application initiation failed during users initiation.");
            return false;
        }

        _logger.LogInformation("Application initiation completed.");
        return true;
    }

    #endregion

    #region Resources
    public async Task<bool> InitiateResourcesAsync()
    {
        var existingResources = await _core.GetResourcesAsync();
        if (!existingResources.Success || existingResources.Data == null)
        {
            _logger.LogError("Failed to retrieve existing resources.");
            foreach (var error in existingResources.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        var defaults = new List<AppResourceModel>
        {
            new()
            {
                Type = "Setting",
                Code = "App.Name",
                Value = "Application"
            },
            new()
            {
                Type="Setting",
                Code = "App.Description",                
                Value = "ASP.NET Core Razor Pages Application"
            },
            new()
            {
                Type = "Setting",
                Code = "App.Copyright",
                Value = "Copyright © 2026"
            }
        };

        foreach (var item in defaults)
        {
            if (existingResources.Data.Any(s => s.Code == item.Code))
            {
                _logger.LogInformation("Resource '{Code}' already exists. Skipping creation.", item.Code);
                continue;
            }

            var created = await _core.CreateResourceAsync(item);

            if (!created.Success)
            {
                _logger.LogError("Failed to create resource '{Code}'", item.Code);
                foreach (var error in created.Messages)
                {
                    _logger.LogError("Error: {Error}", error);
                }

                return false;
            }

            _logger.LogInformation("Created resource: {Code}", item.Code);
        }

        return true;
    }
    #endregion

    #region Users
    public async Task<bool> InitiateUsersAsync()
    {
        var existingUsers = await _core.GetUsersAsync();
        if (!existingUsers.Success || existingUsers.Data == null)
        {
            _logger.LogError("Failed to retrieve existing users.");
            foreach (var error in existingUsers.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        if (existingUsers.Data.Any(u => u.Username == "administrator"))
        {
            _logger.LogInformation("Administrator account already exists. Skipping creation.");
            return true;
        }

        var administrator = new AppUserModel
        {
            Username = "administrator",
            Password = "12345678",
            Status = 1,
            Role = 1
        };

        var created = await _core.CreateUserAsync(administrator);

        if (!created.Success)
        {
            _logger.LogError("Failed to create administrator account.");
            foreach (var error in created.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        _logger.LogInformation("Administrator account created.");
        return true;
    }

    #endregion

}
