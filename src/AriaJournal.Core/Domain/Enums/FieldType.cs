// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Enums/FieldType.cs
// شماره فایل: 5
// =============================================================================

namespace AriaJournal.Core.Domain.Enums;

/// <summary>
/// انواع فیلدهای داینامیک
/// </summary>
public enum FieldType
{
    /// <summary>
    /// متن ساده
    /// </summary>
    Text = 1,

    /// <summary>
    /// عدد صحیح
    /// </summary>
    Integer = 2,

    /// <summary>
    /// عدد اعشاری
    /// </summary>
    Decimal = 3,

    /// <summary>
    /// تاریخ
    /// </summary>
    Date = 4,

    /// <summary>
    /// تاریخ و زمان
    /// </summary>
    DateTime = 5,

    /// <summary>
    /// زمان
    /// </summary>
    Time = 6,

    /// <summary>
    /// بله/خیر
    /// </summary>
    Boolean = 7,

    /// <summary>
    /// لیست انتخابی (تک انتخابی)
    /// </summary>
    Select = 8,

    /// <summary>
    /// لیست انتخابی (چند انتخابی)
    /// </summary>
    MultiSelect = 9,

    /// <summary>
    /// متن چند خطی
    /// </summary>
    TextArea = 10,

    /// <summary>
    /// امتیاز (۱ تا ۵)
    /// </summary>
    Rating = 11,

    /// <summary>
    /// رنگ
    /// </summary>
    Color = 12,

    /// <summary>
    /// تصویر
    /// </summary>
    Image = 13,

    /// <summary>
    /// لینک/URL
    /// </summary>
    Url = 14,

    /// <summary>
    /// درصد
    /// </summary>
    Percentage = 15
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Enums/FieldType.cs
// =============================================================================