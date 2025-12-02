// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/QueryEngine.cs
// توضیح: موتور کوئری و فیلتر داینامیک - نسخه اصلاح‌شده
// =============================================================================

using System.Collections.Concurrent;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// اینترفیس موتور کوئری
/// </summary>
public interface IQueryEngine
{
    IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterDefinition> filters) where T : class;
    IQueryable<T> ApplySort<T>(IQueryable<T> query, string sortColumn, bool ascending = true) where T : class;
    IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class;
    Task<Result<bool>> SaveQueryTemplateAsync(QueryTemplate template);
    Task<List<QueryTemplate>> GetQueryTemplatesAsync(string? category = null);
    Task<Result<bool>> DeleteQueryTemplateAsync(string templateId);
    IQueryable<T> ApplyTemplate<T>(IQueryable<T> query, string templateId) where T : class;
}

/// <summary>
/// تعریف فیلتر
/// </summary>
public class FilterDefinition
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;
    public object? Value { get; set; }
    public object? Value2 { get; set; }
    public FilterLogic Logic { get; set; } = FilterLogic.And;
}

/// <summary>
/// عملگرهای فیلتر
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
/// منطق ترکیب فیلترها
/// </summary>
public enum FilterLogic
{
    And,
    Or
}

/// <summary>
/// Template کوئری
/// </summary>
public class QueryTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<FilterDefinition> Filters { get; set; } = new();
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
    public int? PageSize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// پیاده‌سازی موتور کوئری
/// </summary>
public class QueryEngine : IQueryEngine
{
    private readonly ConcurrentDictionary<string, QueryTemplate> _templates;
    private readonly ICacheEngine _cacheEngine;
    private readonly string _templatesPath;

    public QueryEngine(ICacheEngine cacheEngine)
    {
        _cacheEngine = cacheEngine;
        _templates = new ConcurrentDictionary<string, QueryTemplate>();

        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _templatesPath = Path.Combine(appPath, "data", "query_templates.json");

        _ = LoadTemplatesAsync();
    }

    #region Filter Application

    public IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterDefinition> filters) where T : class
    {
        if (filters == null || !filters.Any())
            return query;

        Expression<Func<T, bool>>? combinedExpression = null;

        foreach (var filter in filters)
        {
            var filterExpression = BuildFilterExpression<T>(filter);
            if (filterExpression == null) continue;

            if (combinedExpression == null)
            {
                combinedExpression = filterExpression;
            }
            else
            {
                combinedExpression = filter.Logic == FilterLogic.And
                    ? CombineAnd(combinedExpression, filterExpression)
                    : CombineOr(combinedExpression, filterExpression);
            }
        }

        return combinedExpression != null
            ? query.Where(combinedExpression)
            : query;
    }

    private Expression<Func<T, bool>>? BuildFilterExpression<T>(FilterDefinition filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, filter.Field);

        if (property == null)
            return null;

        Expression? comparison = null;

        try
        {
            comparison = filter.Operator switch
            {
                FilterOperator.Equals => BuildEqualsExpression(property, filter.Value),
                FilterOperator.NotEquals => Expression.Not(BuildEqualsExpression(property, filter.Value)),
                FilterOperator.Contains => BuildContainsExpression(property, filter.Value),
                FilterOperator.StartsWith => BuildStartsWithExpression(property, filter.Value),
                FilterOperator.EndsWith => BuildEndsWithExpression(property, filter.Value),
                FilterOperator.GreaterThan => Expression.GreaterThan(property, GetConstantExpression(filter.Value, property.Type)),
                FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, GetConstantExpression(filter.Value, property.Type)),
                FilterOperator.LessThan => Expression.LessThan(property, GetConstantExpression(filter.Value, property.Type)),
                FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(property, GetConstantExpression(filter.Value, property.Type)),
                FilterOperator.Between => BuildBetweenExpression(property, filter.Value, filter.Value2),
                FilterOperator.In => BuildInExpression(property, filter.Value),
                FilterOperator.NotIn => Expression.Not(BuildInExpression(property, filter.Value)),
                FilterOperator.IsNull => Expression.Equal(property, Expression.Constant(null)),
                FilterOperator.IsNotNull => Expression.NotEqual(property, Expression.Constant(null)),
                _ => null
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ساخت Expression: {ex.Message}");
            return null;
        }

        return comparison != null
            ? Expression.Lambda<Func<T, bool>>(comparison, parameter)
            : null;
    }

    private MemberExpression? GetPropertyExpression(ParameterExpression parameter, string propertyPath)
    {
        Expression expression = parameter;

        foreach (var part in propertyPath.Split('.'))
        {
            var property = expression.Type.GetProperty(part,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return null;

            expression = Expression.Property(expression, property);
        }

        return expression as MemberExpression;
    }

    private Expression BuildEqualsExpression(Expression property, object? value)
    {
        var constant = GetConstantExpression(value, property.Type);
        return Expression.Equal(property, constant);
    }

    private Expression BuildContainsExpression(Expression property, object? value)
    {
        if (property.Type != typeof(string))
            throw new ArgumentException("Contains فقط برای رشته‌ها قابل استفاده است");

        var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        var constant = Expression.Constant(value?.ToString() ?? "", typeof(string));
        return Expression.Call(property, method, constant);
    }

    private Expression BuildStartsWithExpression(Expression property, object? value)
    {
        var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
        var constant = Expression.Constant(value?.ToString() ?? "", typeof(string));
        return Expression.Call(property, method, constant);
    }

    private Expression BuildEndsWithExpression(Expression property, object? value)
    {
        var method = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;
        var constant = Expression.Constant(value?.ToString() ?? "", typeof(string));
        return Expression.Call(property, method, constant);
    }

    private Expression BuildBetweenExpression(Expression property, object? value1, object? value2)
    {
        var lower = GetConstantExpression(value1, property.Type);
        var upper = GetConstantExpression(value2, property.Type);

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, lower);
        var lessThanOrEqual = Expression.LessThanOrEqual(property, upper);

        return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
    }

    private Expression BuildInExpression(Expression property, object? value)
    {
        if (value is not IEnumerable<object> values)
            throw new ArgumentException("مقدار In باید یک لیست باشد");

        var convertedValues = values.Select(v => Convert.ChangeType(v, property.Type)).ToList();
        var listType = typeof(List<>).MakeGenericType(property.Type);
        var list = Activator.CreateInstance(listType);
        var addMethod = listType.GetMethod("Add")!;

        foreach (var item in convertedValues)
        {
            addMethod.Invoke(list, new[] { item });
        }

        var containsMethod = listType.GetMethod("Contains")!;
        var listConstant = Expression.Constant(list);

        return Expression.Call(listConstant, containsMethod, property);
    }

    private ConstantExpression GetConstantExpression(object? value, Type targetType)
    {
        if (value == null)
            return Expression.Constant(null, targetType);

        var convertedValue = Convert.ChangeType(value,
            Nullable.GetUnderlyingType(targetType) ?? targetType);

        return Expression.Constant(convertedValue, targetType);
    }

    private Expression<Func<T, bool>> CombineAnd<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private Expression<Func<T, bool>> CombineOr<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = Expression.OrElse(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    #endregion

    #region Sorting

    public IQueryable<T> ApplySort<T>(IQueryable<T> query, string sortColumn, bool ascending = true) where T : class
    {
        if (string.IsNullOrWhiteSpace(sortColumn))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression(parameter, sortColumn);

        if (property == null)
            return query;

        var lambda = Expression.Lambda(property, parameter);
        var methodName = ascending ? "OrderBy" : "OrderByDescending";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    #endregion

    #region Pagination

    public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int page, int pageSize) where T : class
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;

        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    #endregion

    #region Templates

    public async Task<Result<bool>> SaveQueryTemplateAsync(QueryTemplate template)
    {
        try
        {
            template.UpdatedAt = DateTime.Now;
            _templates.AddOrUpdate(template.Id, template, (_, _) => template);
            await SaveTemplatesToFileAsync();
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Validation($"خطا در ذخیره Template: {ex.Message}"));
        }
    }

    public async Task<List<QueryTemplate>> GetQueryTemplatesAsync(string? category = null)
    {
        await Task.CompletedTask;

        var templates = _templates.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(category))
        {
            templates = templates.Where(t => t.Category == category);
        }

        return templates.OrderBy(t => t.Name).ToList();
    }

    public async Task<Result<bool>> DeleteQueryTemplateAsync(string templateId)
    {
        if (_templates.TryRemove(templateId, out _))
        {
            await SaveTemplatesToFileAsync();
            return Result.Success(true);
        }

        return Result.Failure<bool>(Error.NotFound);
    }

    public IQueryable<T> ApplyTemplate<T>(IQueryable<T> query, string templateId) where T : class
    {
        if (!_templates.TryGetValue(templateId, out var template))
            return query;

        query = ApplyFilters(query, template.Filters);

        if (!string.IsNullOrEmpty(template.SortColumn))
        {
            query = ApplySort(query, template.SortColumn, template.SortAscending);
        }

        return query;
    }

    private async Task LoadTemplatesAsync()
    {
        try
        {
            if (!File.Exists(_templatesPath))
                return;

            var json = await File.ReadAllTextAsync(_templatesPath);
            var templates = System.Text.Json.JsonSerializer.Deserialize<List<QueryTemplate>>(json);

            if (templates != null)
            {
                foreach (var template in templates)
                {
                    _templates.TryAdd(template.Id, template);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری Templates: {ex.Message}");
        }
    }

    private async Task SaveTemplatesToFileAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_templatesPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = System.Text.Json.JsonSerializer.Serialize(
                _templates.Values.ToList(),
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_templatesPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ذخیره Templates: {ex.Message}");
        }
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/QueryEngine.cs
// =============================================================================