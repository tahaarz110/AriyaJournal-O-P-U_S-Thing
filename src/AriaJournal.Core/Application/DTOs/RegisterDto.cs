// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/RegisterDto.cs
// شماره فایل: 63
// توضیح: DTO ثبت‌نام کاربر
// =============================================================================

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO برای درخواست ثبت‌نام
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// تکرار رمز عبور
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO برای پاسخ ثبت‌نام
/// </summary>
public class RegisterResponseDto
{
    /// <summary>
    /// آیا موفق بود
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// شناسه کاربر ایجاد شده
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// کلید بازیابی (فقط یکبار نمایش داده می‌شود)
    /// </summary>
    public string RecoveryKey { get; set; } = string.Empty;

    /// <summary>
    /// پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO برای بازیابی رمز عبور
/// </summary>
public class RecoverPasswordDto
{
    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// کلید بازیابی
    /// </summary>
    public string RecoveryKey { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور جدید
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// تکرار رمز عبور جدید
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// DTO برای تغییر رمز عبور
/// </summary>
public class ChangePasswordDto
{
    /// <summary>
    /// رمز عبور فعلی
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// رمز عبور جدید
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// تکرار رمز عبور جدید
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/RegisterDto.cs
// =============================================================================