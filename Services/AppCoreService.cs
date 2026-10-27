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
        Email = entity.Email,
        PhoneNumber = entity.PhoneNumber,
        Status = entity.Status,
        Role = entity.Role,
        DateCreated = entity.DateCreated,
        DateModified = entity.DateModified
    };

    public async Task<AppResultModel<List<AppSettingModel>>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _db.AppSettings.AsNoTracking().OrderBy(x => x.Order).ThenBy(x => x.Name).ToListAsync(cancellationToken);
            return AppResultModel<List<AppSettingModel>>.Ok(entities.Select(ToModel).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetSettingsAsync));
            return AppResultModel<List<AppSettingModel>>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<AppSettingModel>> GetSettingAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return AppResultModel<AppSettingModel>.Fail("Setting ID is required.");
            }
            var entity = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<AppSettingModel>.Fail("Setting not found.");
            }
            return AppResultModel<AppSettingModel>.Ok(ToModel(entity));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetSettingAsync));
            return AppResultModel<AppSettingModel>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<string>> CreateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
            {
                return AppResultModel<string>.Fail("Setting data is required.");
            }
            input.Code = input.Code?.Trim();
            input.Name = input.Name?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input.Code))
            {
                errors.Add("Setting code is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Code == input.Code, cancellationToken))
            {
                errors.Add("Setting code already exists.");
            }
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                errors.Add("Setting name is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Name == input.Name, cancellationToken))
            {
                errors.Add("Setting name already exists.");
            }
            if (input.Value is null)
            {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<string>.Fail(errors.ToArray());
            }
            var entity = new AppSetting { Id = AppIdGenerator.GenerateId(), Code = input.Code, Name = input.Name, Value = input.Value, Order = input.Order };
            _db.AppSettings.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<string>.Ok(entity.Id, "Setting created successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(CreateSettingAsync));
            return AppResultModel<string>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> UpdateSettingAsync(AppSettingModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
            {
                return AppResultModel<bool>.Fail("Setting data is required.");
            }
            if (string.IsNullOrWhiteSpace(input.Id))
            {
                return AppResultModel<bool>.Fail("Setting ID is required.");
            }
            input.Code = input.Code?.Trim();
            input.Name = input.Name?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input.Code))
            {
                errors.Add("Setting code is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Code == input.Code && x.Id != input.Id, cancellationToken))
            {
                errors.Add("Setting code already exists.");
            }
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                errors.Add("Setting name is required.");
            }
            else if (await _db.AppSettings.AnyAsync(x => x.Name == input.Name && x.Id != input.Id, cancellationToken))
            {
                errors.Add("Setting name already exists.");
            }
            if (input.Value is null)
            {
                errors.Add("Setting value is required.");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var entity = await _db.AppSettings.FindAsync(new object[] { input.Id }, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("Setting not found.");
            }
            entity.Code = input.Code;
            entity.Name = input.Name;
            entity.Value = input.Value;
            entity.Order = input.Order;
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "Setting updated successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(UpdateSettingAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> DeleteSettingAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return AppResultModel<bool>.Fail("Setting ID is required.");
            }
            var entity = await _db.AppSettings.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("Setting not found.");
            }
            _db.AppSettings.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "Setting deleted successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(DeleteSettingAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<List<AppUserModel>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username).ToListAsync(cancellationToken);
            return AppResultModel<List<AppUserModel>>.Ok(entities.Select(ToModel).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetUsersAsync));
            return AppResultModel<List<AppUserModel>>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<AppUserModel>> GetUserAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return AppResultModel<AppUserModel>.Fail("User ID is required.");
            }
            var entity = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<AppUserModel>.Fail("User not found.");
            }
            return AppResultModel<AppUserModel>.Ok(ToModel(entity));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetUserAsync));
            return AppResultModel<AppUserModel>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<string>> CreateUserAsync(AppUserModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
            {
                return AppResultModel<string>.Fail("User data is required.");
            }
            input.Code = input.Code?.Trim();
            input.Username = input.Username?.Trim() ?? string.Empty;
            input.Email = input.Email?.Trim();
            input.PhoneNumber = input.PhoneNumber?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input.Code))
            {
                errors.Add("User code is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Code == input.Code, cancellationToken))
            {
                errors.Add("User code already exists.");
            }
            if (string.IsNullOrWhiteSpace(input.Username))
            {
                errors.Add("Username is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Username == input.Username, cancellationToken))
            {
                errors.Add("Username already exists.");
            }
            if (string.IsNullOrWhiteSpace(input.Password))
            {
                errors.Add("Password is required.");
            }
            else if (input.Password.Length < 6)
            {
                errors.Add("Password must be at least 6 characters long.");
            }
            if (!string.IsNullOrWhiteSpace(input.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(input.Email))
            {
                errors.Add("Email is invalid.");
            }
            if (!string.IsNullOrWhiteSpace(input.PhoneNumber) && !new System.ComponentModel.DataAnnotations.PhoneAttribute().IsValid(input.PhoneNumber))
            {
                errors.Add("Phone number is invalid.");
            }
            if (input.Status is not (0 or 1))
            {
                errors.Add("Status must be 0 (Inactive) or 1 (Active).");
            }
            if (input.Role is not (0 or 1 or 2))
            {
                errors.Add("Role must be 0 (User), 1 (Administrator), or 2 (Manager).");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<string>.Fail(errors.ToArray());
            }
            var entity = new AppUser { Id = AppIdGenerator.GenerateId(), Code = input.Code, Username = input.Username, Password = input.Password, Email = input.Email, PhoneNumber = input.PhoneNumber, Status = input.Status, Role = input.Role, DateCreated = DateTime.UtcNow, DateModified = DateTime.UtcNow };
            _db.AppUsers.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<string>.Ok(entity.Id, "User created successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(CreateUserAsync));
            return AppResultModel<string>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> UpdateUserAsync(AppUserModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
            {
                return AppResultModel<bool>.Fail("User data is required.");
            }
            if (string.IsNullOrWhiteSpace(input.Id))
            {
                return AppResultModel<bool>.Fail("User ID is required.");
            }
            input.Code = input.Code?.Trim();
            input.Username = input.Username?.Trim() ?? string.Empty;
            input.Email = input.Email?.Trim();
            input.PhoneNumber = input.PhoneNumber?.Trim();
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(input.Code))
            {
                errors.Add("User code is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Code == input.Code && x.Id != input.Id, cancellationToken))
            {
                errors.Add("User code already exists.");
            }
            if (string.IsNullOrWhiteSpace(input.Username))
            {
                errors.Add("Username is required.");
            }
            else if (await _db.AppUsers.AnyAsync(x => x.Username == input.Username && x.Id != input.Id, cancellationToken))
            {
                errors.Add("Username already exists.");
            }
            if (!string.IsNullOrWhiteSpace(input.Password) && input.Password.Length < 6)
            {
                errors.Add("Password must be at least 6 characters long.");
            }
            if (!string.IsNullOrWhiteSpace(input.Email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(input.Email))
            {
                errors.Add("Email is invalid.");
            }
            if (!string.IsNullOrWhiteSpace(input.PhoneNumber) && !new System.ComponentModel.DataAnnotations.PhoneAttribute().IsValid(input.PhoneNumber))
            {
                errors.Add("Phone number is invalid.");
            }
            if (input.Status is not (0 or 1))
            {
                errors.Add("Status must be 0 (Inactive) or 1 (Active).");
            }
            if (input.Role is not (0 or 1 or 2))
            {
                errors.Add("Role must be 0 (User), 1 (Administrator), or 2 (Manager).");
            }
            if (errors.Count > 0)
            {
                return AppResultModel<bool>.Fail(errors.ToArray());
            }
            var entity = await _db.AppUsers.FindAsync(new object[] { input.Id }, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("User not found.");
            }
            if (string.Equals(entity.Username, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(input.Username, "administrator", StringComparison.Ordinal))
                {
                    errors.Add("The administrator username cannot be changed.");
                }
                if (input.Role != 1)
                {
                    errors.Add("The administrator role must remain Administrator (1).");
                }
                if (input.Status != 1)
                {
                    errors.Add("The administrator status must remain Active (1).");
                }
                if (errors.Count > 0)
                {
                    return AppResultModel<bool>.Fail(errors.ToArray());
                }
            }
            entity.Code = input.Code;
            entity.Username = input.Username;
            entity.Email = input.Email;
            entity.PhoneNumber = input.PhoneNumber;
            entity.Status = input.Status;
            entity.Role = input.Role;
            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                entity.Password = input.Password;
            }
            entity.DateModified = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "User updated successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(UpdateUserAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> DeleteUserAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return AppResultModel<bool>.Fail("User ID is required.");
            }
            var entity = await _db.AppUsers.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
            {
                return AppResultModel<bool>.Fail("User not found.");
            }
            if (string.Equals(entity.Username, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                return AppResultModel<bool>.Fail("The administrator account cannot be deleted.");
            }
            _db.AppUsers.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "User deleted successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(DeleteUserAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }
}
