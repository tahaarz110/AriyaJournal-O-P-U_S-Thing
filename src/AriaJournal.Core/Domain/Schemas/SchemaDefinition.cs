// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Schemas/SchemaDefinition.cs
// توضیح: تعریف کلی یک Schema - اصلاح‌شده
// =============================================================================

using System.Collections.Generic;

namespace AriaJournal.Core.Domain.Schemas;

/// <summary>
/// تعریف کلی یک Schema
/// </summary>
public class SchemaDefinition
{
    /// <summary>
    /// نام ماژول
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// نسخه
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// لیست فرم‌ها
    /// </summary>
    public List<FormSchema> Forms { get; set; } = new();

    /// <summary>
    /// لیست جداول
    /// </summary>
    public List<TableSchema> Tables { get; set; } = new();

    /// <summary>
    /// لیست ویجت‌ها
    /// </summary>
    public List<WidgetSchema> Widgets { get; set; } = new();
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Schemas/SchemaDefinition.cs
// =============================================================================