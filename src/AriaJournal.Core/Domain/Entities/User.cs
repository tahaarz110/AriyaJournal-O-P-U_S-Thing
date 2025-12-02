// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/User.cs
// شماره فایل: 6
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت کاربر
/// </summary>
public class User
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام کاربری
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// هش رمز عبور
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// هش کلید بازیابی
    /// </summary>
    public string RecoveryKeyHash { get; set; } = string.Empty;

    /// <summary>
    /// تعداد تلاش‌های ناموفق ورود
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// زمان قفل شدن حساب
    /// </summary>
    public DateTime? LockoutEndTime { get; set; }

    /// <summary>
    /// آخرین زمان ورود
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// فعال بودن حساب
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    /// <summary>
    /// لیست حساب‌های معاملاتی
    /// </summary>
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    /// <summary>
    /// لیست تعاریف فیلد سفارشی
    /// </summary>
    public virtual ICollection<FieldDefinition> FieldDefinitions { get; set; } = new List<FieldDefinition>();

    /// <summary>
    /// تنظیمات کاربر
    /// </summary>
    public virtual Settings? Settings { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/User.cs
// =============================================================================