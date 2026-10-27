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

    public async Task<AppResult<List<AppSettingModel>>> GetSettingsAsync()
    {
        try
        {
            var rows = await _db.AppSettings.AsNoTracking().OrderBy(x => x.Order).ThenBy(x => x.Name).ToListAsync();
            return AppResult<List<AppSettingModel>>.Ok(rows.Select(ToModel).ToList());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetSettingsAsync database error");
            return AppResult<List<AppSettingModel>>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingsAsync unexpected error");
            return AppResult<List<AppSettingModel>>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<AppSettingModel>> GetSettingAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResult<AppSettingModel>.Fail(AppResultCode.ValidationError, "Setting ID is required.");
            }
            var row = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row is null) {
                return AppResult<AppSettingModel>.Fail(AppResultCode.NotFound, "Setting not found.");
            }
            return AppResult<AppSettingModel>.Ok(ToModel(row));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetSettingAsync database error");
            return AppResult<AppSettingModel>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSettingAsync unexpected error");
            return AppResult<AppSettingModel>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<AppSettingModel>> CreateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null) {
                return AppResult<AppSettingModel>.Fail(AppResultCode.ValidationError, "Setting data is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Name)) {
                errors.Add("Setting name is required.");
            }
            if (model.Value is null) {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0) {
                return AppResult<AppSettingModel>.Fail(AppResultCode.ValidationError, errors.ToArray());
            }
            if (model.Code is not null && await _db.AppSettings.AnyAsync(x => x.Code == model.Code)) {
                return AppResult<AppSettingModel>.Fail(AppResultCode.Duplicate, "Setting code already exists.");
            }
            var row = new AppSetting { Id = AppIdGenerator.GenerateId(), Code = model.Code, Name = model.Name, Value = model.Value, Order = model.Order };
            _db.AppSettings.Add(row);
            await _db.SaveChangesAsync();
            return AppResult<AppSettingModel>.Ok(ToModel(row), "Setting created successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "CreateSettingAsync database error");
            return AppResult<AppSettingModel>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSettingAsync unexpected error");
            return AppResult<AppSettingModel>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<bool>> UpdateSettingAsync(AppSettingModel model)
    {
        try
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Id)) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, "Setting ID is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Name)) {
                errors.Add("Setting name is required.");
            }
            if (model.Value is null) {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, errors.ToArray());
            }
            var row = await _db.AppSettings.FindAsync(model.Id);
            if (row is null) {
                return AppResult<bool>.Fail(AppResultCode.NotFound, "Setting not found.");
            }
            if (model.Code is not null && await _db.AppSettings.AnyAsync(x => x.Code == model.Code && x.Id != model.Id)) {
                return AppResult<bool>.Fail(AppResultCode.Duplicate, "Setting code already exists.");
            }
            row.Code = model.Code; row.Name = model.Name; row.Value = model.Value; row.Order = model.Order;
            await _db.SaveChangesAsync();
            return AppResult<bool>.Ok(true, "Setting updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync database error");
            return AppResult<bool>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateSettingAsync unexpected error");
            return AppResult<bool>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<bool>> DeleteSettingAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, "Setting ID is required.");
            }
            var row = await _db.AppSettings.FindAsync(id);
            if (row is null) {
                return AppResult<bool>.Fail(AppResultCode.NotFound, "Setting not found.");
            }
            _db.AppSettings.Remove(row);
            await _db.SaveChangesAsync();
            return AppResult<bool>.Ok(true, "Setting deleted successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync database error");
            return AppResult<bool>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteSettingAsync unexpected error");
            return AppResult<bool>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<List<AppUserModel>>> GetUsersAsync()
    {
        try
        {
            var rows = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username).ToListAsync();
            return AppResult<List<AppUserModel>>.Ok(rows.Select(ToModel).ToList());
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetUsersAsync database error");
            return AppResult<List<AppUserModel>>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync unexpected error");
            return AppResult<List<AppUserModel>>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<AppUserModel>> GetUserAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResult<AppUserModel>.Fail(AppResultCode.ValidationError, "User ID is required.");
            }
            var row = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (row is null) {
                return AppResult<AppUserModel>.Fail(AppResultCode.NotFound, "User not found.");
            }
            return AppResult<AppUserModel>.Ok(ToModel(row));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "GetUserAsync database error");
            return AppResult<AppUserModel>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserAsync unexpected error");
            return AppResult<AppUserModel>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<AppUserModel>> CreateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null) {
                return AppResult<AppUserModel>.Fail(AppResultCode.ValidationError, "User data is required.");
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
                return AppResult<AppUserModel>.Fail(AppResultCode.ValidationError, errors.ToArray());
            }
            if (model.Code is not null && await _db.AppUsers.AnyAsync(x => x.Code == model.Code)) {
                return AppResult<AppUserModel>.Fail(AppResultCode.Duplicate, "User code already exists.");
            }
            if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username)) {
                return AppResult<AppUserModel>.Fail(AppResultCode.Duplicate, "Username already exists.");
            }
            var row = new AppUser { Id = AppIdGenerator.GenerateId(), Code = model.Code, Username = model.Username, Password = model.Password, Email = model.Email, PhoneNumber = model.PhoneNumber, Status = model.Status, Role = model.Role, DateCreated = DateTime.UtcNow, DateModified = DateTime.UtcNow };
            _db.AppUsers.Add(row);
            await _db.SaveChangesAsync();
            return AppResult<AppUserModel>.Ok(ToModel(row), "User created successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "CreateUserAsync database error");
            return AppResult<AppUserModel>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync unexpected error");
            return AppResult<AppUserModel>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<bool>> UpdateUserAsync(AppUserModel model)
    {
        try
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Id)) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, "User ID is required.");
            }
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(model.Username)) {
                errors.Add("Username is required.");
            }
            if (!string.IsNullOrWhiteSpace(model.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email)) {
                errors.Add("Email is invalid.");
            }
            if (errors.Count > 0) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, errors.ToArray());
            }
            var row = await _db.AppUsers.FindAsync(model.Id);
            if (row is null) {
                return AppResult<bool>.Fail(AppResultCode.NotFound, "User not found.");
            }
            if (model.Code is not null && await _db.AppUsers.AnyAsync(x => x.Code == model.Code && x.Id != model.Id)) {
                return AppResult<bool>.Fail(AppResultCode.Duplicate, "User code already exists.");
            }
            if (await _db.AppUsers.AnyAsync(x => x.Username == model.Username && x.Id != model.Id)) {
                return AppResult<bool>.Fail(AppResultCode.Duplicate, "Username already exists.");
            }
            row.Code = model.Code; row.Username = model.Username; row.Email = model.Email; row.PhoneNumber = model.PhoneNumber; row.Status = model.Status; row.Role = model.Role;
            if (!string.IsNullOrWhiteSpace(model.Password)) {
                row.Password = model.Password;
            }
            row.DateModified = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return AppResult<bool>.Ok(true, "User updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "UpdateUserAsync database error");
            return AppResult<bool>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync unexpected error");
            return AppResult<bool>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }

    public async Task<AppResult<bool>> DeleteUserAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) {
                return AppResult<bool>.Fail(AppResultCode.ValidationError, "User ID is required.");
            }
            var row = await _db.AppUsers.FindAsync(id);
            if (row is null) {
                return AppResult<bool>.Fail(AppResultCode.NotFound, "User not found.");
            }
            _db.AppUsers.Remove(row);
            await _db.SaveChangesAsync();
            return AppResult<bool>.Ok(true, "User deleted successfully.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DeleteUserAsync database error");
            return AppResult<bool>.Fail(AppResultCode.DatabaseError, "Unable to save or retrieve data. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteUserAsync unexpected error");
            return AppResult<bool>.Fail(AppResultCode.UnexpectedError, "An unexpected error occurred.");
        }
    }
}
