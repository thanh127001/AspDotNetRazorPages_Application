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

    private const int MaxFileSize = 1024 * 1024;

    private static bool IsSafeFileName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length <= 255 &&
        name == Path.GetFileName(name) && name is not ("." or "..") &&
        name.IndexOfAny(new[] { '/', '\\', ':' }) < 0;


    private static bool TryGetFileType(string? fileName, byte[]? content, out string fileType)
    {
        fileType = string.Empty;
        if (string.IsNullOrWhiteSpace(fileName) || content is null ||
            content.Length == 0 || content.Length > MaxFileSize)
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        ReadOnlySpan<byte> data = content;
        switch (extension)
        {
            case ".png" when data.Length >= 8 &&
                data[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }):
                fileType = "image/png";
                return true;
            case ".jpg" or ".jpeg" when data.Length >= 4 &&
                data[0] == 0xFF && data[1] == 0xD8 &&
                data[^2] == 0xFF && data[^1] == 0xD9:
                fileType = "image/jpeg";
                return true;
            case ".pdf" when data.Length >= 5 &&
                data[..5].SequenceEqual("%PDF-"u8):
                fileType = "application/pdf";
                return true;
            case ".txt":
                try
                {
                    var value = new System.Text.UTF8Encoding(false, true).GetString(content);
                    if (value.Contains('\0')) return false;
                    fileType = "text/plain";
                    return true;
                }
                catch (System.Text.DecoderFallbackException) { return false; }
        }
        return false;
    }

    public async Task<AppResultModel<List<AppFileModel>>> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var files = await _db.AppFiles.AsNoTracking()
                .OrderBy(x => x.Order).ThenBy(x => x.FileName)
                .Select(x => new AppFileModel { Id = x.Id, FileName = x.FileName, FileType = x.FileType, FileSize = x.FileSize, Order = x.Order })
                .ToListAsync(cancellationToken);
            return AppResultModel<List<AppFileModel>>.Ok(files);
        }
        catch (Exception ex)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetFilesAsync));
            return AppResultModel<List<AppFileModel>>.Fail($"Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<AppFileModel>> GetFileAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return AppResultModel<AppFileModel>.Fail("File ID is required.");
        try
        {
            var file = await _db.AppFiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (file is null) return AppResultModel<AppFileModel>.Fail("File not found.");
            return AppResultModel<AppFileModel>.Ok(new AppFileModel
            {
                Id = file.Id, FileName = file.FileName, FileType = file.FileType,
                FileSize = file.FileSize, FileContent = file.FileContent, Order = file.Order
            });
        }
        catch (Exception ex)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetFileAsync));
            return AppResultModel<AppFileModel>.Fail($"Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<string>> CreateFileAsync(AppFileModel input, CancellationToken cancellationToken = default)
    {
        if (input is null || !TryGetFileType(input.FileName, input.FileContent, out var type))
            return AppResultModel<string>.Fail("Only valid PNG, JPG, TXT and PDF files up to 1 MB are allowed.");
        try
        {
            var name = Path.GetFileName(input.FileName);
            if (!IsSafeFileName(name) || await _db.AppFiles.AnyAsync(x => x.FileName == name, cancellationToken))
                return AppResultModel<string>.Fail("Invalid or duplicate file name.");
            var file = new AppFile
            {
                Id = AppIdGenerator.GenerateId(),
                FileName = name,
                FileType = type,
                FileSize = input.FileContent.Length,
                FileContent = input.FileContent,
                Order = input.Order
            };
            _db.AppFiles.Add(file);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<string>.Ok(file.Id, "File created successfully.");
        }
        catch (Exception ex)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(CreateFileAsync));
            return AppResultModel<string>.Fail($"Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> UpdateFileAsync(string id, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(fileName))
            return AppResultModel<bool>.Fail("File ID and name are required.");
        try
        {
            var file = await _db.AppFiles.FindAsync(new object[] { id }, cancellationToken);
            if (file is null) return AppResultModel<bool>.Fail("File not found.");
            var name = Path.GetFileName(fileName.Trim());
            if (name.Length == 0 || name.Length > 255 ||
                !string.Equals(Path.GetExtension(name), Path.GetExtension(file.FileName), StringComparison.OrdinalIgnoreCase))
                return AppResultModel<bool>.Fail("Only the file name may change; the extension must remain the same.");
            if (!IsSafeFileName(name) || await _db.AppFiles.AnyAsync(x => x.Id != id && x.FileName == name, cancellationToken))
                return AppResultModel<bool>.Fail("Invalid or duplicate file name.");
            file.FileName = name;
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "File renamed successfully.");
        }
        catch (Exception ex)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(UpdateFileAsync));
            return AppResultModel<bool>.Fail($"Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> DeleteFileAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return AppResultModel<bool>.Fail("File ID is required.");
        try
        {
            var file = await _db.AppFiles.FindAsync(new object[] { id }, cancellationToken);
            if (file is null) return AppResultModel<bool>.Fail("File not found.");
            _db.AppFiles.Remove(file);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "File deleted successfully.");
        }
        catch (Exception ex)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(DeleteFileAsync));
            return AppResultModel<bool>.Fail($"Error ID: {errorId}");
        }
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
        Username = entity.Username,
        Status = entity.Status,
        Role = entity.Role,
        CreatedTime = entity.CreatedTime,
        UpdatedTime = entity.UpdatedTime
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
            input.Name = input.Name?.Trim();
            var errors = new List<string>();
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
            input.Username = input.Username?.Trim() ?? string.Empty;
            var errors = new List<string>();
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
            var entity = new AppUser { Id = AppIdGenerator.GenerateId(), Username = input.Username, Password = input.Password, Status = input.Status, Role = input.Role, CreatedTime = DateTime.UtcNow };
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
            input.Username = input.Username?.Trim() ?? string.Empty;
            var errors = new List<string>();
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
            entity.Username = input.Username;
            entity.Status = input.Status;
            entity.Role = input.Role;
            entity.UpdatedTime = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                entity.Password = input.Password;
            }
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
    private static AppContentModel ToModel(AppContent entity) => new()
    {
        Id = entity.Id,
        Code = entity.Code,
        Title = entity.Title,
        Content = entity.Content,
        Icon = entity.Icon,
        Order = entity.Order,
        Status = entity.Status,
        CreatedTime = entity.CreatedTime,
        UpdatedTime = entity.UpdatedTime
    };

    public async Task<AppResultModel<List<AppContentModel>>> GetContentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _db.AppContents.AsNoTracking()
                .OrderBy(x => x.Order).ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);
            return AppResultModel<List<AppContentModel>>.Ok(entities.Select(ToModel).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetContentsAsync));
            return AppResultModel<List<AppContentModel>>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<AppContentModel>> GetContentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return AppResultModel<AppContentModel>.Fail("Content ID is required.");
            var entity = await _db.AppContents.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return entity is null
                ? AppResultModel<AppContentModel>.Fail("Content not found.")
                : AppResultModel<AppContentModel>.Ok(ToModel(entity));
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(GetContentAsync));
            return AppResultModel<AppContentModel>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<string>> CreateContentAsync(AppContentModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
                return AppResultModel<string>.Fail("Content data is required.");
            input.Code = string.IsNullOrWhiteSpace(input.Code) ? null : input.Code.Trim();
            input.Title = input.Title?.Trim() ?? string.Empty;
            var errors = new List<string>();
            if (input.Code is not null && (await _db.AppContents.AnyAsync(x => x.Code == input.Code, cancellationToken))
                errors.Add("Content code already exists.");
            if (string.IsNullOrWhiteSpace(input.Title))
                errors.Add("Content title is required.");
            if (input.Content is null)
                errors.Add("Content body is required.");
            if (input.Status is not (0 or 1))
                errors.Add("Status must be 0 (Inactive) or 1 (Active).");
            if (errors.Count > 0)
                return AppResultModel<string>.Fail(errors.ToArray());

            var entity = new AppContent
            {
                Id = AppIdGenerator.GenerateId(),
                Code = input.Code,
                Title = input.Title,
                Content = input.Content,
                Icon = input.Icon,
                Order = input.Order,
                Status = input.Status,
                CreatedTime = DateTime.UtcNow
            };
            _db.AppContents.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<string>.Ok(entity.Id, "Content created successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(CreateContentAsync));
            return AppResultModel<string>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> UpdateContentAsync(AppContentModel input, CancellationToken cancellationToken = default)
    {
        try
        {
            if (input is null)
                return AppResultModel<bool>.Fail("Content data is required.");
            if (string.IsNullOrWhiteSpace(input.Id))
                return AppResultModel<bool>.Fail("Content ID is required.");
            input.Code = string.IsNullOrWhiteSpace(input.Code) ? null : input.Code.Trim();
            input.Title = input.Title?.Trim() ?? string.Empty;
            var errors = new List<string>();
            if (input.Code is not null && (await _db.AppContents.AnyAsync(x => x.Code == input.Code && x.Id != input.Id, cancellationToken))
                errors.Add("Content code already exists.");
            if (string.IsNullOrWhiteSpace(input.Title))
                errors.Add("Content title is required.");
            if (input.Content is null)
                errors.Add("Content body is required.");
            if (input.Status is not (0 or 1))
                errors.Add("Status must be 0 (Inactive) or 1 (Active).");
            if (errors.Count > 0)
                return AppResultModel<bool>.Fail(errors.ToArray());

            var entity = await _db.AppContents.FindAsync(new object[] { input.Id }, cancellationToken);
            if (entity is null)
                return AppResultModel<bool>.Fail("Content not found.");
            entity.Code = input.Code;
            entity.Title = input.Title;
            entity.Content = input.Content;
            entity.Icon = input.Icon;
            entity.Order = input.Order;
            entity.Status = input.Status;
            entity.UpdatedTime = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "Content updated successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(UpdateContentAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

    public async Task<AppResultModel<bool>> DeleteContentAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return AppResultModel<bool>.Fail("Content ID is required.");
            var entity = await _db.AppContents.FindAsync(new object[] { id }, cancellationToken);
            if (entity is null)
                return AppResultModel<bool>.Fail("Content not found.");
            _db.AppContents.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return AppResultModel<bool>.Ok(true, "Content deleted successfully.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            var errorId = AppErrorLog.Log(_logger, ex, nameof(DeleteContentAsync));
            return AppResultModel<bool>.Fail($"An error occurred while processing the request. Error ID: {errorId}");
        }
    }

}
