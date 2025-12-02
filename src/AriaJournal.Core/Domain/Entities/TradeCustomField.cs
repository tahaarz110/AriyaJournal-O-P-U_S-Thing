// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/TradeCustomField.cs
// شماره فایل: 10
// =============================================================================

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت مقدار فیلد سفارشی معامله
/// </summary>
public class TradeCustomField
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
    /// شناسه تعریف فیلد
    /// </summary>
    public int FieldDefinitionId { get; set; }

    /// <summary>
    /// مقدار فیلد (به صورت رشته ذخیره می‌شود)
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    /// <summary>
    /// معامله مرتبط
    /// </summary>
    public virtual Trade? Trade { get; set; }

    /// <summary>
    /// تعریف فیلد مرتبط
    /// </summary>
    public virtual FieldDefinition? FieldDefinition { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/TradeCustomField.cs
// =============================================================================