// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/IMetadataService.cs
// توضیح: اینترفیس سرویس مدیریت متادیتا - نسخه کامل
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// سرویس مدیریت متادیتا و سفارشی‌سازی
/// </summary>
public interface IMetadataService
{
    #region Schema Operations

    /// <summary>
    /// دریافت Schema یک ماژول
    /// </summary>
    Task<Result<SchemaDefinition>> GetSchemaAsync(string module);

    /// <summary>
    /// دریافت فرم با سفارشی‌سازی کاربر
    /// </summary>
    Task<Result<FormSchema>> GetCustomizedFormAsync(int userId, string formId);

    /// <summary>
    /// دریافت جدول با سفارشی‌سازی کاربر
    /// </summary>
    Task<Result<TableSchema>> GetCustomizedTableAsync(int userId, string tableId);

    /// <summary>
    /// دریافت لیست فرم‌های موجود
    /// </summary>
    Task<Result<List<FormSchema>>> GetAllFormsAsync();

    /// <summary>
    /// دریافت لیست جداول موجود
    /// </summary>
    Task<Result<List<TableSchema>>> GetAllTablesAsync();

    #endregion

    #region Field Customization

    /// <summary>
    /// دریافت سفارشی‌سازی فیلدهای کاربر
    /// </summary>
    Task<Result<List<UserFieldCustomization>>> GetFieldCustomizationsAsync(int userId, string formId);

    /// <summary>
    /// ذخیره سفارشی‌سازی فیلد
    /// </summary>
    Task<Result<bool>> SaveFieldCustomizationAsync(UserFieldCustomization customization);

    /// <summary>
    /// ذخیره چندین سفارشی‌سازی فیلد
    /// </summary>
    Task<Result<bool>> SaveFieldCustomizationsAsync(int userId, string formId, List<UserFieldCustomization> customizations);

    /// <summary>
    /// بازنشانی سفارشی‌سازی فیلدها به پیش‌فرض
    /// </summary>
    Task<Result<bool>> ResetFieldCustomizationsAsync(int userId, string formId);

    /// <summary>
    /// دریافت فیلدهای پیش‌فرض یک فرم
    /// </summary>
    Task<Result<List<FieldSchema>>> GetDefaultFieldsAsync(string formId);

    #endregion

    #region Column Customization

    /// <summary>
    /// دریافت سفارشی‌سازی ستون‌های کاربر
    /// </summary>
    Task<Result<List<UserColumnCustomization>>> GetColumnCustomizationsAsync(int userId, string tableId);

    /// <summary>
    /// ذخیره سفارشی‌سازی ستون
    /// </summary>
    Task<Result<bool>> SaveColumnCustomizationAsync(UserColumnCustomization customization);

    /// <summary>
    /// ذخیره چندین سفارشی‌سازی ستون
    /// </summary>
    Task<Result<bool>> SaveColumnCustomizationsAsync(int userId, string tableId, List<UserColumnCustomization> customizations);

    /// <summary>
    /// بازنشانی سفارشی‌سازی ستون‌ها به پیش‌فرض
    /// </summary>
    Task<Result<bool>> ResetColumnCustomizationsAsync(int userId, string tableId);

    /// <summary>
    /// دریافت ستون‌های پیش‌فرض یک جدول
    /// </summary>
    Task<Result<List<ColumnSchema>>> GetDefaultColumnsAsync(string tableId);

    #endregion

    #region User Defined Fields

    /// <summary>
    /// دریافت فیلدهای سفارشی کاربر
    /// </summary>
    Task<Result<List<UserDefinedField>>> GetUserDefinedFieldsAsync(int userId, string entityType);

    /// <summary>
    /// ایجاد فیلد سفارشی
    /// </summary>
    Task<Result<UserDefinedField>> CreateUserDefinedFieldAsync(UserDefinedField field);

    /// <summary>
    /// ویرایش فیلد سفارشی
    /// </summary>
    Task<Result<bool>> UpdateUserDefinedFieldAsync(UserDefinedField field);

    /// <summary>
    /// حذف فیلد سفارشی
    /// </summary>
    Task<Result<bool>> DeleteUserDefinedFieldAsync(int fieldId);

    /// <summary>
    /// دریافت مقدار فیلد سفارشی برای یک رکورد
    /// </summary>
    Task<Result<string?>> GetCustomFieldValueAsync(int userId, string entityType, int entityId, string fieldName);

    /// <summary>
    /// ذخیره مقدار فیلد سفارشی
    /// </summary>
    Task<Result<bool>> SaveCustomFieldValueAsync(int userId, string entityType, int entityId, string fieldName, string? value);

    #endregion

    #region Widget Customization

    /// <summary>
    /// دریافت سفارشی‌سازی ویجت‌های کاربر
    /// </summary>
    Task<Result<List<UserWidgetCustomization>>> GetWidgetCustomizationsAsync(int userId, string dashboardId);

    /// <summary>
    /// ذخیره سفارشی‌سازی ویجت
    /// </summary>
    Task<Result<bool>> SaveWidgetCustomizationAsync(UserWidgetCustomization customization);

    /// <summary>
    /// ذخیره چندین سفارشی‌سازی ویجت
    /// </summary>
    Task<Result<bool>> SaveWidgetCustomizationsAsync(int userId, string dashboardId, List<UserWidgetCustomization> customizations);

    /// <summary>
    /// بازنشانی سفارشی‌سازی ویجت‌ها
    /// </summary>
    Task<Result<bool>> ResetWidgetCustomizationsAsync(int userId, string dashboardId);

    #endregion

    #region Presets

    /// <summary>
    /// دریافت پیش‌تنظیم‌های فرم
    /// </summary>
    Task<Result<List<FormPreset>>> GetFormPresetsAsync(int userId, string formId);

    /// <summary>
    /// ذخیره پیش‌تنظیم
    /// </summary>
    Task<Result<FormPreset>> SaveFormPresetAsync(FormPreset preset);

    /// <summary>
    /// حذف پیش‌تنظیم
    /// </summary>
    Task<Result<bool>> DeleteFormPresetAsync(int presetId);

    /// <summary>
    /// تنظیم پیش‌تنظیم پیش‌فرض
    /// </summary>
    Task<Result<bool>> SetDefaultPresetAsync(int userId, string formId, int presetId);

    /// <summary>
    /// دریافت پیش‌تنظیم پیش‌فرض
    /// </summary>
    Task<Result<FormPreset?>> GetDefaultPresetAsync(int userId, string formId);

    #endregion

    #region Saved Filters

    /// <summary>
    /// دریافت فیلترهای ذخیره‌شده
    /// </summary>
    Task<Result<List<SavedFilter>>> GetSavedFiltersAsync(int userId, string tableId);

    /// <summary>
    /// ذخیره فیلتر
    /// </summary>
    Task<Result<SavedFilter>> SaveFilterAsync(SavedFilter filter);

    /// <summary>
    /// حذف فیلتر
    /// </summary>
    Task<Result<bool>> DeleteFilterAsync(int filterId);

    /// <summary>
    /// تنظیم فیلتر پیش‌فرض
    /// </summary>
    Task<Result<bool>> SetDefaultFilterAsync(int userId, string tableId, int filterId);

    /// <summary>
    /// دریافت فیلتر پیش‌فرض
    /// </summary>
    Task<Result<SavedFilter?>> GetDefaultFilterAsync(int userId, string tableId);

    #endregion

    #region Import/Export

    /// <summary>
    /// صادر کردن تنظیمات کاربر
    /// </summary>
    Task<Result<string>> ExportUserSettingsAsync(int userId);

    /// <summary>
    /// وارد کردن تنظیمات کاربر
    /// </summary>
    Task<Result<bool>> ImportUserSettingsAsync(int userId, string settingsJson);

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/IMetadataService.cs
// =============================================================================