using Application.Common;
using Application.Data;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public sealed class AppCoreService : IAppCoreService
{
    private readonly AppDbContext _db;

    public AppCoreService(AppDbContext db)
    {
        _db = db;
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

    public async Task<List<AppSettingModel>> GetSettingsAsync() =>
        (await _db.AppSettings.AsNoTracking().OrderBy(x => x.Order)
            .ThenBy(x => x.Name).ToListAsync()).Select(ToModel).ToList();

    public async Task<AppSettingModel?> GetSettingAsync(string id)
    {
        var entity = await _db.AppSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<AppSettingModel> CreateSettingAsync(AppSettingModel model)
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

    public async Task<bool> UpdateSettingAsync(AppSettingModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var entity = await _db.AppSettings.FindAsync(model.Id);
        if (entity is null) return false;
        entity.Code = model.Code;
        entity.Name = model.Name;
        entity.Value = model.Value;
        entity.Order = model.Order;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSettingAsync(string id)
    {
        var entity = await _db.AppSettings.FindAsync(id);
        if (entity is null) return false;
        _db.AppSettings.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<AppUserModel>> GetUsersAsync() =>
        (await _db.AppUsers.AsNoTracking().OrderBy(x => x.Username)
            .ToListAsync()).Select(ToModel).ToList();

    public async Task<AppUserModel?> GetUserAsync(string id)
    {
        var entity = await _db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<AppUserModel> CreateUserAsync(AppUserModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        if (string.IsNullOrWhiteSpace(model.Password))
            throw new ArgumentException("Password is required when creating a user.", nameof(model));

        var entity = new AppUser
        {
            Id = AppIdGenerator.GenerateId(),
            Code = model.Code,
            Username = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Status = model.Status,
            Role = model.Role,
            DateCreated = DateTime.UtcNow,
            DateModified = DateTime.UtcNow
        };
        entity.Password = model.Password;
        _db.AppUsers.Add(entity);
        await _db.SaveChangesAsync();
        return ToModel(entity);
    }

    public async Task<bool> UpdateUserAsync(AppUserModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var entity = await _db.AppUsers.FindAsync(model.Id);
        if (entity is null) return false;

        entity.Code = model.Code;
        entity.Username = model.Username;
        entity.Email = model.Email;
        entity.PhoneNumber = model.PhoneNumber;
        entity.Status = model.Status;
        entity.Role = model.Role;
        if (!string.IsNullOrWhiteSpace(model.Password))
            entity.Password = model.Password;
        entity.DateModified = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var entity = await _db.AppUsers.FindAsync(id);
        if (entity is null) return false;
        _db.AppUsers.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}
