using Application.Common;
using Application.Data;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class AppCoreService : IAppCoreService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppCoreService> _logger;

    public AppCoreService(AppDbContext db, ILogger<AppCoreService> logger)
    {
        _db = db;
        _logger = logger;
    }

    private static AppSettingModel ToModel(AppSetting entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Name = entity.Name,
        Value = entity.Value,
        Order = entity.Order
    };

    private static AppUserModel ToModel(AppUser entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Username = entity.Username,
        Password = entity.Password,
        Email = entity.Email,
        PhoneNumber = entity.PhoneNumber,
        Status = entity.Status,
        Role = entity.Role,
        DateCreated = entity.DateCreated,
        DateModified = entity.DateModified
    };

    public async Task<List<AppSettingModel>> GetSettingsAsync()
    {
        try
        {
            var entities = await _db.AppSettings.AsNoTracking().OrderBy(x => x.Order).ThenBy(x => x.Name).ToListAsync();
            return entities.Select(ToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingsAsync failed");
            throw;
        }
    }

    public async Task<AppSettingModel?> GetSettingAsync(string id)
    {
        try
        {
            var entity = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return entity is null ? null : ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingAsync failed for record {Id}", id);
            throw;
        }
    }

    public async Task<AppSettingModel> CreateSettingAsync(AppSettingModel model)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model);
            var entity = new AppSetting
            {
                Id = AppIdGenerator.GenerateId(),
                Code = model.Code,
                Name = model.Name,
                Value = model.Value,
                Order = model.Order
            };
            _db.AppSettings.Add(entity);
            await _db.SaveChangesAsync();
            return ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSettingAsync failed for record {Id}", model?.Id);
            throw;
        }
    }

    public async Task<bool> UpdateSettingAsync(AppSettingModel model)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model);
            var entity = await _db.AppSettings.FindAsync(model.Id);
            if (entity is null)
            {
                return false;
            }
            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Value = model.Value;
            entity.Order = model.Order;
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync failed for record {Id}", model?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteSettingAsync(string id)
    {
        try
        {
            var entity = await _db.AppSettings.FindAsync(id);
            if (entity is null)
            {
                return false;
            }
            _db.AppSettings.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync failed for record {Id}", id);
            throw;
        }
    }

    public async Task<List<AppUserModel>> GetUsersAsync()
    {
        try
        {
            var entities = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username).ToListAsync();
            return entities.Select(ToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync failed");
            throw;
        }
    }

    public async Task<AppUserModel?> GetUserAsync(string id)
    {
        try
        {
            var entity = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return entity is null ? null : ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync failed for record {Id}", id);
            throw;
        }
    }

    public async Task<AppUserModel> CreateUserAsync(AppUserModel model)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model);
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                throw new ArgumentException("Password is required when creating a user.", nameof(model));
            }
            var entity = new AppUser
            {
                Id = AppIdGenerator.GenerateId(),
                Code = model.Code,
                Username = model.Username,
                Password = model.Password,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Status = model.Status,
                Role = model.Role,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _db.AppUsers.Add(entity);
            await _db.SaveChangesAsync();
            return ToModel(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync failed for record {Id}", model?.Id);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(AppUserModel model)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(model);
            var entity = await _db.AppUsers.FindAsync(model.Id);
            if (entity is null)
            {
                return false;
            }
            entity.Code = model.Code;
            entity.Username = model.Username;
            entity.Email = model.Email;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Status = model.Status;
            entity.Role = model.Role;
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                entity.Password = model.Password;
            }
            entity.DateModified = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync failed for record {Id}", model?.Id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        try
        {
            var entity = await _db.AppUsers.FindAsync(id);
            if (entity is null)
            {
                return false;
            }
            _db.AppUsers.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUserAsync failed for record {Id}", id);
            throw;
        }
    }
}
