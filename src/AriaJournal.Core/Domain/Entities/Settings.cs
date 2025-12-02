// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/Settings.cs
// شماره فایل: 12
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت تنظیمات کاربر
/// </summary>
public class Settings
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// تم برنامه (Dark/Light)
    /// </summary>
    public string Theme { get; set; } = "Dark";

    /// <summary>
    /// زبان
    /// </summary>
    public string Language { get; set; } = "fa";

    /// <summary>
    /// فرمت تاریخ
    /// </summary>
    public string DateFormat { get; set; } = "yyyy/MM/dd";

    /// <summary>
    /// فرمت زمان
    /// </summary>
    public string TimeFormat { get; set; } = "HH:mm";

    /// <summary>
    /// واحد ارز پیش‌فرض
    /// </summary>
    public string DefaultCurrency { get; set; } = "USD";

    /// <summary>
    /// تعداد اعشار برای نمایش قیمت
    /// </summary>
    public int PriceDecimals { get; set; } = 5;

    /// <summary>
    /// تعداد اعشار برای نمایش حجم
    /// </summary>
    public int VolumeDecimals { get; set; } = 2;

    /// <summary>
    /// فعال بودن بکاپ خودکار
    /// </summary>
    public bool AutoBackup { get; set; } = true;

    /// <summary>
    /// بازه بکاپ خودکار (روز)
    /// </summary>
    public int AutoBackupInterval { get; set; } = 7;

    /// <summary>
    /// مسیر بکاپ
    /// </summary>
    public string? BackupPath { get; set; }

    /// <summary>
    /// نمایش اعلان‌ها
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// پخش صدا برای اعلان‌ها
    /// </summary>
    public bool PlaySounds { get; set; } = false;

    /// <summary>
    /// حالت نمایش لیست معاملات
    /// </summary>
    public string TradeListViewMode { get; set; } = "Table";

    /// <summary>
    /// تعداد آیتم در هر صفحه
    /// </summary>
    public int ItemsPerPage { get; set; } = 50;

    /// <summary>
    /// تنظیمات سفارشی (JSON)
    /// </summary>
    public string? CustomSettings { get; set; }

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    /// <summary>
    /// کاربر مالک
    /// </summary>
    public virtual User? User { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/Settings.cs
// =============================================================================