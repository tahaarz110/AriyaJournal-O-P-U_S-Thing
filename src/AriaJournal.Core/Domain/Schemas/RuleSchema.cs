// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Schemas/RuleSchema.cs
// توضیح: تعریف قوانین فرم - نسخه کامل
// =============================================================================

namespace AriaJournal.Core.Domain.Schemas;

/// <summary>
/// تعریف یک قانون برای فرم
/// </summary>
public class RuleSchema
{
    /// <summary>
    /// شناسه قانون
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// نوع Trigger (OnLoad, OnChange, OnSave, OnValidate)
    /// </summary>
    public string Trigger { get; set; } = "OnChange";

    /// <summary>
    /// وابستگی به فیلد خاص
    /// </summary>
    public string? DependsOn { get; set; }

    /// <summary>
    /// شرط اجرای قانون (Expression)
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// نوع عمل (Show, Hide, Enable, Disable, Validate, Calculate, SetValue)
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// هدف عمل (شناسه فیلد)
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// مقدار یا پیام
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// اولویت اجرا (کمتر = زودتر)
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// آیا قانون فعال است؟
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// گروه قانون
    /// </summary>
    public string? Group { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Schemas/RuleSchema.cs
// =============================================================================