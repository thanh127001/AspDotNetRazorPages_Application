using Application.Models;

namespace Application.Services;

public sealed class AppDataInitiationService : IAppDataInitiationService
{
    private readonly IAppCoreService _core;

    public AppDataInitiationService(IAppCoreService core)
    {
        _core = core;
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

        var result = await _core.GetUsersAsync(cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                string.Join(" ", result.Messages));
        }

        if (result.Data is { Count: > 0 })
        {
            return;
        }

        var input = new AppUserModel
        {
            Code = "administrator",
            Username = "administrator",
            Password = administratorPassword,
            Role = 1,
            Status = 1
        };

        var created = await _core.CreateUserAsync(input, cancellationToken);

        if (!created.Success)
        {
            throw new InvalidOperationException(
                string.Join(" ", created.Messages));
        }
    }
}
