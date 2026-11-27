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
                Role = x.Role
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
                Role = x.Role
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

            if (input.Status is not (0 or 1))
            {
                return AppResultModel<string>.Fail("Status is required.");
            }

            if (input.Role is not (0 or 1))
            {
                return AppResultModel<string>.Fail("Role is required.");
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
                Role = input.Role
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

            if (string.IsNullOrWhiteSpace(input.Username))
            {
                return AppResultModel<bool>.Fail("Username is required.");
            }

            if (input.Status is not (0 or 1))
            {
                return AppResultModel<bool>.Fail("Status is required.");
            }

            if (input.Role is not (0 or 1))
            {
                return AppResultModel<bool>.Fail("Role is required.");
            }

            input.Username = input.Username.Trim();

            var usernameExists = await _db.AppUsers.AnyAsync(
                x => x.Username == input.Username && x.Id != input.Id,
                cancellationToken);

            if (usernameExists)
            {
                return AppResultModel<bool>.Fail("Username already exists.");
            }

            var entity = await _db.AppUsers.FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken);
            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (string.Equals(entity.Username, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(entity.Username, input.Username, StringComparison.OrdinalIgnoreCase)
                    || entity.Status != input.Status
                    || entity.Role != input.Role)
                {
                    return AppResultModel<bool>.Fail("An error occurred.");
                }
            }
            else
            {
                entity.Username = input.Username;
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

            if (string.Equals(entity.Username, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                return AppResultModel<bool>.Fail("An error occurred.");
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
                Value = x.Value
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
                Value = x.Value
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

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<string>.Fail("Value is required.");
            }

            input.Type = input.Type.Trim();
            input.Code = input.Code.Trim();
            input.Culture = input.Culture?.Trim();
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
                Value = input.Value
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

            if (string.IsNullOrWhiteSpace(input.Type))
            {
                return AppResultModel<bool>.Fail("Type is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<bool>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<bool>.Fail("Value is required.");
            }

            input.Type = input.Type.Trim();
            input.Code = input.Code.Trim();
            input.Culture = input.Culture?.Trim();
            input.Value = input.Value.Trim();

            var resourceExists = await _db.AppResources.AnyAsync(
                x => x.Code == input.Code
                    && x.Culture == input.Culture
                    && x.Id != input.Id,
                cancellationToken);

            if (resourceExists)
            {
                return AppResultModel<bool>.Fail("Resource already exists.");
            }

            var entity = await _db.AppResources.FirstOrDefaultAsync(
                x => x.Id == input.Id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            entity.Code = input.Code;
            entity.Culture = input.Culture;
            entity.Value = input.Value;

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

    public async Task<AppResultModel<List<string>>> UpsertResourceListAsync(List<AppResourceModel> input, CancellationToken cancellationToken = default)
    {
        var originalIds = new List<string>();

        try
        {
            // Begin transaction
            await using var transaction = await _db.Database
                .BeginTransactionAsync(cancellationToken);

            // Get existing resources
            var existingEntities = await _db.AppResources
                .ToListAsync(cancellationToken);

            originalIds = existingEntities
                .Select(x => x.Id)
                .ToList();

            // Validate input
            if (input == null)
            {
                return new AppResultModel<List<string>>
                {
                    Success = false,
                    Messages = new() { "Input cannot be null." },
                    Data = originalIds
                };
            }

            if (input.Count == 0)
            {
                return new AppResultModel<List<string>>
                {
                    Success = false,
                    Messages = new() { "Input cannot be empty." },
                    Data = originalIds
                };
            }

            foreach (var item in input)
            {
                if (item == null)
                {
                    return new AppResultModel<List<string>>
                    {
                        Success = false,
                        Messages = new() { "Resource cannot be null." },
                        Data = originalIds
                    };
                }

                if (string.IsNullOrWhiteSpace(item.Type))
                {
                    return new AppResultModel<List<string>>
                    {
                        Success = false,
                        Messages = new() { "Language is required." },
                        Data = originalIds
                    };
                }

                if (string.IsNullOrWhiteSpace(item.Code))
                {
                    return new AppResultModel<List<string>>
                    {
                        Success = false,
                        Messages = new() { "Code is required." },
                        Data = originalIds
                    };
                }

                if (string.IsNullOrWhiteSpace(item.Value))
                {
                    return new AppResultModel<List<string>>
                    {
                        Success = false,
                        Messages = new() { "Value is required." },
                        Data = originalIds
                    };
                }

                item.Type = item.Type.Trim();
                item.Code = item.Code.Trim();
                item.Culture = item.Culture?.Trim();
                item.Value = item.Value.Trim();
            }

            // Check duplicate translations in input
            var duplicate = input
                .GroupBy(x => (
                    Code: x.Code.ToUpperInvariant(),
                    Language: x.Culture?.ToUpperInvariant()
                ))
                .FirstOrDefault(x => x.Count() > 1);

            if (duplicate != null)
            {
                var item = duplicate.First();

                return new AppResultModel<List<string>>
                {
                    Success = false,
                    Messages = new()
                {
                    $"Resource '{item.Code}' ({item.Culture}) is duplicated in input."
                },
                    Data = originalIds
                };
            }

            // Build dictionaries
            var existingDictionary = existingEntities.ToDictionary(x => (
                Code: x.Code.ToUpperInvariant(),
                Language: x.Culture?.ToUpperInvariant()
            ));

            var inputDictionary = input.ToDictionary(x => (
                Code: x.Code.ToUpperInvariant(),
                Language: x.Culture?.ToUpperInvariant()
            ));

            var ids = new List<string>();

            try
            {
                // Insert or update translations
                foreach (var item in input)
                {
                    var key = (
                        Code: item.Code.ToUpperInvariant(),
                        Language: item.Culture?.ToUpperInvariant()
                    );

                    if (existingDictionary.TryGetValue(key, out var entity))
                    {
                        // Update existing translation
                        entity.Type = item.Type;
                        entity.Code = item.Code;
                        entity.Culture = item.Culture;
                        entity.Value = item.Value;

                        ids.Add(entity.Id);
                    }
                    else
                    {
                        // Insert new resource
                        entity = new AppResource
                        {
                            Id = AppIdGenerator.GenerateId(),
                            Type = item.Type,
                            Code = item.Code,
                            Culture = item.Culture,
                            Value = item.Value
                        };

                        _db.AppResources.Add(entity);

                        ids.Add(entity.Id);
                    }
                }

                // Delete resources that do not exist in input
                var entitiesToDelete = existingEntities
                    .Where(x => !inputDictionary.ContainsKey((
                        Code: x.Code.ToUpperInvariant(),
                        Language: x.Culture?.ToUpperInvariant()
                    )))
                    .ToList();

                _db.AppResources.RemoveRange(entitiesToDelete);

                // Save all changes
                await _db.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                return AppResultModel<List<string>>.Ok(ids);
            }
            catch
            {
                // Rollback all changes
                await transaction.RollbackAsync(CancellationToken.None);

                // Clear tracked entities
                _db.ChangeTracker.Clear();

                throw;
            }
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";

            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");

            return new AppResultModel<List<string>>
            {
                Success = false,
                Messages = new()
            {
                $"[{errorCode}] {errorMessage}"
            },
                Data = originalIds
            };
        }
    }

    public async Task<string> GetResourceValueAsync(string code, string language)
    {
        try
        {
            var entity = await _db.AppResources.AsNoTracking().FirstOrDefaultAsync(
                x => x.Code == code && x.Culture == language);

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
