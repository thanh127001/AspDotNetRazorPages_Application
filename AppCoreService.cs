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

    #region Settings
    public async Task<AppResultModel<List<AppSettingModel>>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppSettings.AsNoTracking().OrderBy(x => x.Code)
            .Select(x => new AppSettingModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Value = x.Value
            }).ToListAsync(cancellationToken);

            return AppResultModel<List<AppSettingModel>>.Ok(models);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<List<AppSettingModel>>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<AppSettingModel?>> GetSettingAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppSettings.AsNoTracking().Select(x => new AppSettingModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Value = x.Value
            }).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppSettingModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppSettingModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppSettingModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<string>> CreateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<string>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<string>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return AppResultModel<string>.Fail("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<string>.Fail("Value is required.");
            }

            input.Code = input.Code.Trim();
            input.Name = input.Name.Trim();
            input.Value = input.Value.Trim();

            bool codeExists = await _db.AppSettings.AnyAsync(x => x.Code == input.Code, cancellationToken);
            if (codeExists)
            {
                return AppResultModel<string>.Fail("Code already exists.");
            }

            var entity = new AppSetting
            {
                Id = AppIdGenerator.GenerateId(),
                Code = input.Code,
                Name = input.Name,
                Value = input.Value
            };
            _db.AppSettings.Add(entity);
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

    public async Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<bool>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return AppResultModel<bool>.Fail("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<bool>.Fail("Value is required.");
            }

            input.Code = input.Code.Trim();
            input.Name = input.Name.Trim();
            input.Value = input.Value.Trim();

            var codeExists = await _db.AppSettings.AnyAsync(x => x.Code == input.Code && x.Id != input.Id, cancellationToken);
            if (codeExists)
            {
                return AppResultModel<bool>.Fail("Code already exists.");
            }

            var entity = await _db.AppSettings.FirstOrDefaultAsync(x => x.Id == input.Id, cancellationToken);
            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            entity.Code = input.Code;
            entity.Name = input.Name;
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

    public async Task<AppResultModel<bool>> DeleteSettingAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.AppSettings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            _db.AppSettings.Remove(entity);
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

    public async Task<string> GetSettingValueAsync(string code)
    {
        try
        {
            var entity = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);
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

    #region Contents
    public async Task<AppResultModel<List<AppContentModel>>> GetContentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppContents.AsNoTracking().OrderBy(x => x.Order)
            .Select(x => new AppContentModel
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Body = x.Body,
                Icon = x.Icon,
                Order = x.Order
            }).ToListAsync(cancellationToken);

            return AppResultModel<List<AppContentModel>>.Ok(models);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<List<AppContentModel>>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<AppContentModel?>> GetContentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppContents.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AppContentModel
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Body = x.Body,
                Icon = x.Icon,
                Order = x.Order,
                Status = x.Status
            }).FirstOrDefaultAsync(cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppContentModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppContentModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppContentModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<string>> CreateContentAsync(AppContentModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<string>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<string>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Title))
            {
                return AppResultModel<string>.Fail("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Body))
            {
                return AppResultModel<string>.Fail("Body is required.");
            }

            if (input.Order < 0)
            {
                return AppResultModel<string>.Fail("Invalid order.");
            }

            input.Code = input.Code.Trim();
            input.Title = input.Title.Trim();

            bool codeExists = await _db.AppContents.AnyAsync(
                x => x.Code == input.Code,
                cancellationToken);

            if (codeExists)
            {
                return AppResultModel<string>.Fail("Code already exists.");
            }

            var entity = new AppContent
            {
                Id = AppIdGenerator.GenerateId(),
                Code = input.Code,
                Title = input.Title,
                Body = input.Body,
                Icon = input.Icon,
                Order = input.Order,
                Status = input.Status
            };

            _db.AppContents.Add(entity);
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

    public async Task<AppResultModel<bool>> UpdateContentAsync(AppContentModel input, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<bool>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Title))
            {
                return AppResultModel<bool>.Fail("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Body))
            {
                return AppResultModel<bool>.Fail("Body is required.");
            }

            if (input.Order < 0)
            {
                return AppResultModel<bool>.Fail("Invalid  order.");
            }

            input.Code = input.Code.Trim();
            input.Title = input.Title.Trim();

            bool codeExists = await _db.AppContents.AnyAsync(
                x => x.Code == input.Code && x.Id != input.Id,
                cancellationToken);

            if (codeExists)
            {
                return AppResultModel<bool>.Fail("Code already exists.");
            }

            var entity = await _db.AppContents.FirstOrDefaultAsync(
                x => x.Id == input.Id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            if (string.Equals(entity.Code, "home", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(entity.Code, input.Code, StringComparison.OrdinalIgnoreCase)
                    || entity.Status != input.Status)
                {
                    return AppResultModel<bool>.Fail("An error occurred.");
                }
            }
            entity.Code = input.Code;
            entity.Title = input.Title;
            entity.Body = input.Body;
            entity.Icon = input.Icon;
            entity.Order = input.Order;
            entity.Status = input.Status;

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

    public async Task<AppResultModel<bool>> DeleteContentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.AppContents.FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            _db.AppContents.Remove(entity);
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

    public async Task<List<AppContentModel>> GetContentMenuAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppContents.AsNoTracking().Where(x => x.Status == 1).OrderBy(x => x.Order)
            .Select(x => new AppContentModel
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Body = string.Empty,
                Icon = x.Icon,
                Order = x.Order,
                Status = x.Status
            }).ToListAsync(cancellationToken);

            return models;
        }
        catch
        {
            return new List<AppContentModel>();
        }
    }

    public async Task<AppResultModel<AppContentModel?>> GetContentByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppContents.AsNoTracking()
            .Where(x => x.Code == code && x.Status == 1)
            .Select(x => new AppContentModel
            {
                Id = x.Id,
                Code = x.Code,
                Title = x.Title,
                Body = x.Body,
                Icon = x.Icon,
                Order = x.Order,
                Status = x.Status
            }).FirstOrDefaultAsync(cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppContentModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppContentModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppContentModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }
    #endregion

    #region Translations
    public async Task<AppResultModel<List<AppTranslationModel>>> GetTranslationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var models = await _db.AppTranslations.AsNoTracking()
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Language)
            .Select(x => new AppTranslationModel
            {
                Id = x.Id,
                Code = x.Code,
                Language = x.Language,
                Value = x.Value
            }).ToListAsync(cancellationToken);

            return AppResultModel<List<AppTranslationModel>>.Ok(models);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<List<AppTranslationModel>>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<AppTranslationModel?>> GetTranslationAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await _db.AppTranslations.AsNoTracking().Select(x => new AppTranslationModel
            {
                Id = x.Id,
                Code = x.Code,
                Language = x.Language,
                Value = x.Value
            }).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (model == null)
            {
                return AppResultModel<AppTranslationModel?>.Fail("Data not found.");
            }

            return AppResultModel<AppTranslationModel?>.Ok(model);
        }
        catch (Exception ex)
        {
            var errorCode = AppIdGenerator.GenerateId();
            var errorMessage = "An error occurred.";
            _logger.LogError(ex, $"[{errorCode}] {errorMessage}");
            return AppResultModel<AppTranslationModel?>.Fail($"[{errorCode}] {errorMessage}");
        }
    }

    public async Task<AppResultModel<string>> CreateTranslationAsync(AppTranslationModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input == null)
            {
                return AppResultModel<string>.Fail("Input cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<string>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Language))
            {
                return AppResultModel<string>.Fail("Language is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<string>.Fail("Value is required.");
            }

            input.Code = input.Code.Trim();
            input.Language = input.Language.Trim();
            input.Value = input.Value.Trim();

            bool translationExists = await _db.AppTranslations.AnyAsync(
                x => x.Code == input.Code
                    && x.Language == input.Language,
                cancellationToken);

            if (translationExists)
            {
                return AppResultModel<string>.Fail("Translation already exists.");
            }

            var entity = new AppTranslation
            {
                Id = AppIdGenerator.GenerateId(),
                Code = input.Code,
                Language = input.Language,
                Value = input.Value
            };

            _db.AppTranslations.Add(entity);
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

    public async Task<AppResultModel<bool>> UpdateTranslationAsync(AppTranslationModel input, CancellationToken cancellationToken = default)
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

            if (string.IsNullOrWhiteSpace(input.Code))
            {
                return AppResultModel<bool>.Fail("Code is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Language))
            {
                return AppResultModel<bool>.Fail("Language is required.");
            }

            if (string.IsNullOrWhiteSpace(input.Value))
            {
                return AppResultModel<bool>.Fail("Value is required.");
            }

            input.Code = input.Code.Trim();
            input.Language = input.Language.Trim();
            input.Value = input.Value.Trim();

            var translationExists = await _db.AppTranslations.AnyAsync(
                x => x.Code == input.Code
                    && x.Language == input.Language
                    && x.Id != input.Id,
                cancellationToken);

            if (translationExists)
            {
                return AppResultModel<bool>.Fail("Translation already exists.");
            }

            var entity = await _db.AppTranslations.FirstOrDefaultAsync(
                x => x.Id == input.Id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            entity.Code = input.Code;
            entity.Language = input.Language;
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

    public async Task<AppResultModel<bool>> DeleteTranslationAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.AppTranslations.FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

            if (entity == null)
            {
                return AppResultModel<bool>.Fail("Data not found.");
            }

            _db.AppTranslations.Remove(entity);
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

    public async Task<AppResultModel<List<string>>> UpsertTranslationListAsync(List<AppTranslationModel> input, CancellationToken cancellationToken = default)
    {
        var originalIds = new List<string>();

        try
        {
            // Begin transaction
            await using var transaction = await _db.Database
                .BeginTransactionAsync(cancellationToken);

            // Get existing translations
            var existingEntities = await _db.AppTranslations
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
                        Messages = new() { "Translation cannot be null." },
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

                if (string.IsNullOrWhiteSpace(item.Language))
                {
                    return new AppResultModel<List<string>>
                    {
                        Success = false,
                        Messages = new() { "Language is required." },
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

                item.Code = item.Code.Trim();
                item.Language = item.Language.Trim();
                item.Value = item.Value.Trim();
            }

            // Check duplicate translations in input
            var duplicate = input
                .GroupBy(x => (
                    Code: x.Code.ToUpperInvariant(),
                    Language: x.Language.ToUpperInvariant()
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
                    $"Translation '{item.Code}' ({item.Language}) is duplicated in input."
                },
                    Data = originalIds
                };
            }

            // Build dictionaries
            var existingDictionary = existingEntities.ToDictionary(x => (
                Code: x.Code.ToUpperInvariant(),
                Language: x.Language.ToUpperInvariant()
            ));

            var inputDictionary = input.ToDictionary(x => (
                Code: x.Code.ToUpperInvariant(),
                Language: x.Language.ToUpperInvariant()
            ));

            var ids = new List<string>();

            try
            {
                // Insert or update translations
                foreach (var item in input)
                {
                    var key = (
                        Code: item.Code.ToUpperInvariant(),
                        Language: item.Language.ToUpperInvariant()
                    );

                    if (existingDictionary.TryGetValue(key, out var entity))
                    {
                        // Update existing translation
                        entity.Code = item.Code;
                        entity.Language = item.Language;
                        entity.Value = item.Value;

                        ids.Add(entity.Id);
                    }
                    else
                    {
                        // Insert new translation
                        entity = new AppTranslation
                        {
                            Id = AppIdGenerator.GenerateId(),
                            Code = item.Code,
                            Language = item.Language,
                            Value = item.Value
                        };

                        _db.AppTranslations.Add(entity);

                        ids.Add(entity.Id);
                    }
                }

                // Delete translations that do not exist in input
                var entitiesToDelete = existingEntities
                    .Where(x => !inputDictionary.ContainsKey((
                        Code: x.Code.ToUpperInvariant(),
                        Language: x.Language.ToUpperInvariant()
                    )))
                    .ToList();

                _db.AppTranslations.RemoveRange(entitiesToDelete);

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

    public async Task<string> GetTranslationValueAsync(string code, string language)
    {
        try
        {
            var entity = await _db.AppTranslations.AsNoTracking().FirstOrDefaultAsync(
                x => x.Code == code && x.Language == language);

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
