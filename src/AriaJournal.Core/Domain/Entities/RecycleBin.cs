// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/RecycleBin.cs
// شماره فایل: 13
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت سطل زباله برای بازیابی آیتم‌های حذف‌شده
/// </summary>
public class RecycleBin
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
    /// نوع موجودیت (Trade, Account, ...)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// شناسه موجودیت اصلی
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// داده موجودیت به صورت JSON
    /// </summary>
    public string EntityData { get; set; } = string.Empty;

    /// <summary>
    /// تاریخ حذف
    /// </summary>
    public DateTime DeletedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ انقضا (حذف دائمی)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// آیا بازیابی شده
    /// </summary>
    public bool IsRestored { get; set; }

    /// <summary>
    /// تاریخ بازیابی
    /// </summary>
    public DateTime? RestoredAt { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/RecycleBin.cs
// =============================================================================