// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/PluginState.cs
// شماره فایل: 14
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت وضعیت پلاگین
/// </summary>
public class PluginState
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه پلاگین
    /// </summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>
    /// نام پلاگین
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// نسخه پلاگین
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// آیا فعال است
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// ترتیب بارگذاری
    /// </summary>
    public int LoadOrder { get; set; }

    /// <summary>
    /// تنظیمات پلاگین (JSON)
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// تاریخ نصب
    /// </summary>
    public DateTime InstalledAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ آخرین فعال‌سازی
    /// </summary>
    public DateTime? LastEnabledAt { get; set; }

    /// <summary>
    /// تاریخ آخرین غیرفعال‌سازی
    /// </summary>
    public DateTime? LastDisabledAt { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/PluginState.cs
// =============================================================================