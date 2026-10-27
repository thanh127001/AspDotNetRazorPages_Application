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

    public async Task<AppResultModel<List<AppSettingModel>>> GetSettingsAsync()
    {
        try
        {
            var rows = await _db.AppSettings.AsNoTracking().OrderBy(x => x.Order).ThenBy(x => x.Name).ToListAsync();
            return AppResultModel<List<AppSettingModel>>.Ok(rows.Select(ToModel).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingsAsync failed");
            return AppResultModel<List<AppSettingModel>>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<AppSettingModel>> GetSettingAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResultModel<AppSettingModel>.Fail("Setting ID is required.");
            }
            var row = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row is null) {
                return AppResultModel<AppSettingModel>.Fail("Setting not found.");
            }
            return AppResultModel<AppSettingModel>.Ok(ToModel(row));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingAsync failed");
            return AppResultModel<AppSettingModel>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<string>> CreateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null)
            {
                return AppResultModel<string>.Fail("Setting data is required.");
            }
            model.Code = model.Code?.Trim();
            model.Name = model.Name?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                errors.Add("Setting code is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Code == model.Code && true))
            {
                errors.Add("Setting code already exists.");
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add("Setting name is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Name == model.Name && true))
            {
                errors.Add("Setting name already exists.");
            }
            if (model.Value is null)
            {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<string>.Fail(errors.ToArray());
            }
            var entity = new AppSetting { Id = AppIdGenerator.GenerateId(), Code = model.Code, Name = model.Name, Value = model.Value, Order = model.Order };
            _db.AppSettings.Add(entity);
            await _db.SaveChangesAsync();
            return AppResultModel<string>.Ok(entity.Id, "Setting created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSettingAsync failed");
            return AppResultModel<string>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null)
            {
                return AppResultModel<bool>.Fail("Setting data is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return AppResultModel<bool>.Fail("Setting ID is required.");
            }
            model.Code = model.Code?.Trim();
            model.Name = model.Name?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                errors.Add("Setting code is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
            {
                errors.Add("Setting code already exists.");
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                errors.Add("Setting name is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Name == model.Name && x.Id != model.Id))
            {
                errors.Add("Setting name already exists.");
            }
            if (model.Value is null)
            {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var entity = await _db.AppSettings.FindAsync(model.Id);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("Setting not found.");
            }
            entity.Code = model.Code;
            entity.Name = model.Name;
            entity.Value = model.Value;
            entity.Order = model.Order;
            await _db.SaveChangesAsync();
            return AppResultModel<bool>.Ok(true, "Setting updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync failed");
            return AppResultModel<bool>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> DeleteSettingAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResultModel<bool>.Fail("Setting ID is required.");
            }
            var row = await _db.AppSettings.FindAsync(id);
            if (row is null) {
                return AppResultModel<bool>.Fail("Setting not found.");
            }
            _db.AppSettings.Remove(row);
            await _db.SaveChangesAsync();
            return AppResultModel<bool>.Ok(true, "Setting deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync failed");
            return AppResultModel<bool>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<List<AppUserModel>>> GetUsersAsync()
    {
        try
        {
            var rows = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username).ToListAsync();
            return AppResultModel<List<AppUserModel>>.Ok(rows.Select(ToModel).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync failed");
            return AppResultModel<List<AppUserModel>>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<AppUserModel>> GetUserAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResultModel<AppUserModel>.Fail("User ID is required.");
            }
            var row = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row is null) {
                return AppResultModel<AppUserModel>.Fail("User not found.");
            }
            return AppResultModel<AppUserModel>.Ok(ToModel(row));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync failed");
            return AppResultModel<AppUserModel>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<string>> CreateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null)
            {
                return AppResultModel<string>.Fail("User data is required.");
            }
            model.Code = model.Code?.Trim();
            model.Username = model.Username?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                errors.Add("User code is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Code == model.Code ))
            {
                errors.Add("User code already exists.");
            }
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                errors.Add("Username is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username ))
            {
                errors.Add("Username already exists.");
            }
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                errors.Add("Password is required.");
            }
            if (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email))
            {
                errors.Add("Email is invalid.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<string>.Fail(errors.ToArray());
            }
            var entity = new AppUser { Id = AppIdGenerator.GenerateId(), Code = model.Code, Username = model.Username, Password = model.Password, Email = model.Email, PhoneNumber = model.PhoneNumber, Status = model.Status, Role = model.Role, DateCreated = DateTime.UtcNow, DateModified = DateTime.UtcNow };
            _db.AppUsers.Add(entity);
            await _db.SaveChangesAsync();
            return AppResultModel<string>.Ok(entity.Id, "User created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync failed");
            return AppResultModel<string>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null)
            {
                return AppResultModel<bool>.Fail("User data is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return AppResultModel<bool>.Fail("User ID is required.");
            }
            model.Code = model.Code?.Trim();
            model.Username = model.Username?.Trim() ?? string.Empty;
            model.Email = model.Email?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                errors.Add("User code is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Code == model.Code && x.Id != model.Id))
            {
                errors.Add("User code already exists.");
            }
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                errors.Add("Username is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username && x.Id != model.Id))
            {
                errors.Add("Username already exists.");
            }
            
            if (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email))
            {
                errors.Add("Email is invalid.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var entity = await _db.AppUsers.FindAsync(model.Id);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("User not found.");
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
            return AppResultModel<bool>.Ok(true, "User updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync failed");
            return AppResultModel<bool>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> DeleteUserAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResultModel<bool>.Fail("User ID is required.");
            }
            var row = await _db.AppUsers.FindAsync(id);
            if (row is null) {
                return AppResultModel<bool>.Fail("User not found.");
            }
            _db.AppUsers.Remove(row);
            await _db.SaveChangesAsync();
            return AppResultModel<bool>.Ok(true, "User deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUserAsync failed");
            return AppResultModel<bool>.Fail("An error occurred while processing the request.", ex.Message);
        }
    }
}
