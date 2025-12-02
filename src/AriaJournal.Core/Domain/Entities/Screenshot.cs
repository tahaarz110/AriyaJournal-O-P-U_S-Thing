// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/Screenshot.cs
// شماره فایل: 11
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت اسکرین‌شات معامله
/// </summary>
public class Screenshot
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه معامله
    /// </summary>
    public int TradeId { get; set; }

    /// <summary>
    /// نام فایل
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// مسیر فایل
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// نوع تصویر (قبل/بعد/حین معامله)
    /// </summary>
    public string? ImageType { get; set; }

    /// <summary>
    /// تایم‌فریم چارت
    /// </summary>
    public string? Timeframe { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// حجم فایل (بایت)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// عرض تصویر
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// ارتفاع تصویر
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation Properties
    /// <summary>
    /// معامله مرتبط
    /// </summary>
    public virtual Trade? Trade { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/Screenshot.cs
// =============================================================================