using Application.Common;
using Application.Data;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class AppCoreService : IAppCoreService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppCoreService> _logger;
    public AppCoreService(AppDbContext db, ILogger<AppCoreService> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Users
    public async Task<AppResultModel<List<AppUserModel>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username)
            .Select(x => new AppUserModel
            {
                Id = x.Id,
                Username = x.Username,
                Status = x.Status,
                Role = x.Role,
                Protected = x.Protected
            }).ToListAsync(cancellationToken);

            return AppResultModel<List<AppUserModel>>.Ok(models);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<List<AppUserModel>>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<AppUserModel?>> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppUsers.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AppUserModel
            {
                Id = x.Id,
                Username = x.Username,
                Status = x.Status,
                Role = x.Role,
                Protected = x.Protected
            }).FirstOrDefaultAsync(cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppUserModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppUserModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppUserModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<string>> CreateUserAsync(AppUserModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<string>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Username))
            {
                return AppResultModel<string>.Fail("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Password))
            {
                return AppResultModel<string>.Fail("Password is required.");
            }

            if (input.Role is not (0 or 1))
            {
                return AppResultModel<string>.Fail("Invalid role.");
            }

            if (input.Status is not (0 or 1))
            {
                return AppResultModel<string>.Fail("Status is required.");
            }

            input.Username = input.Username.Trim();

            bool usernameExists = await _db.AppUsers.AnyAsync(x => x.Username == input.Username, cancellationToken);
            if (usernameExists)
            {
                return AppResultModel<string>.Fail("Username already exists.");
            }

            var entity = new AppUser
            {
                Id = AppIdGenerator.GenerateId(),
                Username = input.Username,
                Password = input.Password,
                Status = input.Status,
                Role = input.Role,
                Protected = input.Protected
            };

            _db.AppUsers.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<string>.Ok(entity.Id);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<string>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<bool>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Id))
            {
                return AppResultModel<bool>.Fail("Id is required.");
            }

            var entity = await _db.AppUsers.FirstOrDefaultAsync(
                x => x.Id == input.Id.Trim(),
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (!entity.Protected)
            {
                if (string.IsNullOrWhiteSpace(input.Username))
                {
                    return AppResultModel<bool>.Fail("Username is required.");
                }

                if (input.Status is not (0 or 1))
                {
                    return AppResultModel<bool>.Fail("Status is required.");
                }

                if (input.Role is not (0))
                {
                    return AppResultModel<bool>.Fail("Role is required.");
                }

                var username = input.Username.Trim();

                var usernameExists = await _db.AppUsers.AnyAsync(
                    x => x.Username == username
                        && x.Id != entity.Id,
                    cancellationToken);

                if (usernameExists)
                {
                    return AppResultModel<bool>.Fail("Username already exists.");
                }

                entity.Username = username;
                entity.Status = input.Status;
                entity.Role = input.Role;
            }

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                entity.Password = input.Password;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<bool>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<bool>> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (entity.Protected)
            {
                return AppResultModel<bool>.Fail("Cannot delete a protected user.");
            }

            _db.AppUsers.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<bool>.Fail($"[{errorCode}] {errorMessage}");
        }
    }
    #endregion

    #region Resources
    public async Task<AppResultModel<List<AppResourceModel>>> GetResourcesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppResources.AsNoTracking()
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Culture)
            .Select(x => new AppResourceModel
            {
                Id = x.Id,
                Type = x.Type,
                Code = x.Code,
                Culture = x.Culture,
                Value = x.Value,
                Protected = x.Protected
            }).ToListAsync(cancellationToken);

            return AppResultModel<List<AppResourceModel>>.Ok(models);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<List<AppResourceModel>>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<AppResourceModel?>> GetResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppResources.AsNoTracking().Select(x => new AppResourceModel
            {
                Id = x.Id,
                Type = x.Type,
                Code = x.Code,
                Culture = x.Culture,
                Value = x.Value,
                Protected = x.Protected
            }).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppResourceModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppResourceModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppResourceModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<string>> CreateResourceAsync(AppResourceModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<string>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Type))
            {
                return AppResultModel<string>.Fail("Type is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<string>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Culture))
            {
                return AppResultModel<string>.Fail("Culture is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<string>.Fail("Value is required.");
            }

            input.Type = input.Type.Trim();
            input.Code = input.Code.Trim();
            input.Culture = input.Culture.Trim();
            input.Value = input.Value.Trim();

            bool resourceExists = await _db.AppResources.AnyAsync(
                x => x.Code == input.Code
                    && x.Culture == input.Culture,
                cancellationToken);

            if (resourceExists)
            {
                return AppResultModel<string>.Fail("Resource already exists.");
            }

            var entity = new AppResource
            {
                Id = AppIdGenerator.GenerateId(),
                Type = input.Type,
                Code = input.Code,
                Culture = input.Culture,
                Value = input.Value,
                Protected = input.Protected
            };

            _db.AppResources.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<string>.Ok(entity.Id);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<string>.Fail($"[{errorCode}] {errorMessage}");
        }
    }


    public async Task<AppResultModel<bool>> UpdateResourceAsync(AppResourceModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<bool>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Id))
            {
                return AppResultModel<bool>.Fail("Id is required.");
            }

            var entity = await _db.AppResources.FirstOrDefaultAsync(
                x => x.Id == input.Id.Trim(),
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<bool>.Fail("Value is required.");
            }

            if (!entity.Protected)
            {
                if (string.IsNullOrWhiteSpace(input.Type))
                {
                    return AppResultModel<bool>.Fail("Type is required.");
                }

                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    return AppResultModel<bool>.Fail("Code is required.");
                }

                if (string.IsNullOrWhiteSpace(input.Culture))
                {
                    return AppResultModel<bool>.Fail("Culture is required.");
                }

                var type = input.Type.Trim();
                var code = input.Code.Trim();
                var culture = input.Culture.Trim();

                var resourceExists = await _db.AppResources.AnyAsync(
                    x => x.Code == code
                        && x.Culture == culture
                        && x.Id != entity.Id,
                    cancellationToken);

                if (resourceExists)
                {
                    return AppResultModel<bool>.Fail("Resource already exists.");
                }

                entity.Type = type;
                entity.Code = code;
                entity.Culture = culture;
            }

            entity.Value = input.Value.Trim();

            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<bool>.Fail($"[{errorCode}] {errorMessage}");
        }
    }


    public async Task<AppResultModel<bool>> DeleteResourceAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.AppResources.FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (entity.Protected)
            {
                return AppResultModel<bool>.Fail("Cannot delete a protected resource.");
            }

            _db.AppResources.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return AppResultModel<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<bool>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<string> GetResourceValueAsync(string code, string culture)
    {
        try
        {
            var entity = await _db.AppResources.AsNoTracking().FirstOrDefaultAsync(
                x => x.Code == code && x.Culture == culture);

            if (entity == null)
            {
                return code;
            }

            return entity.Value;
        }
        catch
        {
            return code;
        }
    }
    #endregion
}
