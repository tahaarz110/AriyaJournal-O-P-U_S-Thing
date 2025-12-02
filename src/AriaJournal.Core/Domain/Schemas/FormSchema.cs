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
// OptionSchema - تعریف گزینه
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف یک گزینه برای فیلدهای انتخابی
/// </summary>
public class OptionSchema
{
    /// <summary>
    /// مقدار ذخیره شده
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// متن نمایشی فارسی
    /// </summary>
    public string LabelFa { get; set; } = string.Empty;

    /// <summary>
    /// متن نمایشی انگلیسی
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
    /// ترتیب نمایش
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// آیا این گزینه پیش‌فرض است؟
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// آیا گزینه فعال است؟
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// گروه‌بندی
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// ValidationSchema - تعریف اعتبارسنجی
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف قوانین اعتبارسنجی یک فیلد
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
    /// حداقل طول متن
    /// </summary>
    public int? MinLength { get; set; }

    /// <summary>
    /// حداکثر طول متن
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
    /// آیا فیلد الزامی است؟
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// پیام الزامی بودن
    /// </summary>
    public string? RequiredMessage { get; set; }

    /// <summary>
    /// Expression سفارشی
    /// </summary>
    public string? CustomExpression { get; set; }

    /// <summary>
    /// پیام Expression سفارشی
    /// </summary>
    public string? CustomMessage { get; set; }

    /// <summary>
    /// نوع اعتبارسنجی: Email, Phone, Url
    /// </summary>
    public string? ValidationType { get; set; }

    /// <summary>
    /// حداقل تعداد انتخاب
    /// </summary>
    public int? MinSelections { get; set; }

    /// <summary>
    /// حداکثر تعداد انتخاب
    /// </summary>
    public int? MaxSelections { get; set; }

    /// <summary>
    /// فرمت تاریخ
    /// </summary>
    public string? DateFormat { get; set; }

    /// <summary>
    /// حداقل تاریخ
    /// </summary>
    public DateTime? MinDate { get; set; }

    /// <summary>
    /// حداکثر تاریخ
    /// </summary>
    public DateTime? MaxDate { get; set; }

    /// <summary>
    /// تعداد اعشار مجاز
    /// </summary>
    public int? DecimalPlaces { get; set; }

    /// <summary>
    /// مقادیر مجاز
    /// </summary>
    public List<string>? AllowedValues { get; set; }

    /// <summary>
    /// مقادیر غیرمجاز
    /// </summary>
    public List<string>? ForbiddenValues { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// WidgetSchema - تعریف ویجت داشبورد
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// تعریف یک ویجت برای داشبورد
/// </summary>
public class WidgetSchema
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// عنوان فارسی
    /// </summary>
    public string TitleFa { get; set; } = string.Empty;

    /// <summary>
    /// عنوان انگلیسی
    /// </summary>
    public string? TitleEn { get; set; }

    /// <summary>
    /// نوع ویجت: Chart, KPI, Table, List, Custom
    /// </summary>
    public string Type { get; set; } = "KPI";

    /// <summary>
    /// زیرنوع: Line, Bar, Pie, ...
    /// </summary>
    public string? SubType { get; set; }

    /// <summary>
    /// عرض (1-12 ستون)
    /// </summary>
    public int Width { get; set; } = 4;

    /// <summary>
    /// ارتفاع (ردیف)
    /// </summary>
    public int Height { get; set; } = 1;

    /// <summary>
    /// موقعیت X
    /// </summary>
    public int PositionX { get; set; } = 0;

    /// <summary>
    /// موقعیت Y
    /// </summary>
    public int PositionY { get; set; } = 0;

    /// <summary>
    /// آیکون
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// رنگ پس‌زمینه
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// رنگ متن
    /// </summary>
    public string? TextColor { get; set; }

    /// <summary>
    /// منبع داده
    /// </summary>
    public string? DataSource { get; set; }

    /// <summary>
    /// پارامترهای منبع داده
    /// </summary>
    public Dictionary<string, object>? DataParameters { get; set; }

    /// <summary>
    /// فیلتر پیش‌فرض
    /// </summary>
    public string? DefaultFilter { get; set; }

    /// <summary>
    /// تنظیمات نمودار
    /// </summary>
    public ChartSettings? ChartSettings { get; set; }

    /// <summary>
    /// تنظیمات KPI
    /// </summary>
    public KpiSettings? KpiSettings { get; set; }

    /// <summary>
    /// آیا قابل تغییر اندازه است؟
    /// </summary>
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// آیا قابل جابجایی است؟
    /// </summary>
    public bool Draggable { get; set; } = true;

    /// <summary>
    /// آیا قابل مشاهده است؟
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// ترتیب نمایش
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// بازه رفرش خودکار (ثانیه)
    /// </summary>
    public int? RefreshInterval { get; set; }

    /// <summary>
    /// شناسه پلاگین
    /// </summary>
    public string? PluginId { get; set; }
}

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