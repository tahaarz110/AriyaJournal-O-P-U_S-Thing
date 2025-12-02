// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ISchemaEngine.cs
// شماره فایل: 23
// =============================================================================

using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور مدیریت Schema ها
/// </summary>
public interface ISchemaEngine
{
    /// <summary>
    /// راه‌اندازی اولیه و خواندن Schema ها
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// بارگذاری Schema از فایل
    /// </summary>
    Task<Result<SchemaDefinition>> LoadAsync(string path);

    /// <summary>
    /// دریافت Schema یک ماژول
    /// </summary>
    SchemaDefinition? GetSchema(string module);

    /// <summary>
    /// دریافت فرم با شناسه
    /// </summary>
    FormSchema? GetForm(string formId);

    /// <summary>
    /// دریافت فیلدهای یک فرم
    /// </summary>
    List<FieldSchema> GetFields(string formId);

    /// <summary>
    /// دریافت قوانین یک فرم
    /// </summary>
    List<RuleSchema> GetRules(string formId);

    /// <summary>
    /// ثبت Schema توسط پلاگین
    /// </summary>
    Task<Result<bool>> RegisterSchemaAsync(SchemaDefinition schema);

    /// <summary>
    /// حذف Schema یک ماژول
    /// </summary>
    void UnregisterSchema(string module);

    /// <summary>
    /// بارگذاری مجدد همه Schema ها
    /// </summary>
    Task ReloadAllAsync();

    /// <summary>
    /// لیست ماژول‌های ثبت‌شده
    /// </summary>
    IEnumerable<string> GetRegisteredModules();
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ISchemaEngine.cs
// =============================================================================