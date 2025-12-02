// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IQueryEngine.cs
// توضیح: اینترفیس موتور کوئری برای فیلتر و جستجوی داینامیک
// =============================================================================

using System.Linq.Expressions;
using AriaJournal.Core.Domain.Common;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// تعریف فیلتر
/// </summary>
public class FilterDefinition
{
    /// <summary>
    /// شناسه فیلتر
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// عملگر مقایسه
    /// </summary>
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;

    /// <summary>
    /// مقدار فیلتر
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// مقدار دوم (برای Between)
    /// </summary>
    public object? Value2 { get; set; }

    /// <summary>
    /// عملگر منطقی با فیلتر بعدی
    /// </summary>
    public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
}

/// <summary>
/// عملگرهای مقایسه
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Between,
    In,
    NotIn,
    IsNull,
    IsNotNull
}

/// <summary>
/// عملگرهای منطقی
/// </summary>
public enum LogicalOperator
{
    And,
    Or
}

/// <summary>
/// تعریف مرتب‌سازی
/// </summary>
public class SortDefinition
{
    /// <summary>
    /// نام فیلد
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// نزولی؟
    /// </summary>
    public bool Descending { get; set; }
}

/// <summary>
/// قالب کوئری ذخیره‌شده
/// </summary>
public class QueryTemplate
{
    /// <summary>
    /// شناسه
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// نام قالب
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// نوع موجودیت
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// فیلترها (JSON)
    /// </summary>
    public string FiltersJson { get; set; } = "[]";

    /// <summary>
    /// مرتب‌سازی‌ها (JSON)
    /// </summary>
    public string SortsJson { get; set; } = "[]";

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// پیش‌فرض؟
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// نتیجه صفحه‌بندی شده
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// آیتم‌ها
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// تعداد کل
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// شماره صفحه
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// اندازه صفحه
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// تعداد کل صفحات
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// صفحه بعدی وجود دارد؟
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// صفحه قبلی وجود دارد؟
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// اینترفیس موتور کوئری
/// </summary>
public interface IQueryEngine
{
    /// <summary>
    /// اعمال فیلترها روی کوئری
    /// </summary>
    IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterDefinition> filters) where T : class;

    /// <summary>
    /// اعمال مرتب‌سازی
    /// </summary>
    IQueryable<T> ApplySort<T>(IQueryable<T> query, string sortColumn, bool ascending = true) where T : class;

    /// <summary>
    /// اعمال صفحه‌بندی
    /// </summary>
    IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class;

    /// <summary>
    /// ذخیره قالب کوئری
    /// </summary>
    Task<Result<bool>> SaveQueryTemplateAsync(QueryTemplate template);

    /// <summary>
    /// دریافت قالب‌های کوئری
    /// </summary>
    Task<List<QueryTemplate>> GetQueryTemplatesAsync(string? category = null);

    /// <summary>
    /// حذف قالب
    /// </summary>
    Task<Result<bool>> DeleteQueryTemplateAsync(string templateId);

    /// <summary>
    /// اعمال قالب
    /// </summary>
    IQueryable<T> ApplyTemplate<T>(IQueryable<T> query, string templateId) where T : class;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IQueryEngine.cs
// =============================================================================