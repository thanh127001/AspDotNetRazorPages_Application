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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetSettingsAsync database error");
            return AppResultModel<List<AppSettingModel>>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingsAsync unexpected error");
            return AppResultModel<List<AppSettingModel>>.Fail("An unexpected error occurred.", ex.Message);
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetSettingAsync database error");
            return AppResultModel<AppSettingModel>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingAsync unexpected error");
            return AppResultModel<AppSettingModel>.Fail("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<AppResultModel<AppSettingModel>> CreateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null) {
                return AppResultModel<AppSettingModel>.Fail("Setting data is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Name)) {
                errors.Add("Setting name is required.");
            }
            if (model.Value is null) {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0) {
                return AppResultModel<AppSettingModel>.Fail(errors.ToArray());
            }
            if (model.Code is not null && await _db.AppSettings.AnyAsync(x => x.Code == model.Code)) {
                return AppResultModel<AppSettingModel>.Fail("Setting code already exists.");
            }
            var row = new AppSetting { Id = AppIdGenerator.GenerateId(), Code = model.Code, Name = model.Name, Value = model.Value, Order = model.Order };
            _db.AppSettings.Add(row);
            await _db.SaveChangesAsync();
            return AppResultModel<AppSettingModel>.Ok(ToModel(row), "Setting created successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "CreateSettingAsync database error");
            return AppResultModel<AppSettingModel>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSettingAsync unexpected error");
            return AppResultModel<AppSettingModel>.Fail("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Id)) {
                return AppResultModel<bool>.Fail("Setting ID is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Name)) {
                errors.Add("Setting name is required.");
            }
            if (model.Value is null) {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0) {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var row = await _db.AppSettings.FindAsync(model.Id);
            if (row is null) {
                return AppResultModel<bool>.Fail("Setting not found.");
            }
            if (model.Code is not null && await _db.AppSettings.AnyAsync(x => x.Code == model.Code && x.Id != model.Id)) {
                return AppResultModel<bool>.Fail("Setting code already exists.");
            }
            row.Code = model.Code; row.Name = model.Name; row.Value = model.Value; row.Order = model.Order;
            await _db.SaveChangesAsync();
            return AppResultModel<bool>.Ok(true, "Setting updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync database error");
            return AppResultModel<bool>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync unexpected error");
            return AppResultModel<bool>.Fail("An unexpected error occurred.", ex.Message);
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync database error");
            return AppResultModel<bool>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync unexpected error");
            return AppResultModel<bool>.Fail("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<AppResultModel<List<AppUserModel>>> GetUsersAsync()
    {
        try
        {
            var rows = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username).ToListAsync();
            return AppResultModel<List<AppUserModel>>.Ok(rows.Select(ToModel).ToList());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetUsersAsync database error");
            return AppResultModel<List<AppUserModel>>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync unexpected error");
            return AppResultModel<List<AppUserModel>>.Fail("An unexpected error occurred.", ex.Message);
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetUserAsync database error");
            return AppResultModel<AppUserModel>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync unexpected error");
            return AppResultModel<AppUserModel>.Fail("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<AppResultModel<AppUserModel>> CreateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null) {
                return AppResultModel<AppUserModel>.Fail("User data is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Username)) {
                errors.Add("Username is required.");
            }
            if (string.IsNullOrWhiteSpace(model.Password)) {
                errors.Add("Password is required.");
            }
            if (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email)) {
                errors.Add("Email is invalid.");
            }
            if (errors.Count > 0) {
                return AppResultModel<AppUserModel>.Fail(errors.ToArray());
            }
            if (model.Code is not null && await _db.AppUsers.AnyAsync(x => x.Code == model.Code)) {
                return AppResultModel<AppUserModel>.Fail("User code already exists.");
            }
            if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username)) {
                return AppResultModel<AppUserModel>.Fail("Username already exists.");
            }
            var row = new AppUser { Id = AppIdGenerator.GenerateId(), Code = model.Code, Username = model.Username, Password = model.Password, Email = model.Email, PhoneNumber = model.PhoneNumber, Status = model.Status, Role = model.Role, DateCreated = DateTime.UtcNow, DateModified = DateTime.UtcNow };
            _db.AppUsers.Add(row);
            await _db.SaveChangesAsync();
            return AppResultModel<AppUserModel>.Ok(ToModel(row), "User created successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "CreateUserAsync database error");
            return AppResultModel<AppUserModel>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync unexpected error");
            return AppResultModel<AppUserModel>.Fail("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Id)) {
                return AppResultModel<bool>.Fail("User ID is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Username)) {
                errors.Add("Username is required.");
            }
            if (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email)) {
                errors.Add("Email is invalid.");
            }
            if (errors.Count > 0) {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var row = await _db.AppUsers.FindAsync(model.Id);
            if (row is null) {
                return AppResultModel<bool>.Fail("User not found.");
            }
            if (model.Code is not null && await _db.AppUsers.AnyAsync(x => x.Code == model.Code && x.Id != model.Id)) {
                return AppResultModel<bool>.Fail("User code already exists.");
            }
            if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username && x.Id != model.Id)) {
                return AppResultModel<bool>.Fail("Username already exists.");
            }
            row.Code = model.Code; row.Username = model.Username; row.Email = model.Email; row.PhoneNumber = model.PhoneNumber; row.Status = model.Status; row.Role = model.Role;
            if (!string.IsNullOrWhiteSpace(model.Password)) {
                row.Password = model.Password;
            }
            row.DateModified = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return AppResultModel<bool>.Ok(true, "User updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "UpdateUserAsync database error");
            return AppResultModel<bool>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync unexpected error");
            return AppResultModel<bool>.Fail("An unexpected error occurred.", ex.Message);
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
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DeleteUserAsync database error");
            return AppResultModel<bool>.Fail("Unable to save or retrieve data. Please try again.", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUserAsync unexpected error");
            return AppResultModel<bool>.Fail("An unexpected error occurred.", ex.Message);
        }
    }
}
