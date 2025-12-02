// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/RuleEngine.cs
// شماره فایل: 57
// توضیح: موتور اجرای قوانین فرم
// =============================================================================

using System.Collections.Concurrent;
using System.Windows;                    // حتماً باشه
using System.Windows.Controls;           // برای FrameworkElement و کنترل‌ها
using System.Windows.Data;               // برای Binding اگر نیاز باشه
using NCalc;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;
using System.IO;
using Expression = NCalc.Expression;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور اجرای قوانین
/// </summary>
public class RuleEngine : IRuleEngine
{
    private readonly ISchemaEngine _schemaEngine;
    private readonly ConcurrentDictionary<string, List<RuleSchema>> _rules;
    private readonly ConcurrentDictionary<string, Func<object?[], object?>> _customFunctions;
    private IUIRendererEngine? _uiRenderer;

    public RuleEngine(ISchemaEngine schemaEngine)
    {
        _schemaEngine = schemaEngine ?? throw new ArgumentNullException(nameof(schemaEngine));
        _rules = new ConcurrentDictionary<string, List<RuleSchema>>();
        _customFunctions = new ConcurrentDictionary<string, Func<object?[], object?>>();

        RegisterBuiltInFunctions();
    }

    /// <summary>
    /// تنظیم UIRenderer (برای جلوگیری از وابستگی دایره‌ای)
    /// </summary>
    public void SetUIRenderer(IUIRendererEngine uiRenderer)
    {
        _uiRenderer = uiRenderer;
    }

    public void ApplyRules(string formId, FrameworkElement form, string trigger) // کامنت: default value رو حذف کردم تا match با اینترفیس بشه - اگر نیاز بود برگردون: string propertyName = "Visibility"
    {
        if (string.IsNullOrWhiteSpace(formId) || form == null)
            return;

        // دریافت قوانین از Schema
        var schemaRules = _schemaEngine.GetRules(formId);
        
        // دریافت قوانین ثبت‌شده اضافی
        _rules.TryGetValue(formId, out var additionalRules);

        var allRules = new List<RuleSchema>();
        allRules.AddRange(schemaRules);
        if (additionalRules != null)
        {
            allRules.AddRange(additionalRules);
        }

        // فیلتر بر اساس Trigger
        var applicableRules = allRules
            .Where(r => r.Trigger.Equals(trigger, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.Priority)
            .ToList();

        // استخراج داده‌های فرم برای Context
        var context = ExtractFormContext(form);

        foreach (var rule in applicableRules)
        {
            try
            {
                ExecuteRule(rule, form, context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در اجرای قانون {rule.Id}: {ex.Message}");
            }
        }
    }

    public bool EvaluateCondition(string condition, Dictionary<string, object?> context)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return true;

        try
        {
            // تبدیل عملگرهای فارسی‌پسند
            var normalizedCondition = NormalizeCondition(condition);

            var expression = new Expression(normalizedCondition);

            // تنظیم پارامترها
            foreach (var kvp in context)
            {
                expression.Parameters[kvp.Key] = kvp.Value ?? DBNull.Value;
            }

            // ثبت توابع سفارشی
            expression.EvaluateFunction += (name, args) =>
            {
                if (_customFunctions.TryGetValue(name.ToLower(), out var func))
                {
                    var parameters = args.Parameters.Select(p => p.Evaluate()).ToArray();
                    args.Result = func(parameters);
                }
            };

            var result = expression.Evaluate();
            return result is bool boolResult && boolResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ارزیابی شرط '{condition}': {ex.Message}");
            return false;
        }
    }

    public object? EvaluateExpression(string expressionStr, Dictionary<string, object?> context)
    {
        if (string.IsNullOrWhiteSpace(expressionStr))
            return null;

        try
        {
            var normalizedExpression = NormalizeCondition(expressionStr);
            var expression = new Expression(normalizedExpression);

            // تنظیم پارامترها
            foreach (var kvp in context)
            {
                expression.Parameters[kvp.Key] = kvp.Value ?? 0;
            }

            // ثبت توابع سفارشی
            expression.EvaluateFunction += (name, args) =>
            {
                if (_customFunctions.TryGetValue(name.ToLower(), out var func))
                {
                    var parameters = args.Parameters.Select(p => p.Evaluate()).ToArray();
                    args.Result = func(parameters);
                }
            };

            return expression.Evaluate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در محاسبه عبارت '{expressionStr}': {ex.Message}");
            return null;
        }
    }

    public void RegisterRule(string formId, RuleSchema rule)
    {
        if (string.IsNullOrWhiteSpace(formId) || rule == null)
            return;

        var rules = _rules.GetOrAdd(formId, _ => new List<RuleSchema>());
        
        // حذف قانون قبلی با همان شناسه
        rules.RemoveAll(r => r.Id == rule.Id);
        rules.Add(rule);
    }

    public void UnregisterRules(string formId)
    {
        if (!string.IsNullOrWhiteSpace(formId))
        {
            _rules.TryRemove(formId, out _);
        }
    }

    public List<RuleSchema> GetRules(string formId)
    {
        var schemaRules = _schemaEngine.GetRules(formId);
        _rules.TryGetValue(formId, out var additionalRules);

        var allRules = new List<RuleSchema>();
        allRules.AddRange(schemaRules);
        if (additionalRules != null)
        {
            allRules.AddRange(additionalRules);
        }

        return allRules;
    }

    public void RegisterFunction(string name, Func<object?[], object?> function)
    {
        if (string.IsNullOrWhiteSpace(name) || function == null)
            return;

        _customFunctions.AddOrUpdate(name.ToLower(), function, (_, _) => function);
    }

    #region Private Methods

    private void ExecuteRule(RuleSchema rule, FrameworkElement form, Dictionary<string, object?> context)
    {
        // بررسی شرط
        if (!string.IsNullOrWhiteSpace(rule.Condition))
        {
            if (!EvaluateCondition(rule.Condition, context))
            {
                return;
            }
        }

        // اجرای عمل
        switch (rule.Action.ToLower())
        {
            case "show":
                ExecuteShowAction(form, rule.Target);
                break;

            case "hide":
                ExecuteHideAction(form, rule.Target);
                break;

            case "enable":
                ExecuteEnableAction(form, rule.Target);
                break;

            case "disable":
                ExecuteDisableAction(form, rule.Target);
                break;

            case "setvalue":
                ExecuteSetValueAction(form, rule.Target, rule.Value, context);
                break;

            case "calculate":
                ExecuteCalculateAction(form, rule.Target, rule.Value, context);
                break;

            case "validate":
                ExecuteValidateAction(form, rule.Target, rule.Value);
                break;

            case "required":
                ExecuteRequiredAction(form, rule.Target, true);
                break;

            case "optional":
                ExecuteRequiredAction(form, rule.Target, false);
                break;

            case "clear":
                ExecuteClearAction(form, rule.Target);
                break;

            default:
                System.Diagnostics.Debug.WriteLine($"عمل ناشناخته: {rule.Action}");
                break;
        }
    }

    private void ExecuteShowAction(FrameworkElement form, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        _uiRenderer.SetFieldVisibility(form, target, true);
    }

    private void ExecuteHideAction(FrameworkElement form, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        _uiRenderer.SetFieldVisibility(form, target, false);
    }

    private void ExecuteEnableAction(FrameworkElement form, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        _uiRenderer.SetFieldEnabled(form, target, true);
    }

    private void ExecuteDisableAction(FrameworkElement form, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        _uiRenderer.SetFieldEnabled(form, target, false);
    }

    private void ExecuteSetValueAction(FrameworkElement form, string? target, string? value, Dictionary<string, object?> context)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        // اگر مقدار شامل متغیر است، محاسبه کن
        object? finalValue = value;
        if (!string.IsNullOrWhiteSpace(value) && (value.Contains("[") || value.Contains("{")))
        {
            finalValue = ReplaceVariables(value, context);
        }

        _uiRenderer.SetFieldValue(form, target, finalValue);
    }

    private void ExecuteCalculateAction(FrameworkElement form, string? target, string? expression, Dictionary<string, object?> context)
    {
        if (string.IsNullOrWhiteSpace(target) || string.IsNullOrWhiteSpace(expression) || _uiRenderer == null)
            return;

        var result = EvaluateExpression(expression, context);
        _uiRenderer.SetFieldValue(form, target, result);
    }

    private void ExecuteValidateAction(FrameworkElement form, string? target, string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        // نمایش پیام خطا
        _uiRenderer.ShowFieldError(form, target, errorMessage ?? "مقدار نامعتبر است");
    }

    private void ExecuteRequiredAction(FrameworkElement form, string? target, bool required)
    {
        // این عمل نیاز به پیاده‌سازی خاص دارد
        // فعلاً فقط لاگ می‌کنیم
        System.Diagnostics.Debug.WriteLine($"تغییر وضعیت الزامی فیلد {target} به {required}");
    }

    private void ExecuteClearAction(FrameworkElement form, string? target)
    {
        if (string.IsNullOrWhiteSpace(target) || _uiRenderer == null)
            return;

        _uiRenderer.SetFieldValue(form, target, null);
    }

    private Dictionary<string, object?> ExtractFormContext(FrameworkElement form)
    {
        if (_uiRenderer == null)
            return new Dictionary<string, object?>();

        return _uiRenderer.ExtractData(form);
    }

    private string NormalizeCondition(string condition)
    {
        // تبدیل عملگرها به فرمت استاندارد
        return condition
            .Replace("==", "=")
            .Replace("!=", "<>")
            .Replace("&&", " AND ")
            .Replace("||", " OR ")
            .Replace("!", " NOT ")
            .Replace("null", "NULL")
            .Replace("true", "True")
            .Replace("false", "False");
    }

    private string ReplaceVariables(string template, Dictionary<string, object?> context)
    {
        var result = template;

        foreach (var kvp in context)
        {
            result = result
                .Replace($"[{kvp.Key}]", kvp.Value?.ToString() ?? "")
                .Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? "");
        }

        return result;
    }

    private void RegisterBuiltInFunctions()
    {
        // تابع امروز
        RegisterFunction("today", args => DateTime.Today);

        // تابع الان
        RegisterFunction("now", args => DateTime.Now);

        // تابع حداقل
        RegisterFunction("min", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return args.Where(a => a != null).Select(Convert.ToDecimal).Min();
        });

        // تابع حداکثر
        RegisterFunction("max", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return args.Where(a => a != null).Select(Convert.ToDecimal).Max();
        });

        // تابع مجموع
        RegisterFunction("sum", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return args.Where(a => a != null).Select(Convert.ToDecimal).Sum();
        });

        // تابع میانگین
        RegisterFunction("avg", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return args.Where(a => a != null).Select(Convert.ToDecimal).Average();
        });

        // تابع گرد کردن
        RegisterFunction("round", args =>
        {
            if (args == null || args.Length == 0) return 0;
            var value = Convert.ToDecimal(args[0]);
            var decimals = args.Length > 1 ? Convert.ToInt32(args[1]) : 0;
            return Math.Round(value, decimals);
        });

        // تابع قدر مطلق
        RegisterFunction("abs", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return Math.Abs(Convert.ToDecimal(args[0]));
        });

        // تابع بررسی خالی بودن
        RegisterFunction("isempty", args =>
        {
            if (args == null || args.Length == 0) return true;
            return args[0] == null || string.IsNullOrWhiteSpace(args[0].ToString());
        });

        // تابع بررسی عدد بودن
        RegisterFunction("isnumber", args =>
        {
            if (args == null || args.Length == 0) return false;
            return decimal.TryParse(args[0]?.ToString(), out _);
        });

        // تابع تبدیل به عدد
        RegisterFunction("tonumber", args =>
        {
            if (args == null || args.Length == 0) return 0;
            return decimal.TryParse(args[0]?.ToString(), out var result) ? result : 0;
        });

        // تابع شرطی
        RegisterFunction("iif", args =>
        {
            if (args == null || args.Length < 3) return null;
            var condition = Convert.ToBoolean(args[0]);
            return condition ? args[1] : args[2];
        });

        // تابع محاسبه پیپ (برای فارکس)
        RegisterFunction("pips", args =>
        {
            if (args == null || args.Length < 2) return 0;
            var price1 = Convert.ToDecimal(args[0]);
            var price2 = Convert.ToDecimal(args[1]);
            var pipSize = args.Length > 2 ? Convert.ToDecimal(args[2]) : 0.0001m;
            return Math.Abs(price1 - price2) / pipSize;
        });

        // تابع محاسبه سود/زیان
        RegisterFunction("profit", args =>
        {
            if (args == null || args.Length < 4) return 0;
            var entryPrice = Convert.ToDecimal(args[0]);
            var exitPrice = Convert.ToDecimal(args[1]);
            var volume = Convert.ToDecimal(args[2]);
            var direction = Convert.ToInt32(args[3]); // 1 = Buy, 2 = Sell
            
            var priceDiff = direction == 1 ? exitPrice - entryPrice : entryPrice - exitPrice;
            return priceDiff * volume * 100000; // Standard lot
        });

        // تابع محاسبه درصد
        RegisterFunction("percent", args =>
        {
            if (args == null || args.Length < 2) return 0;
            var value = Convert.ToDecimal(args[0]);
            var total = Convert.ToDecimal(args[1]);
            if (total == 0) return 0;
            return (value / total) * 100;
        });

        // تابع محاسبه R:R
        RegisterFunction("rr", args =>
        {
            if (args == null || args.Length < 3) return 0;
            var entry = Convert.ToDecimal(args[0]);
            var sl = Convert.ToDecimal(args[1]);
            var tp = Convert.ToDecimal(args[2]);
            
            var risk = Math.Abs(entry - sl);
            if (risk == 0) return 0;
            var reward = Math.Abs(tp - entry);
            return Math.Round(reward / risk, 2);
        });
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/RuleEngine.cs
// =============================================================================