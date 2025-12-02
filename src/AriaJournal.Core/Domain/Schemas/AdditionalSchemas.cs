// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Schemas/AdditionalSchemas.cs
// توضیح: کلاس‌های Schema اضافی - بدون کلاس‌های تکراری
// نکته: WidgetSchema و FilterSchema در MetadataModels.cs هستند
// نکته: OptionSchema و ValidationSchema در FormSchema.cs هستند
// =============================================================================

namespace AriaJournal.Core.Domain.Schemas;

#region Menu Schema

/// <summary>
/// Schema منو
/// </summary>
public class MenuSchema
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public List<MenuGroupSchema> Groups { get; set; } = new();
    public string? DefaultActiveItem { get; set; }
}

/// <summary>
/// گروه منو
/// </summary>
public class MenuGroupSchema
{
    public string Id { get; set; } = string.Empty;
    public string? TitleFa { get; set; }
    public string? Icon { get; set; }
    public List<MenuItemSchema> Items { get; set; } = new();
    public bool ShowSeparator { get; set; } = true;
    public int Order { get; set; }
}

/// <summary>
/// آیتم منو
/// </summary>
public class MenuItemSchema
{
    public string Id { get; set; } = string.Empty;
    public string? TitleFa { get; set; }
    public string? Icon { get; set; }
    public string? Tooltip { get; set; }
    public string? NavigateTo { get; set; }
    public string? Command { get; set; }
    public string? Badge { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public List<MenuItemSchema>? Children { get; set; }
    public int Order { get; set; }
    public string? Permission { get; set; }
}

#endregion

#region Dashboard Schema

/// <summary>
/// Schema داشبورد
/// </summary>
public class DashboardSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public int Columns { get; set; } = 3;
    public int RowHeight { get; set; } = 200;
    public int Gap { get; set; } = 10;
    public List<DashboardWidgetSchema> Widgets { get; set; } = new();
    public bool CanCustomize { get; set; } = true;
}

/// <summary>
/// تنظیمات ویجت در داشبورد
/// </summary>
public class DashboardWidgetSchema
{
    public string WidgetId { get; set; } = string.Empty;
    public int Column { get; set; }
    public int Row { get; set; }
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public bool Visible { get; set; } = true;
    public Dictionary<string, object>? Settings { get; set; }
}

#endregion

#region Tab Schema

/// <summary>
/// Schema تب
/// </summary>
public class TabSchema
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public List<TabItemSchema> Tabs { get; set; } = new();
    public string? DefaultTabId { get; set; }
    public bool CanAddTabs { get; set; }
    public bool CanCloseTabs { get; set; }
    public bool CanReorderTabs { get; set; }
}

/// <summary>
/// آیتم تب
/// </summary>
public class TabItemSchema
{
    public string Id { get; set; } = string.Empty;
    public string? TitleFa { get; set; }
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool Closeable { get; set; }
    public string? ContentType { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public int Order { get; set; }
}

#endregion

#region DataGrid Schema

/// <summary>
/// Schema گرید داده
/// </summary>
public class DataGridSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string? DataSource { get; set; }
    public List<DataGridColumnSchema> Columns { get; set; } = new();
    public bool AllowSort { get; set; } = true;
    public bool AllowFilter { get; set; } = true;
    public bool AllowPaging { get; set; } = true;
    public int PageSize { get; set; } = 50;
    public bool AllowSelection { get; set; } = true;
    public SelectionModeType SelectionMode { get; set; } = SelectionModeType.Single;
    public bool AllowExport { get; set; } = true;
    public bool ShowRowNumbers { get; set; } = true;
    public string? DefaultSortColumn { get; set; }
    public bool DefaultSortAscending { get; set; } = true;
}

/// <summary>
/// ستون گرید
/// </summary>
public class DataGridColumnSchema
{
    public string Id { get; set; } = string.Empty;
    public string? HeaderFa { get; set; }
    public string? BindingPath { get; set; }
    public ColumnType Type { get; set; } = ColumnType.Text;
    public int Width { get; set; } = 100;
    public int MinWidth { get; set; } = 50;
    public int MaxWidth { get; set; } = 500;
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public bool Resizable { get; set; } = true;
    public string? Format { get; set; }
    public string? CellTemplate { get; set; }
    public HorizontalAlignmentType Alignment { get; set; } = HorizontalAlignmentType.Right;
    public string? ForegroundBinding { get; set; }
    public string? BackgroundBinding { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// نوع ستون
/// </summary>
public enum ColumnType
{
    Text,
    Number,
    Decimal,
    Date,
    DateTime,
    Boolean,
    Currency,
    Percent,
    Progress,
    Badge,
    Link,
    Image,
    Custom
}

/// <summary>
/// حالت انتخاب
/// </summary>
public enum SelectionModeType
{
    None,
    Single,
    Multiple
}

/// <summary>
/// تراز افقی
/// </summary>
public enum HorizontalAlignmentType
{
    Right,
    Left,
    Center
}

#endregion

#region Filter Field Schema (کمکی برای FilterSchema در MetadataModels)

/// <summary>
/// فیلد فیلتر
/// </summary>
public class FilterFieldSchema
{
    public string Id { get; set; } = string.Empty;
    public string LabelFa { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public FilterFieldType Type { get; set; } = FilterFieldType.Text;
    public List<OptionSchema>? Options { get; set; }
    public object? DefaultValue { get; set; }
    public bool IsQuickFilter { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// نوع فیلد فیلتر
/// </summary>
public enum FilterFieldType
{
    Text,
    Number,
    NumberRange,
    Date,
    DateRange,
    Select,
    MultiSelect,
    Boolean,
    Custom
}

/// <summary>
/// پریست فیلتر
/// </summary>
public class FilterPreset
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public Dictionary<string, object> Values { get; set; } = new();
    public bool IsBuiltIn { get; set; }
}

#endregion

#region Report Schema

/// <summary>
/// Schema گزارش
/// </summary>
public class ReportSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public ReportPageSettings PageSettings { get; set; } = new();
    public List<ReportSectionSchema> Sections { get; set; } = new();
    public List<ReportParameterSchema> Parameters { get; set; } = new();
}

/// <summary>
/// تنظیمات صفحه گزارش
/// </summary>
public class ReportPageSettings
{
    public string PageSize { get; set; } = "A4";
    public bool IsLandscape { get; set; }
    public int MarginTop { get; set; } = 20;
    public int MarginBottom { get; set; } = 20;
    public int MarginLeft { get; set; } = 20;
    public int MarginRight { get; set; } = 20;
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public bool ShowPageNumbers { get; set; } = true;
}

/// <summary>
/// بخش گزارش
/// </summary>
public class ReportSectionSchema
{
    public string Id { get; set; } = string.Empty;
    public string? TitleFa { get; set; }
    public ReportSectionType Type { get; set; }
    public string? DataSource { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// نوع بخش گزارش
/// </summary>
public enum ReportSectionType
{
    Title,
    Summary,
    Table,
    Chart,
    Text,
    Image,
    Separator,
    Custom
}

/// <summary>
/// پارامتر گزارش
/// </summary>
public class ReportParameterSchema
{
    public string Id { get; set; } = string.Empty;
    public string LabelFa { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public object? DefaultValue { get; set; }
    public bool Required { get; set; }
}

#endregion

#region User Customization Schemas

/// <summary>
/// تعریف فیلد سفارشی شده توسط کاربر
/// </summary>
public class UserFieldCustomization
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FieldId { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
    public int Order { get; set; }
    public string? DefaultValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// تعریف ستون سفارشی شده توسط کاربر
/// </summary>
public class UserColumnCustomization
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ColumnId { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
    public int Width { get; set; } = 100;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// فیلد تعریف‌شده توسط کاربر
/// </summary>
public class UserDefinedField
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FieldId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? DefaultValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// سفارشی‌سازی ویجت توسط کاربر
/// </summary>
public class UserWidgetCustomization
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string WidgetId { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

#endregion

/// <summary>
/// عملیات جدول
/// </summary>
public class TableActionSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string Type { get; set; } = "button";
    public string? Command { get; set; }
    public bool RequiresSelection { get; set; }
    public int Order { get; set; }
}

#region Form & Table Schemas

/// <summary>
/// تعریف جدول
/// </summary>
public class TableSchema
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? Description { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new();
    public List<TableActionSchema> Actions { get; set; } = new();
    public List<FilterSchema> Filters { get; set; } = new();
    public bool AllowAdd { get; set; } = true;
    public bool AllowEdit { get; set; } = true;
    public bool AllowDelete { get; set; } = true;
    public string? DataSource { get; set; }
    public int DefaultPageSize { get; set; } = 50;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// تعریف ستون جدول
/// </summary>
public class ColumnSchema
{
    public string Field { get; set; } = string.Empty;
    public string HeaderFa { get; set; } = string.Empty;
    public string? HeaderEn { get; set; }
    public string Type { get; set; } = "text";
    public string? BindingPath { get; set; }
    public int Width { get; set; } = 100;
    public bool Visible { get; set; } = true;
    public bool IsSortable { get; set; } = true;
    public bool IsFilterable { get; set; } = true;
    public int Order { get; set; }
    public string? Format { get; set; }
    public bool IsRequired { get; set; }
}

/// <summary>
/// تنظیم پیش‌فرض فرم
/// </summary>
public class FormPreset
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string PresetName { get; set; } = string.Empty;
    public string FormId { get; set; } = string.Empty;
    public Dictionary<string, object> FieldValues { get; set; } = new();
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// فیلتر ذخیره‌شده
/// </summary>
public class SavedFilter
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FilterName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilterExpression { get; set; } = string.Empty;
    public Dictionary<string, object> FilterCriteria { get; set; } = new();
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

#endregion

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Schemas/AdditionalSchemas.cs
// =============================================================================