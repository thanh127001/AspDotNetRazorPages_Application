using Application.Models;

namespace Application.Services;

public interface IAppInitiationService
{
    Task<bool> InitiateAsync();
    Task<bool> InitiateResourcesAsync();
    Task<bool> InitiateUsersAsync();
}
