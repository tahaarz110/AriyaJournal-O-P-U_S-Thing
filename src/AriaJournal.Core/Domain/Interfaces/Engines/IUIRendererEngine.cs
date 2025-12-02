// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IUIRendererEngine.cs
// شماره فایل: 24
// =============================================================================

using System.Windows;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور رندر UI از Schema
/// </summary>
public interface IUIRendererEngine
{
    /// <summary>
    /// رندر فرم با شناسه
    /// </summary>
    FrameworkElement RenderForm(string formId);

    /// <summary>
    /// رندر فرم از Schema
    /// </summary>
    FrameworkElement RenderForm(FormSchema schema);

    /// <summary>
    /// رندر یک فیلد
    /// </summary>
    FrameworkElement RenderField(FieldSchema field);

    /// <summary>
    /// اتصال داده به فرم
    /// </summary>
    void BindData(FrameworkElement form, object data);

    /// <summary>
    /// استخراج داده از فرم
    /// </summary>
    Dictionary<string, object?> ExtractData(FrameworkElement form);

    /// <summary>
    /// اعتبارسنجی فرم
    /// </summary>
    Result<bool> ValidateForm(FrameworkElement form);

    /// <summary>
    /// دریافت مقدار یک فیلد
    /// </summary>
    object? GetFieldValue(FrameworkElement form, string fieldId);

    /// <summary>
    /// تنظیم مقدار یک فیلد
    /// </summary>
    void SetFieldValue(FrameworkElement form, string fieldId, object? value);

    /// <summary>
    /// نمایش/مخفی کردن فیلد
    /// </summary>
    void SetFieldVisibility(FrameworkElement form, string fieldId, bool visible);

    /// <summary>
    /// فعال/غیرفعال کردن فیلد
    /// </summary>
    void SetFieldEnabled(FrameworkElement form, string fieldId, bool enabled);

    /// <summary>
    /// نمایش خطا روی فیلد
    /// </summary>
    void ShowFieldError(FrameworkElement form, string fieldId, string message);

    /// <summary>
    /// پاک کردن خطاها
    /// </summary>
    void ClearErrors(FrameworkElement form);
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IUIRendererEngine.cs
// =============================================================================