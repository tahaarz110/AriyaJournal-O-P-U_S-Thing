// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Schemas/FieldSchema.cs
// توضیح: تعریف یک فیلد - نسخه کامل با همه Property ها
// =============================================================================

using System.Collections.Generic;

namespace AriaJournal.Core.Domain.Schemas;

/// <summary>
/// تعریف یک فیلد در Schema
/// </summary>
public class FieldSchema
{
    /// <summary>
    /// شناسه فیلد
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// برچسب فارسی
    /// </summary>
    public string LabelFa { get; set; } = string.Empty;

    /// <summary>
    /// برچسب انگلیسی
    /// </summary>
    public string? LabelEn { get; set; }

    /// <summary>
    /// نوع فیلد (text, number, select, date, ...)
    /// </summary>
    public string Type { get; set; } = "text";

    /// <summary>
    /// الزامی بودن
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// فقط خواندنی
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// غیرفعال
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// پنهان
    /// </summary>
    public bool Hidden { get; set; } = false;

    /// <summary>
    /// نمایش داده شود
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// مقدار پیش‌فرض
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// متن Placeholder
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// متن راهنما
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// آیکون
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// عرض
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// تعداد ستون اشغال‌شده (ColSpan)
    /// </summary>
    public int? ColSpan { get; set; }

    /// <summary>
    /// تعداد ستون اشغال‌شده (ColumnSpan - برای سازگاری)
    /// </summary>
    public int? ColumnSpan
    {
        get => ColSpan;
        set => ColSpan = value;
    }

    /// <summary>
    /// تعداد ردیف اشغال‌شده
    /// </summary>
    public int? RowSpan { get; set; }

    /// <summary>
    /// شماره ستون
    /// </summary>
    public int? Column { get; set; }

    /// <summary>
    /// شماره ردیف
    /// </summary>
    public int? Row { get; set; }

    /// <summary>
    /// منبع داده
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// وابستگی به فیلد دیگر
    /// </summary>
    public string? DependsOn { get; set; }

    /// <summary>
    /// شرط نمایش
    /// </summary>
    public string? VisibleCondition { get; set; }

    /// <summary>
    /// شرط فعال بودن
    /// </summary>
    public string? EnableCondition { get; set; }

    /// <summary>
    /// فرمول محاسبه
    /// </summary>
    public string? CalculateExpression { get; set; }

    /// <summary>
    /// قابل ویرایش
    /// </summary>
    public bool Editable { get; set; } = true;

    /// <summary>
    /// قابل جستجو
    /// </summary>
    public bool Searchable { get; set; } = false;

    /// <summary>
    /// قابل مرتب‌سازی
    /// </summary>
    public bool Sortable { get; set; } = false;

    /// <summary>
    /// قابل فیلتر
    /// </summary>
    public bool Filterable { get; set; } = false;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// گروه/دسته
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// گزینه‌ها (برای select)
    /// </summary>
    public List<OptionSchema>? Options { get; set; }

    /// <summary>
    /// تنظیمات اعتبارسنجی
    /// </summary>
    public ValidationSchema? Validation { get; set; }

    /// <summary>
    /// متادیتای اضافی
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// استایل CSS اضافی
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// استایل inline
    /// </summary>
    public string? Style { get; set; }
}

/// <summary>
/// گزینه‌های یک فیلد انتخابی
/// </summary>
public class OptionSchema
{
    /// <summary>
    /// مقدار
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// برچسب فارسی
    /// </summary>
    public string LabelFa { get; set; } = string.Empty;

    /// <summary>
    /// برچسب انگلیسی
    /// </summary>
    public string? LabelEn { get; set; }

    /// <summary>
    /// آیکون
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// رنگ
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// غیرفعال
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// ترتیب
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// گروه
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// تنظیمات اعتبارسنجی
/// </summary>
public class ValidationSchema
{
    /// <summary>
    /// حداقل مقدار عددی
    /// </summary>
    public decimal? Min { get; set; }

    /// <summary>
    /// حداکثر مقدار عددی
    /// </summary>
    public decimal? Max { get; set; }

    /// <summary>
    /// حداقل طول
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// حداکثر طول
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// الگوی Regex
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// پیام خطای فارسی
    /// </summary>
    public string? MessageFa { get; set; }

    /// <summary>
    /// پیام خطای انگلیسی
    /// </summary>
    public string? MessageEn { get; set; }

    /// <summary>
    /// اعتبارسنج سفارشی
    /// </summary>
    public string? CustomValidator { get; set; }

    /// <summary>
    /// مقادیر مجاز
    /// </summary>
    public List<string>? AllowedValues { get; set; }

    /// <summary>
    /// مقادیر غیرمجاز
    /// </summary>
    public List<string>? ForbiddenValues { get; set; }

    /// <summary>
    /// الزامی بودن شرطی
    /// </summary>
    public string? RequiredIf { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Schemas/FieldSchema.cs
// =============================================================================