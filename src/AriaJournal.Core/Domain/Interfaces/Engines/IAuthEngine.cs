// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IAuthEngine.cs
// شماره فایل: 27
// =============================================================================

using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور احراز هویت
/// </summary>
public interface IAuthEngine
{
    /// <summary>
    /// ورود کاربر
    /// </summary>
    Task<Result<LoginResult>> LoginAsync(string username, string password);

    /// <summary>
    /// ثبت‌نام کاربر جدید
    /// </summary>
    Task<Result<RegisterResult>> RegisterAsync(string username, string password);

    /// <summary>
    /// تغییر رمز عبور
    /// </summary>
    Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    /// <summary>
    /// بازیابی رمز عبور با کلید بازیابی
    /// </summary>
    Task<Result<bool>> RecoverAsync(string username, string recoveryKey, string newPassword);

    /// <summary>
    /// بررسی قفل بودن حساب
    /// </summary>
    bool IsLockedOut(int userId);

    /// <summary>
    /// خروج کاربر
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// کاربر فعلی
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// آیا کاربر وارد شده
    /// </summary>
    bool IsAuthenticated { get; }
}

/// <summary>
/// نتیجه ورود
/// </summary>
public class LoginResult
{
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// نتیجه ثبت‌نام
/// </summary>
public class RegisterResult
{
    public User User { get; set; } = null!;
    public string RecoveryKey { get; set; } = string.Empty;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IAuthEngine.cs
// =============================================================================