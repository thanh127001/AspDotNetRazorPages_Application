using System.Threading;
using Application.Models;

namespace Application.Services;

public class AppInitiationService : IAppInitiationService
{
    private static int _initiated = 0;

    private readonly IAppCoreService _core;
    private readonly ILogger<AppInitiationService> _logger;

    public AppInitiationService(
        IAppCoreService core,
        ILogger<AppInitiationService> logger)
    {
        _core = core;
        _logger = logger;
    }

    #region Initiation

    public async Task<bool> InitiateAsync()
    {
        if (Interlocked.CompareExchange(ref _initiated, 1, 0) != 0)
        {
            return false;
        }

        _logger.LogInformation("Application initiation started.");

        var settingsResult = await InitiateResourcesAsync();
        if (!settingsResult)
        {
            _logger.LogError("Application initiation failed during resource initiation.");
            return false;
        }

        var usersResult = await InitiateUsersAsync();
        if (!usersResult)
        {
            _logger.LogError("Application initiation failed during users initiation.");
            return false;
        }

        _logger.LogInformation("Application initiation completed.");
        return true;
    }

    #endregion

    #region Resources
    public async Task<bool> InitiateResourcesAsync()
    {
        var existingResources = await _core.GetResourcesAsync();
        if (!existingResources.Success || existingResources.Data == null)
        {
            _logger.LogError("Failed to retrieve existing resources.");
            foreach (var error in existingResources.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        var defaults = new List<AppResourceModel>
        {
            // Settings
            new() { Code = "App.Name", Type = "Setting", Culture = "en", Value = "Application" },
            new() { Code = "App.Description", Type = "Setting", Culture = "en", Value = "ASP.NET Core Razor Pages Application" },
            new() { Code = "App.Copyright", Type = "Setting", Culture = "en", Value = "Copyright © 2026" },

            // Titles
            new() { Code = "Title.Home", Type = "Title", Culture = "en", Value = "Home" },
            new() { Code = "Title.Home", Type = "Title", Culture = "vi", Value = "Trang chủ" },
            new() { Code = "Title.Home", Type = "Title", Culture = "zh", Value = "首页" },
            new() { Code = "Title.Information", Type = "Title", Culture = "en", Value = "Information" },
            new() { Code = "Title.Information", Type = "Title", Culture = "vi", Value = "Thông tin" },
            new() { Code = "Title.Information", Type = "Title", Culture = "zh", Value = "信息" },
            new() { Code = "Title.About", Type = "Title", Culture = "en", Value = "About" },
            new() { Code = "Title.About", Type = "Title", Culture = "vi", Value = "Giới thiệu" },
            new() { Code = "Title.About", Type = "Title", Culture = "zh", Value = "关于" },
            new() { Code = "Title.Contact", Type = "Title", Culture = "en", Value = "Contact" },
            new() { Code = "Title.Contact", Type = "Title", Culture = "vi", Value = "Liên hệ" },
            new() { Code = "Title.Contact", Type = "Title", Culture = "zh", Value = "联系" },
            new() { Code = "Title.Privacy", Type = "Title", Culture = "en", Value = "Privacy Policy" },
            new() { Code = "Title.Privacy", Type = "Title", Culture = "vi", Value = "Chính sách bảo mật" },
            new() { Code = "Title.Privacy", Type = "Title", Culture = "zh", Value = "隐私政策" },
            new() { Code = "Title.Terms", Type = "Title", Culture = "en", Value = "Terms of Service" },
            new() { Code = "Title.Terms", Type = "Title", Culture = "vi", Value = "Điều khoản dịch vụ" },
            new() { Code = "Title.Terms", Type = "Title", Culture = "zh", Value = "服务条款" },
            new() { Code = "Title.Account", Type = "Title", Culture = "en", Value = "Account" },
            new() { Code = "Title.Account", Type = "Title", Culture = "vi", Value = "Tài khoản" },
            new() { Code = "Title.Account", Type = "Title", Culture = "zh", Value = "账户" },
            new() { Code = "Title.Login", Type = "Title", Culture = "en", Value = "Login" },
            new() { Code = "Title.Login", Type = "Title", Culture = "vi", Value = "Đăng nhập" },
            new() { Code = "Title.Login", Type = "Title", Culture = "zh", Value = "登录" },
            new() { Code = "Title.Logout", Type = "Title", Culture = "en", Value = "Logout" },
            new() { Code = "Title.Logout", Type = "Title", Culture = "vi", Value = "Đăng xuất" },
            new() { Code = "Title.Logout", Type = "Title", Culture = "zh", Value = "退出登录" },
            new() { Code = "Title.Administration", Type = "Title", Culture = "en", Value = "Administration" },
            new() { Code = "Title.Administration", Type = "Title", Culture = "vi", Value = "Quản trị" },
            new() { Code = "Title.Administration", Type = "Title", Culture = "zh", Value = "管理" },
            new() { Code = "Title.Settings", Type = "Title", Culture = "en", Value = "Settings" },
            new() { Code = "Title.Settings", Type = "Title", Culture = "vi", Value = "Cài đặt" },
            new() { Code = "Title.Settings", Type = "Title", Culture = "zh", Value = "设置" },
            new() { Code = "Title.Users", Type = "Title", Culture = "en", Value = "Users" },
            new() { Code = "Title.Users", Type = "Title", Culture = "vi", Value = "Người dùng" },
            new() { Code = "Title.Users", Type = "Title", Culture = "zh", Value = "用户" },
            new() { Code = "Title.Contents", Type = "Title", Culture = "en", Value = "Contents" },
            new() { Code = "Title.Contents", Type = "Title", Culture = "vi", Value = "Nội dung" },
            new() { Code = "Title.Contents", Type = "Title", Culture = "zh", Value = "内容" },
            new() { Code = "Title.Files", Type = "Title", Culture = "en", Value = "Files" },
            new() { Code = "Title.Files", Type = "Title", Culture = "vi", Value = "Tệp" },
            new() { Code = "Title.Files", Type = "Title", Culture = "zh", Value = "文件" },
            new() { Code = "Title.Translations", Type = "Title", Culture = "en", Value = "Translations" },
            new() { Code = "Title.Translations", Type = "Title", Culture = "vi", Value = "Bản dịch" },
            new() { Code = "Title.Translations", Type = "Title", Culture = "zh", Value = "翻译" },
            new() { Code = "Title.Culture", Type = "Title", Culture = "en", Value = "Culture" },
            new() { Code = "Title.Culture", Type = "Title", Culture = "vi", Value = "Ngôn ngữ" },
            new() { Code = "Title.Culture", Type = "Title", Culture = "zh", Value = "语言" },

            // Texts
            new() { Code = "Text.Actions", Type = "Text", Culture = "en", Value = "Actions" },
            new() { Code = "Text.Actions", Type = "Text", Culture = "vi", Value = "Thao tác" },
            new() { Code = "Text.Actions", Type = "Text", Culture = "zh", Value = "操作" },
            new() { Code = "Text.Create", Type = "Text", Culture = "en", Value = "Create" },
            new() { Code = "Text.Create", Type = "Text", Culture = "vi", Value = "Tạo mới" },
            new() { Code = "Text.Create", Type = "Text", Culture = "zh", Value = "创建" },
            new() { Code = "Text.Edit", Type = "Text", Culture = "en", Value = "Edit" },
            new() { Code = "Text.Edit", Type = "Text", Culture = "vi", Value = "Chỉnh sửa" },
            new() { Code = "Text.Edit", Type = "Text", Culture = "zh", Value = "编辑" },
            new() { Code = "Text.Delete", Type = "Text", Culture = "en", Value = "Delete" },
            new() { Code = "Text.Delete", Type = "Text", Culture = "vi", Value = "Xóa" },
            new() { Code = "Text.Delete", Type = "Text", Culture = "zh", Value = "删除" },
            new() { Code = "Text.Details", Type = "Text", Culture = "en", Value = "Details" },
            new() { Code = "Text.Details", Type = "Text", Culture = "vi", Value = "Chi tiết" },
            new() { Code = "Text.Details", Type = "Text", Culture = "zh", Value = "详情" },
            new() { Code = "Text.Save", Type = "Text", Culture = "en", Value = "Save" },
            new() { Code = "Text.Save", Type = "Text", Culture = "vi", Value = "Lưu" },
            new() { Code = "Text.Save", Type = "Text", Culture = "zh", Value = "保存" },
            new() { Code = "Text.Cancel", Type = "Text", Culture = "en", Value = "Cancel" },
            new() { Code = "Text.Cancel", Type = "Text", Culture = "vi", Value = "Hủy" },
            new() { Code = "Text.Cancel", Type = "Text", Culture = "zh", Value = "取消" },
            new() { Code = "Text.Back", Type = "Text", Culture = "en", Value = "Back" },
            new() { Code = "Text.Back", Type = "Text", Culture = "vi", Value = "Quay lại" },
            new() { Code = "Text.Back", Type = "Text", Culture = "zh", Value = "返回" },
            new() { Code = "Text.Search", Type = "Text", Culture = "en", Value = "Search" },
            new() { Code = "Text.Search", Type = "Text", Culture = "vi", Value = "Tìm kiếm" },
            new() { Code = "Text.Search", Type = "Text", Culture = "zh", Value = "搜索" },
            new() { Code = "Text.Close", Type = "Text", Culture = "en", Value = "Close" },
            new() { Code = "Text.Close", Type = "Text", Culture = "vi", Value = "Đóng" },
            new() { Code = "Text.Close", Type = "Text", Culture = "zh", Value = "关闭" },
            new() { Code = "Text.Login", Type = "Text", Culture = "en", Value = "Login" },
            new() { Code = "Text.Login", Type = "Text", Culture = "vi", Value = "Đăng nhập" },
            new() { Code = "Text.Login", Type = "Text", Culture = "zh", Value = "登录" },
            new() { Code = "Text.Logout", Type = "Text", Culture = "en", Value = "Logout" },
            new() { Code = "Text.Logout", Type = "Text", Culture = "vi", Value = "Đăng xuất" },
            new() { Code = "Text.Logout", Type = "Text", Culture = "zh", Value = "退出登录" },
            new() { Code = "Text.Captcha", Type = "Text", Culture = "en", Value = "CAPTCHA" },
            new() { Code = "Text.Captcha", Type = "Text", Culture = "vi", Value = "Mã xác minh" },
            new() { Code = "Text.Captcha", Type = "Text", Culture = "zh", Value = "验证码" },
            new() { Code = "Text.PasswordUnchanged", Type = "Text", Culture = "en", Value = "Leave blank to keep the current password." },
            new() { Code = "Text.PasswordUnchanged", Type = "Text", Culture = "vi", Value = "Để trống nếu không thay đổi mật khẩu." },
            new() { Code = "Text.PasswordUnchanged", Type = "Text", Culture = "zh", Value = "留空以保留当前密码。" },

            // Messages
            new() { Code = "Message.LogoutConfirmation", Type = "Message", Culture = "en", Value = "Are you sure you want to log out?" },
            new() { Code = "Message.LogoutConfirmation", Type = "Message", Culture = "vi", Value = "Bạn có chắc chắn muốn đăng xuất không?" },
            new() { Code = "Message.LogoutConfirmation", Type = "Message", Culture = "zh", Value = "确定要退出登录吗？" },
            new() { Code = "Message.Welcome", Type = "Message", Culture = "en", Value = "Welcome to the application!" },
            new() { Code = "Message.Welcome", Type = "Message", Culture = "vi", Value = "Chào mừng bạn đến với ứng dụng!" },
            new() { Code = "Message.Welcome", Type = "Message", Culture = "zh", Value = "欢迎使用本应用！" },
            new() { Code = "Message.Success", Type = "Message", Culture = "en", Value = "Operation completed successfully." },
            new() { Code = "Message.Success", Type = "Message", Culture = "vi", Value = "Thao tác thành công." },
            new() { Code = "Message.Success", Type = "Message", Culture = "zh", Value = "操作成功。" },
            new() { Code = "Message.Failed", Type = "Message", Culture = "en", Value = "Operation failed." },
            new() { Code = "Message.Failed", Type = "Message", Culture = "vi", Value = "Thao tác thất bại." },
            new() { Code = "Message.Failed", Type = "Message", Culture = "zh", Value = "操作失败。" },
            new() { Code = "Message.Required", Type = "Message", Culture = "en", Value = "{0} is required." },
            new() { Code = "Message.Required", Type = "Message", Culture = "vi", Value = "{0} là bắt buộc." },
            new() { Code = "Message.Required", Type = "Message", Culture = "zh", Value = "{0}为必填项。" },
            new() { Code = "Message.DeleteConfirmation", Type = "Message", Culture = "en", Value = "Are you sure you want to delete this item?" },
            new() { Code = "Message.DeleteConfirmation", Type = "Message", Culture = "vi", Value = "Bạn có chắc chắn muốn xóa mục này không?" },
            new() { Code = "Message.DeleteConfirmation", Type = "Message", Culture = "zh", Value = "确定要删除此项吗？" },

            // AppUser
            new() { Code = "AppUser.Id", Type = "AppUser", Culture = "en", Value = "ID" },
            new() { Code = "AppUser.Id", Type = "AppUser", Culture = "vi", Value = "ID" },
            new() { Code = "AppUser.Id", Type = "AppUser", Culture = "zh", Value = "ID" },
            new() { Code = "AppUser.Username", Type = "AppUser", Culture = "en", Value = "Username" },
            new() { Code = "AppUser.Username", Type = "AppUser", Culture = "vi", Value = "Tên đăng nhập" },
            new() { Code = "AppUser.Username", Type = "AppUser", Culture = "zh", Value = "用户名" },
            new() { Code = "AppUser.Password", Type = "AppUser", Culture = "en", Value = "Password" },
            new() { Code = "AppUser.Password", Type = "AppUser", Culture = "vi", Value = "Mật khẩu" },
            new() { Code = "AppUser.Password", Type = "AppUser", Culture = "zh", Value = "密码" },
            new() { Code = "AppUser.Status", Type = "AppUser", Culture = "en", Value = "Status" },
            new() { Code = "AppUser.Status", Type = "AppUser", Culture = "vi", Value = "Trạng thái" },
            new() { Code = "AppUser.Status", Type = "AppUser", Culture = "zh", Value = "状态" },
            new() { Code = "AppUser.Role", Type = "AppUser", Culture = "en", Value = "Role" },
            new() { Code = "AppUser.Role", Type = "AppUser", Culture = "vi", Value = "Vai trò" },
            new() { Code = "AppUser.Role", Type = "AppUser", Culture = "zh", Value = "角色" },

            // AppUserStatus
            new() { Code = "AppUserStatus.Active", Type = "AppUserStatus", Culture = "en", Value = "Active" },
            new() { Code = "AppUserStatus.Active", Type = "AppUserStatus", Culture = "vi", Value = "Hoạt động" },
            new() { Code = "AppUserStatus.Active", Type = "AppUserStatus", Culture = "zh", Value = "启用" },
            new() { Code = "AppUserStatus.Inactive", Type = "AppUserStatus", Culture = "en", Value = "Inactive" },
            new() { Code = "AppUserStatus.Inactive", Type = "AppUserStatus", Culture = "vi", Value = "Không hoạt động" },
            new() { Code = "AppUserStatus.Inactive", Type = "AppUserStatus", Culture = "zh", Value = "停用" }
        };

        foreach (var item in defaults)
        {
            item.Protected = true;
            
            if (existingResources.Data.Any(x =>
            string.Equals(x.Code, item.Code, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Culture, item.Culture, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Resource '{Code}' ({Culture}) already exists. Skipping creation.", item.Code, item.Culture);
                continue;
            }

            var created = await _core.CreateResourceAsync(item);

            if (!created.Success)
            {
                _logger.LogError("Failed to create resource '{Code}'", item.Code);
                foreach (var error in created.Messages)
                {
                    _logger.LogError("Error: {Error}", error);
                }

                return false;
            }

            _logger.LogInformation("Created resource: {Code}", item.Code);
        }

        return true;
    }
    #endregion

    #region Users
    public async Task<bool> InitiateUsersAsync()
    {
        var existingUsers = await _core.GetUsersAsync();
        if (!existingUsers.Success || existingUsers.Data == null)
        {
            _logger.LogError("Failed to retrieve existing users.");
            foreach (var error in existingUsers.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        if (existingUsers.Data.Any(u => u.Username == "administrator"))
        {
            _logger.LogInformation("Administrator account already exists. Skipping creation.");
            return true;
        }

        var administrator = new AppUserModel
        {
            Username = "administrator",
            Password = "12345678",
            Status = 1,
            Role = 1,
            Protected = true
        };

        var created = await _core.CreateUserAsync(administrator);

        if (!created.Success)
        {
            _logger.LogError("Failed to create administrator account.");
            foreach (var error in created.Messages)
            {
                _logger.LogError("Error: {Error}", error);
            }

            return false;
        }

        _logger.LogInformation("Administrator account created.");
        return true;
    }

    #endregion

}
