// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Schemas/MetadataModels.cs
// توضیح: مدل‌های متادیتا - نسخه اصلاح‌شده (بدون تکرار)
// =============================================================================

namespace AriaJournal.Core.Domain.Schemas;

/// <summary>
/// تعریف ویجت
/// </summary>
public class WidgetSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public WidgetType Type { get; set; } = WidgetType.Value;
    public int Width { get; set; }
    public int Height { get; set; }
    public int MinWidth { get; set; } = 150;
    public int MinHeight { get; set; } = 100;
    public string? BackgroundColor { get; set; }
    public bool ShowFooter { get; set; }
    public int RefreshInterval { get; set; }
    public string? DataSource { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// نوع ویجت
/// </summary>
public enum WidgetType
{
    Value,
    Chart,
    List,
    Table,
    Progress,
    Gauge,
    Calendar,
    Custom
}

/// <summary>
/// تعریف فیلتر
/// </summary>
public class FilterSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? TargetDataSource { get; set; }
    public List<FilterFieldSchema> Fields { get; set; } = new();
    public List<FilterPreset> Presets { get; set; } = new();
    public bool AllowSavePreset { get; set; } = true;
}

/// <summary>
/// تعریف ستون قابل سفارشی‌سازی
/// </summary>
public class ColumnDefinition
{
    public string Id { get; set; } = string.Empty;
    public string HeaderFa { get; set; } = string.Empty;
    public string? BindingPath { get; set; }
    public string Type { get; set; } = "text";
    public int Width { get; set; } = 100;
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public int Order { get; set; }
    public string? Format { get; set; }
}

/// <summary>
/// تنظیمات نمایش کاربر
/// </summary>
public class UserDisplaySettings
{
    public int UserId { get; set; }
    public string ViewId { get; set; } = string.Empty;
    public List<ColumnDefinition> Columns { get; set; } = new();
    public string? DefaultSortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
    public int PageSize { get; set; } = 50;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Schemas/MetadataModels.cs
// =============================================================================