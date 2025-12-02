// ═══════════════════════════════════════════════════════════════════════
// فایل: AuditLog.cs
// مسیر: src/AriaJournal.Core/Domain/Entities/AuditLog.cs
// توضیح: لاگ عملیات و تغییرات مهم سیستم
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// لاگ عملیات سیستم
/// </summary>
public class AuditLog
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه کاربر (null برای عملیات سیستمی)
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// کاربر مرتبط
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// نوع عملیات
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// نام موجودیت (Entity)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// شناسه موجودیت
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// مقادیر قبلی (JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// مقادیر جدید (JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آدرس IP (برای آینده)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// نام دستگاه
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// نسخه نرم‌افزار
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// شناسه پلاگین (اگر عملیات توسط پلاگین انجام شده)
    /// </summary>
    public string? PluginId { get; set; }

    /// <summary>
    /// سطح اهمیت
    /// </summary>
    public AuditLevel Level { get; set; } = AuditLevel.Info;

    /// <summary>
    /// آیا عملیات موفق بود؟
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// پیام خطا (در صورت شکست)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// زمان عملیات
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// مدت زمان عملیات (میلی‌ثانیه)
    /// </summary>
    public long? DurationMs { get; set; }
}

/// <summary>
/// انواع عملیات قابل ثبت
/// </summary>
public enum AuditAction
{
    // عملیات کاربر
    Login = 1,
    Logout = 2,
    LoginFailed = 3,
    PasswordChanged = 4,
    PasswordRecovered = 5,

    // عملیات CRUD
    Create = 10,
    Read = 11,
    Update = 12,
    Delete = 13,
    Restore = 14,

    // عملیات معامله
    TradeOpened = 20,
    TradeClosed = 21,
    TradeModified = 22,
    TradeDeleted = 23,

    // عملیات Import/Export
    Import = 30,
    Export = 31,
    Backup = 32,
    Restore_Backup = 33,

    // عملیات پلاگین
    PluginLoaded = 40,
    PluginUnloaded = 41,
    PluginEnabled = 42,
    PluginDisabled = 43,
    PluginError = 44,

    // عملیات سیستم
    AppStarted = 50,
    AppClosed = 51,
    DatabaseMigrated = 52,
    SettingsChanged = 53,
    SchemaReloaded = 54,

    // خطاها
    Error = 90,
    Warning = 91,
    Critical = 92,

    // سایر
    Other = 99
}

/// <summary>
/// سطح اهمیت لاگ
/// </summary>
public enum AuditLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: AuditLog.cs
// ═══════════════════════════════════════════════════════════════════════