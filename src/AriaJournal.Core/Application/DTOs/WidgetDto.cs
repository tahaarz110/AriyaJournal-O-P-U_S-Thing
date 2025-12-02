// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/WidgetDto.cs
// توضیح: DTOs مربوط به ویجت‌ها
// =============================================================================

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO ویجت
/// </summary>
public class WidgetDto
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string Type { get; set; } = "value";
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 200;
    public int Column { get; set; }
    public int Row { get; set; }
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public bool Visible { get; set; } = true;
    public int RefreshInterval { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// DTO تنظیمات داشبورد کاربر
/// </summary>
public class UserDashboardSettingsDto
{
    public int UserId { get; set; }
    public string DashboardId { get; set; } = "default";
    public List<WidgetDto> Widgets { get; set; } = new();
    public int Columns { get; set; } = 3;
    public int RowHeight { get; set; } = 200;
    public int Gap { get; set; } = 15;
}

/// <summary>
/// DTO داده ویجت KPI
/// </summary>
public class KpiWidgetDataDto
{
    public string Value { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Trend { get; set; }
    public string? TrendDirection { get; set; } // up, down, neutral
    public string? Color { get; set; }
}

/// <summary>
/// DTO داده ویجت نمودار
/// </summary>
public class ChartWidgetDataDto
{
    public string ChartType { get; set; } = "line"; // line, bar, pie, area
    public List<ChartDataPointDto> DataPoints { get; set; } = new();
    public List<string>? Labels { get; set; }
    public List<string>? Colors { get; set; }
}

/// <summary>
/// نقطه داده نمودار
/// </summary>
public class ChartDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// DTO داده ویجت لیست
/// </summary>
public class ListWidgetDataDto
{
    public List<ListWidgetItemDto> Items { get; set; } = new();
}

/// <summary>
/// آیتم لیست ویجت
/// </summary>
public class ListWidgetItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/WidgetDto.cs
// =============================================================================