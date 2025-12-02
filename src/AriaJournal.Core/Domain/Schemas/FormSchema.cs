// ═══════════════════════════════════════════════════════════════════════
// فایل: FormSchema.cs
// مسیر: src/AriaJournal.Core/Domain/Schemas/FormSchema.cs
// توضیح: تمام کلاس‌های Schema برای سیستم Meta-Driven
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Schemas;

// ═══════════════════════════════════════════════════════════════════════
// FormSchema - تعریف فرم
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف یک فرم در Schema
/// </summary>
public class FormSchema
{
    /// <summary>
    /// شناسه یکتای فرم
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی فرم
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی فرم
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// جهت نمایش (rtl یا ltr)
    /// </summary>
    public string Direction { get; set; } = "rtl";

    /// <summary>
    /// توضیحات فرم
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آیکون فرم
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// لیست بخش‌های فرم
    /// </summary>
    public List<SectionSchema> Sections { get; set; } = new();

    /// <summary>
    /// لیست اکشن‌های فرم (دکمه‌ها)
    /// </summary>
    public List<ActionSchema> Actions { get; set; } = new();

    /// <summary>
    /// لیست قوانین فرم
    /// </summary>
    public List<RuleSchema> Rules { get; set; } = new();

    /// <summary>
    /// تنظیمات اضافی
    /// </summary>
    public Dictionary<string, object>? Settings { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// SectionSchema - تعریف بخش
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف یک بخش (Section) در فرم
/// </summary>
public class SectionSchema
{
    /// <summary>
    /// شناسه یکتای بخش
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی بخش
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی بخش
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// توضیحات بخش
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// آیا بخش به صورت پیش‌فرض جمع شده باشد؟
    /// </summary>
    public bool Collapsed { get; set; } = false;

    /// <summary>
    /// آیکون بخش
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// آیا بخش قابل مشاهده است؟
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// شرط نمایش (Expression)
    /// </summary>
    public string? VisibleCondition { get; set; }

    /// <summary>
    /// لیست فیلدهای این بخش
    /// </summary>
    public List<FieldSchema> Fields { get; set; } = new();

    /// <summary>
    /// تعداد ستون‌ها
    /// </summary>
    public int Columns { get; set; } = 1;

    /// <summary>
    /// استایل سفارشی
    /// </summary>
    public Dictionary<string, string>? CustomStyles { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// ActionSchema - تعریف اکشن/دکمه
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف یک اکشن (دکمه) در فرم
/// </summary>
public class ActionSchema
{
    /// <summary>
    /// شناسه یکتای اکشن
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// متن فارسی دکمه
    /// </summary>
    public string LabelFa { get; set; } = string.Empty;

    /// <summary>
    /// متن انگلیسی دکمه
    /// </summary>
    public string? LabelEn { get; set; }

    /// <summary>
    /// نوع اکشن: Submit, Cancel, Reset, Custom, Navigate
    /// </summary>
    public string Type { get; set; } = "Submit";

    /// <summary>
    /// استایل دکمه: Primary, Secondary, Danger, Success, Warning
    /// </summary>
    public string Style { get; set; } = "Primary";

    /// <summary>
    /// آیکون دکمه
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// آیا دکمه فعال است؟
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// شرط فعال بودن
    /// </summary>
    public string? EnabledCondition { get; set; }

    /// <summary>
    /// آیا دکمه قابل مشاهده است؟
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// شرط نمایش
    /// </summary>
    public string? VisibleCondition { get; set; }

    /// <summary>
    /// پیام تأیید قبل از اجرا
    /// </summary>
    public string? ConfirmMessage { get; set; }

    /// <summary>
    /// مسیر ناوبری
    /// </summary>
    public string? NavigateTo { get; set; }

    /// <summary>
    /// نام متد سفارشی
    /// </summary>
    public string? CustomHandler { get; set; }

    /// <summary>
    /// کلید میانبر
    /// </summary>
    public string? Shortcut { get; set; }

    /// <summary>
    /// Tooltip
    /// </summary>
    public string? Tooltip { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// OptionSchema و ValidationSchema تعریف شده در FieldSchema.cs هستند
// ═══════════════════════════════════════════════════════════════════════

// WidgetSchema - تعریف ویجت داشبورد توسط MetadataModels.cs

/// <summary>
/// تنظیمات نمودار
/// </summary>
public class ChartSettings
{
    public string? XAxis { get; set; }
    public string? YAxis { get; set; }
    public string? XAxisLabel { get; set; }
    public string? YAxisLabel { get; set; }
    public bool ShowLegend { get; set; } = true;
    public bool ShowValues { get; set; } = false;
    public List<string>? Colors { get; set; }
    public bool Filled { get; set; } = false;
    public string? Interpolation { get; set; }
}

/// <summary>
/// تنظیمات KPI
/// </summary>
public class KpiSettings
{
    public string? Format { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public bool ShowChange { get; set; } = false;
    public string? ComparePeriod { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? DangerThreshold { get; set; }
    public string PositiveColor { get; set; } = "#4CAF50";
    public string NegativeColor { get; set; } = "#F44336";
    public bool Inverted { get; set; } = false;
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: FormSchema.cs
// ═══════════════════════════════════════════════════════════════════════