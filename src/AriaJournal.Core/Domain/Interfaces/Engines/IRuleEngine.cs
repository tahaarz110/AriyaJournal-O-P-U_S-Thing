// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IRuleEngine.cs
// شماره فایل: 25
// =============================================================================

using System.Windows;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور اجرای قوانین
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// اجرای قوانین یک فرم
    /// </summary>
    void ApplyRules(string formId, FrameworkElement form, string trigger);

    /// <summary>
    /// ارزیابی شرط
    /// </summary>
    bool EvaluateCondition(string condition, Dictionary<string, object?> context);

    /// <summary>
    /// محاسبه عبارت
    /// </summary>
    object? EvaluateExpression(string expression, Dictionary<string, object?> context);

    /// <summary>
    /// ثبت قانون جدید
    /// </summary>
    void RegisterRule(string formId, RuleSchema rule);

    /// <summary>
    /// حذف قوانین یک فرم
    /// </summary>
    void UnregisterRules(string formId);

    /// <summary>
    /// دریافت قوانین یک فرم
    /// </summary>
    List<RuleSchema> GetRules(string formId);

    /// <summary>
    /// ثبت تابع سفارشی
    /// </summary>
    void RegisterFunction(string name, Func<object?[], object?> function);
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IRuleEngine.cs
// =============================================================================