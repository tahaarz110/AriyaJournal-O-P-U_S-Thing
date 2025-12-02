// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/LoginDto.cs
// شماره فایل: 62
// توضیح: DTO ورود کاربر
// =============================================================================

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO برای درخواست ورود
/// </summary>
public class LoginDto
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
    /// مرا به خاطر بسپار
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// DTO برای پاسخ ورود
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// آیا موفق بود
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// توکن (برای استفاده آینده)
    /// </summary>
    public string? Token { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/LoginDto.cs
// =============================================================================