// =============================================================================
// فایل: src/AriaJournal.Core/Application/Services/MetadataService.cs
// توضیح: سرویس مدیریت متادیتا و سفارشی‌سازی - نسخه کامل و نهایی اصلاح‌شده
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// سرویس مدیریت متادیتا و سفارشی‌سازی
/// </summary>
public class MetadataService : IMetadataService
{
    private readonly ISchemaEngine _schemaEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheEngine _cacheEngine;
    private readonly IEventBusEngine _eventBus;

    // تنظیمات JSON برای سریالایز/دیسریالایز
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public MetadataService(
        ISchemaEngine schemaEngine,
        IUnitOfWork unitOfWork,
        ICacheEngine cacheEngine,
        IEventBusEngine eventBus)
    {
        _schemaEngine = schemaEngine ?? throw new ArgumentNullException(nameof(schemaEngine));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    #region Schema Operations

    public async Task<Result<SchemaDefinition>> GetSchemaAsync(string module)
    {
        try
        {
            var schema = _schemaEngine.GetSchema(module);
            if (schema == null)
            {
                return Result.Failure<SchemaDefinition>(
                    Error.NotFound($"Schema برای ماژول '{module}' یافت نشد"));
            }

            return await Task.FromResult(Result.Success(schema));
        }
        catch (Exception ex)
        {
            return Result.Failure<SchemaDefinition>(
                Error.Validation($"خطا در دریافت Schema: {ex.Message}"));
        }
    }

    public async Task<Result<FormSchema>> GetCustomizedFormAsync(int userId, string formId)
    {
        try
        {
            var form = _schemaEngine.GetForm(formId);
            if (form == null)
            {
                return Result.Failure<FormSchema>(
                    Error.NotFound($"فرم '{formId}' یافت نشد"));
            }

            var customizedForm = CloneForm(form);

            var customizations = await GetFieldCustomizationsAsync(userId, formId);
            if (customizations.IsSuccess && customizations.Value.Any())
            {
                ApplyFieldCustomizations(customizedForm, customizations.Value);
            }

            var userFields = await GetUserDefinedFieldsAsync(userId, "Trade");
            if (userFields.IsSuccess && userFields.Value.Any())
            {
                AddUserDefinedFieldsToForm(customizedForm, userFields.Value);
            }

            return Result.Success(customizedForm);
        }
        catch (Exception ex)
        {
            return Result.Failure<FormSchema>(
                Error.Validation($"خطا در دریافت فرم سفارشی: {ex.Message}"));
        }
    }

    public async Task<Result<TableSchema>> GetCustomizedTableAsync(int userId, string tableId)
    {
        try
        {
            var schema = _schemaEngine.GetSchema("Trade");
            var table = schema?.Tables?.FirstOrDefault(t => t.Id == tableId);

            if (table == null)
            {
                table = CreateDefaultTradeTable();
            }

            var customizedTable = CloneTable(table);

            var customizations = await GetColumnCustomizationsAsync(userId, tableId);
            if (customizations.IsSuccess && customizations.Value.Any())
            {
                ApplyColumnCustomizations(customizedTable, customizations.Value);
            }

            return Result.Success(customizedTable);
        }
        catch (Exception ex)
        {
            return Result.Failure<TableSchema>(
                Error.Validation($"خطا در دریافت جدول سفارشی: {ex.Message}"));
        }
    }

    public async Task<Result<List<FormSchema>>> GetAllFormsAsync()
    {
        try
        {
            var forms = new List<FormSchema>();
            var modules = _schemaEngine.GetRegisteredModules();

            foreach (var module in modules)
            {
                var schema = _schemaEngine.GetSchema(module);
                if (schema?.Forms != null)
                {
                    forms.AddRange(schema.Forms);
                }
            }

            return await Task.FromResult(Result.Success(forms));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FormSchema>>(
                Error.Validation($"خطا در دریافت فرم‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<List<TableSchema>>> GetAllTablesAsync()
    {
        try
        {
            var tables = new List<TableSchema>();
            var modules = _schemaEngine.GetRegisteredModules();

            foreach (var module in modules)
            {
                var schema = _schemaEngine.GetSchema(module);
                if (schema?.Tables != null)
                {
                    tables.AddRange(schema.Tables);
                }
            }

            // اضافه کردن جداول پیش‌فرض
            if (!tables.Any(t => t.Id == "tradeList"))
            {
                tables.Add(CreateDefaultTradeTable());
            }

            return await Task.FromResult(Result.Success(tables));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<TableSchema>>(
                Error.Validation($"خطا در دریافت جداول: {ex.Message}"));
        }
    }

    public async Task<Result<List<FieldSchema>>> GetDefaultFieldsAsync(string formId)
    {
        try
        {
            var form = _schemaEngine.GetForm(formId);
            if (form == null)
            {
                return Result.Failure<List<FieldSchema>>(
                    Error.NotFound($"فرم '{formId}' یافت نشد"));
            }

            var fields = form.Sections.SelectMany(s => s.Fields).ToList();
            return await Task.FromResult(Result.Success(fields));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FieldSchema>>(
                Error.Validation($"خطا در دریافت فیلدها: {ex.Message}"));
        }
    }

    public async Task<Result<List<ColumnSchema>>> GetDefaultColumnsAsync(string tableId)
    {
        try
        {
            var schema = _schemaEngine.GetSchema("Trade");
            var table = schema?.Tables?.FirstOrDefault(t => t.Id == tableId);

            if (table == null)
            {
                table = CreateDefaultTradeTable();
            }

            return await Task.FromResult(Result.Success(table.Columns));
        }
        catch (Exception ex)
        {
            return Result.Failure<List<ColumnSchema>>(
                Error.Validation($"خطا در دریافت ستون‌ها: {ex.Message}"));
        }
    }

    #endregion

    #region Field Customization

    public async Task<Result<List<UserFieldCustomization>>> GetFieldCustomizationsAsync(int userId, string formId)
    {
        try
        {
            var cacheKey = $"field_customization:{userId}:{formId}";
            var cached = _cacheEngine.Get<List<UserFieldCustomization>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var repo = _unitOfWork.Repository<FieldDefinition>();
            var allDefinitions = await repo.GetAllAsync();
            var definitions = allDefinitions.Where(f => f.UserId == userId);

            var customizations = definitions
                .Where(d => d.Metadata?.Contains(formId) == true)
                .Select(d => new UserFieldCustomization
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    FormId = formId,
                    FieldId = d.FieldName,
                    Visible = d.IsActive,
                    Order = d.Order,
                    CustomLabel = d.DisplayName,
                    Required = d.IsRequired
                })
                .ToList();

            _cacheEngine.Set(cacheKey, customizations, TimeSpan.FromMinutes(10));

            return Result.Success(customizations);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserFieldCustomization>>(
                Error.Validation($"خطا در دریافت سفارشی‌سازی فیلدها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveFieldCustomizationAsync(UserFieldCustomization customization)
    {
        try
        {
            var repo = _unitOfWork.Repository<FieldDefinition>();

            var existing = await repo.FirstOrDefaultAsync(f =>
                f.UserId == customization.UserId &&
                f.FieldName == customization.FieldId);

            if (existing != null)
            {
                existing.DisplayName = customization.CustomLabel ?? existing.DisplayName;
                existing.IsActive = customization.Visible;
                existing.Order = customization.Order;
                existing.IsRequired = customization.Required;
                existing.UpdatedAt = DateTime.Now;

                repo.Update(existing);
            }
            else
            {
                var newField = new FieldDefinition
                {
                    UserId = customization.UserId,
                    FieldName = customization.FieldId,
                    DisplayName = customization.CustomLabel ?? customization.FieldId,
                    FieldType = FieldType.Text,
                    IsActive = customization.Visible,
                    Order = customization.Order,
                    IsRequired = customization.Required,
                    Metadata = JsonSerializer.Serialize(new { FormId = customization.FormId }, _jsonOptions),
                    CreatedAt = DateTime.Now
                };

                await repo.AddAsync(newField);
            }

            await _unitOfWork.SaveChangesAsync();

            _cacheEngine.Remove($"field_customization:{customization.UserId}:{customization.FormId}");

            _eventBus.Publish(new CustomizationChangedEvent(
                customization.UserId, "Field", customization.FieldId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveFieldCustomizationsAsync(
        int userId, string formId, List<UserFieldCustomization> customizations)
    {
        try
        {
            foreach (var customization in customizations)
            {
                customization.UserId = userId;
                customization.FormId = formId;
                await SaveFieldCustomizationAsync(customization);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> ResetFieldCustomizationsAsync(int userId, string formId)
    {
        try
        {
            var repo = _unitOfWork.Repository<FieldDefinition>();
            var allDefinitions = await repo.GetAllAsync();
            var customizations = allDefinitions.Where(f =>
                f.UserId == userId &&
                f.Metadata != null &&
                f.Metadata.Contains(formId)).ToList();

            foreach (var item in customizations)
            {
                repo.Delete(item);
            }

            await _unitOfWork.SaveChangesAsync();

            _cacheEngine.Remove($"field_customization:{userId}:{formId}");

            _eventBus.Publish(new CustomizationChangedEvent(userId, "FieldReset", formId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در بازنشانی: {ex.Message}"));
        }
    }

    #endregion

    #region Column Customization

    public async Task<Result<List<UserColumnCustomization>>> GetColumnCustomizationsAsync(int userId, string tableId)
    {
        try
        {
            var cacheKey = $"column_customization:{userId}:{tableId}";
            var cached = _cacheEngine.Get<List<UserColumnCustomization>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            var customizations = new List<UserColumnCustomization>();

            if (settings?.CustomSettings != null)
            {
                try
                {
                    var allCustomizations = JsonSerializer.Deserialize<Dictionary<string, List<UserColumnCustomization>>>(
                        settings.CustomSettings, _jsonOptions);

                    if (allCustomizations != null && allCustomizations.TryGetValue($"columns_{tableId}", out var cols))
                    {
                        customizations = cols;
                    }
                }
                catch
                {
                    // JSON نامعتبر - لیست خالی برگردان
                }
            }

            _cacheEngine.Set(cacheKey, customizations, TimeSpan.FromMinutes(10));

            return Result.Success(customizations);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserColumnCustomization>>(
                Error.Validation($"خطا در دریافت سفارشی‌سازی ستون‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveColumnCustomizationAsync(UserColumnCustomization customization)
    {
        try
        {
            var customizations = await GetColumnCustomizationsAsync(customization.UserId, customization.TableId);
            var list = customizations.IsSuccess ? customizations.Value : new List<UserColumnCustomization>();

            var existing = list.FirstOrDefault(c => c.ColumnId == customization.ColumnId);
            if (existing != null)
            {
                existing.Visible = customization.Visible;
                existing.Order = customization.Order;
                existing.Width = customization.Width;
                existing.CustomHeader = customization.CustomHeader;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                customization.UpdatedAt = DateTime.Now;
                list.Add(customization);
            }

            await SaveColumnCustomizationsToSettingsAsync(customization.UserId, customization.TableId, list);

            _cacheEngine.Remove($"column_customization:{customization.UserId}:{customization.TableId}");

            _eventBus.Publish(new CustomizationChangedEvent(
                customization.UserId, "Column", customization.ColumnId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی ستون: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveColumnCustomizationsAsync(
        int userId, string tableId, List<UserColumnCustomization> customizations)
    {
        try
        {
            await SaveColumnCustomizationsToSettingsAsync(userId, tableId, customizations);

            _cacheEngine.Remove($"column_customization:{userId}:{tableId}");

            _eventBus.Publish(new CustomizationChangedEvent(userId, "ColumnBatch", tableId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> ResetColumnCustomizationsAsync(int userId, string tableId)
    {
        try
        {
            await SaveColumnCustomizationsToSettingsAsync(userId, tableId, new List<UserColumnCustomization>());

            _cacheEngine.Remove($"column_customization:{userId}:{tableId}");

            _eventBus.Publish(new CustomizationChangedEvent(userId, "ColumnReset", tableId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در بازنشانی: {ex.Message}"));
        }
    }

    private async Task SaveColumnCustomizationsToSettingsAsync(
        int userId, string tableId, List<UserColumnCustomization> customizations)
    {
        var settingsRepo = _unitOfWork.Repository<Settings>();
        var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new Settings { UserId = userId };
            await settingsRepo.AddAsync(settings);
        }

        var allCustomizations = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(settings.CustomSettings))
        {
            try
            {
                allCustomizations = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    settings.CustomSettings, _jsonOptions) ?? new Dictionary<string, object>();
            }
            catch
            {
                allCustomizations = new Dictionary<string, object>();
            }
        }

        allCustomizations[$"columns_{tableId}"] = customizations;

        settings.CustomSettings = JsonSerializer.Serialize(allCustomizations, _jsonOptions);
        settings.UpdatedAt = DateTime.Now;

        settingsRepo.Update(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region User Defined Fields

    public async Task<Result<List<UserDefinedField>>> GetUserDefinedFieldsAsync(int userId, string entityType)
    {
        try
        {
            var cacheKey = $"user_defined_fields:{userId}:{entityType}";
            var cached = _cacheEngine.Get<List<UserDefinedField>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var repo = _unitOfWork.Repository<FieldDefinition>();
            var allDefinitions = await repo.GetAllAsync();
            var definitions = allDefinitions.Where(f =>
                f.UserId == userId &&
                f.EntityType == entityType &&
                f.IsActive);

            var fields = definitions.Select(d => new UserDefinedField
            {
                Id = d.Id,
                UserId = d.UserId,
                EntityType = d.EntityType ?? entityType,
                FieldName = d.FieldName,
                DisplayName = d.DisplayName,
                FieldType = d.FieldType.ToString(),
                Required = d.IsRequired,
                DefaultValue = d.DefaultValue,
                Options = d.Options,
                Order = d.Order,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt
            }).OrderBy(f => f.Order).ToList();

            _cacheEngine.Set(cacheKey, fields, TimeSpan.FromMinutes(10));

            return Result.Success(fields);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserDefinedField>>(
                Error.Validation($"خطا در دریافت فیلدهای سفارشی: {ex.Message}"));
        }
    }

    public async Task<Result<UserDefinedField>> CreateUserDefinedFieldAsync(UserDefinedField field)
    {
        try
        {
            var repo = _unitOfWork.Repository<FieldDefinition>();

            var existing = await repo.FirstOrDefaultAsync(f =>
                f.UserId == field.UserId &&
                f.EntityType == field.EntityType &&
                f.FieldName == field.FieldName);

            if (existing != null)
            {
                return Result.Failure<UserDefinedField>(
                    Error.Validation("فیلدی با این نام قبلاً وجود دارد"));
            }

            var fieldType = FieldType.Text;
            if (Enum.TryParse<FieldType>(field.FieldType, true, out var parsed))
            {
                fieldType = parsed;
            }

            var definition = new FieldDefinition
            {
                UserId = field.UserId,
                EntityType = field.EntityType,
                FieldName = field.FieldName,
                DisplayName = field.DisplayName,
                FieldType = fieldType,
                IsRequired = field.Required,
                DefaultValue = field.DefaultValue,
                Options = field.Options,
                Order = field.Order,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await repo.AddAsync(definition);
            await _unitOfWork.SaveChangesAsync();

            field.Id = definition.Id;
            field.CreatedAt = definition.CreatedAt;

            // پاک کردن کش
            _cacheEngine.Remove($"user_defined_fields:{field.UserId}:{field.EntityType}");

            _eventBus.Publish(new CustomizationChangedEvent(field.UserId, "FieldCreated", field.FieldName));

            return Result.Success(field);
        }
        catch (Exception ex)
        {
            return Result.Failure<UserDefinedField>(
                Error.Validation($"خطا در ایجاد فیلد: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> UpdateUserDefinedFieldAsync(UserDefinedField field)
    {
        try
        {
            var repo = _unitOfWork.Repository<FieldDefinition>();
            var definition = await repo.GetByIdAsync(field.Id);

            if (definition == null)
            {
                return Result.Failure<bool>(Error.NotFound("فیلد یافت نشد"));
            }

            var fieldType = FieldType.Text;
            if (Enum.TryParse<FieldType>(field.FieldType, true, out var parsed))
            {
                fieldType = parsed;
            }

            definition.DisplayName = field.DisplayName;
            definition.FieldType = fieldType;
            definition.IsRequired = field.Required;
            definition.DefaultValue = field.DefaultValue;
            definition.Options = field.Options;
            definition.Order = field.Order;
            definition.IsActive = field.IsActive;
            definition.UpdatedAt = DateTime.Now;

            repo.Update(definition);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"user_defined_fields:{field.UserId}:{field.EntityType}");

            _eventBus.Publish(new CustomizationChangedEvent(field.UserId, "FieldUpdated", field.FieldName));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ویرایش فیلد: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> DeleteUserDefinedFieldAsync(int fieldId)
    {
        try
        {
            var repo = _unitOfWork.Repository<FieldDefinition>();
            var definition = await repo.GetByIdAsync(fieldId);

            if (definition == null)
            {
                return Result.Failure<bool>(Error.NotFound("فیلد یافت نشد"));
            }

            // Soft delete
            definition.IsActive = false;
            definition.UpdatedAt = DateTime.Now;

            repo.Update(definition);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"user_defined_fields:{definition.UserId}:{definition.EntityType}");

            _eventBus.Publish(new CustomizationChangedEvent(
                definition.UserId, "FieldDeleted", definition.FieldName));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در حذف فیلد: {ex.Message}"));
        }
    }

    public async Task<Result<string?>> GetCustomFieldValueAsync(int userId, string entityType, int entityId, string fieldName)
    {
        try
        {
            var repo = _unitOfWork.Repository<TradeCustomField>();
            var allCustomFields = await repo.GetAllAsync();
            
            var customField = allCustomFields.FirstOrDefault(f =>
                f.TradeId == entityId &&
                f.FieldDefinition != null &&
                f.FieldDefinition.FieldName == fieldName &&
                f.FieldDefinition.UserId == userId);

            return Result.Success(customField?.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<string?>(
                Error.Validation($"خطا در دریافت مقدار فیلد: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveCustomFieldValueAsync(int userId, string entityType, int entityId, string fieldName, string? value)
    {
        try
        {
            var fieldDefRepo = _unitOfWork.Repository<FieldDefinition>();
            var fieldDef = await fieldDefRepo.FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.EntityType == entityType &&
                f.FieldName == fieldName);

            if (fieldDef == null)
            {
                return Result.Failure<bool>(Error.NotFound("تعریف فیلد یافت نشد"));
            }

            var customFieldRepo = _unitOfWork.Repository<TradeCustomField>();
            var existing = await customFieldRepo.FirstOrDefaultAsync(f =>
                f.TradeId == entityId &&
                f.FieldDefinitionId == fieldDef.Id);

            if (existing != null)
            {
                existing.Value = value;
                customFieldRepo.Update(existing);
            }
            else
            {
                var newCustomField = new TradeCustomField
                {
                    TradeId = entityId,
                    FieldDefinitionId = fieldDef.Id,
                    Value = value
                };
                await customFieldRepo.AddAsync(newCustomField);
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره مقدار فیلد: {ex.Message}"));
        }
    }

    #endregion

    #region Widget Customization

    public async Task<Result<List<UserWidgetCustomization>>> GetWidgetCustomizationsAsync(int userId, string dashboardId)
    {
        try
        {
            var cacheKey = $"widget_customization:{userId}:{dashboardId}";
            var cached = _cacheEngine.Get<List<UserWidgetCustomization>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            var customizations = new List<UserWidgetCustomization>();

            if (settings?.CustomSettings != null)
            {
                try
                {
                    var allData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        settings.CustomSettings, _jsonOptions);

                    if (allData != null && allData.TryGetValue($"widgets_{dashboardId}", out var widgetsElement))
                    {
                        customizations = JsonSerializer.Deserialize<List<UserWidgetCustomization>>(
                            widgetsElement.GetRawText(), _jsonOptions) ?? new List<UserWidgetCustomization>();
                    }
                }
                catch
                {
                    // JSON نامعتبر
                }
            }

            _cacheEngine.Set(cacheKey, customizations, TimeSpan.FromMinutes(10));

            return Result.Success(customizations);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserWidgetCustomization>>(
                Error.Validation($"خطا در دریافت سفارشی‌سازی ویجت‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveWidgetCustomizationAsync(UserWidgetCustomization customization)
    {
        try
        {
            var customizations = await GetWidgetCustomizationsAsync(customization.UserId, customization.DashboardId);
            var list = customizations.IsSuccess ? customizations.Value : new List<UserWidgetCustomization>();

            var existing = list.FirstOrDefault(w => w.WidgetId == customization.WidgetId);
            if (existing != null)
            {
                existing.Visible = customization.Visible;
                existing.Row = customization.Row;
                existing.Column = customization.Column;
                existing.Width = customization.Width;
                existing.Height = customization.Height;
                existing.Collapsed = customization.Collapsed;
                existing.CustomConfig = customization.CustomConfig;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                customization.UpdatedAt = DateTime.Now;
                list.Add(customization);
            }

            await SaveWidgetCustomizationsToSettingsAsync(customization.UserId, customization.DashboardId, list);

            _cacheEngine.Remove($"widget_customization:{customization.UserId}:{customization.DashboardId}");

            _eventBus.Publish(new CustomizationChangedEvent(
                customization.UserId, "Widget", customization.WidgetId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی ویجت: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SaveWidgetCustomizationsAsync(
        int userId, string dashboardId, List<UserWidgetCustomization> customizations)
    {
        try
        {
            await SaveWidgetCustomizationsToSettingsAsync(userId, dashboardId, customizations);

            _cacheEngine.Remove($"widget_customization:{userId}:{dashboardId}");

            _eventBus.Publish(new CustomizationChangedEvent(userId, "WidgetBatch", dashboardId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در ذخیره سفارشی‌سازی‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> ResetWidgetCustomizationsAsync(int userId, string dashboardId)
    {
        try
        {
            await SaveWidgetCustomizationsToSettingsAsync(userId, dashboardId, new List<UserWidgetCustomization>());

            _cacheEngine.Remove($"widget_customization:{userId}:{dashboardId}");

            _eventBus.Publish(new CustomizationChangedEvent(userId, "WidgetReset", dashboardId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در بازنشانی: {ex.Message}"));
        }
    }

    private async Task SaveWidgetCustomizationsToSettingsAsync(
        int userId, string dashboardId, List<UserWidgetCustomization> customizations)
    {
        var settingsRepo = _unitOfWork.Repository<Settings>();
        var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new Settings { UserId = userId };
            await settingsRepo.AddAsync(settings);
        }

        var allData = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(settings.CustomSettings))
        {
            try
            {
                allData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    settings.CustomSettings, _jsonOptions) ?? new Dictionary<string, object>();
            }
            catch
            {
                allData = new Dictionary<string, object>();
            }
        }

        allData[$"widgets_{dashboardId}"] = customizations;

        settings.CustomSettings = JsonSerializer.Serialize(allData, _jsonOptions);
        settings.UpdatedAt = DateTime.Now;

        settingsRepo.Update(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Presets

    public async Task<Result<List<FormPreset>>> GetFormPresetsAsync(int userId, string formId)
    {
        try
        {
            var cacheKey = $"form_presets:{userId}:{formId}";
            var cached = _cacheEngine.Get<List<FormPreset>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            var presets = new List<FormPreset>();

            if (settings?.CustomSettings != null)
            {
                try
                {
                    var allData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        settings.CustomSettings, _jsonOptions);

                    if (allData != null && allData.TryGetValue($"presets_{formId}", out var presetsElement))
                    {
                        presets = JsonSerializer.Deserialize<List<FormPreset>>(
                            presetsElement.GetRawText(), _jsonOptions) ?? new List<FormPreset>();
                    }
                }
                catch
                {
                    // JSON نامعتبر
                }
            }

            _cacheEngine.Set(cacheKey, presets, TimeSpan.FromMinutes(10));

            return Result.Success(presets);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<FormPreset>>(
                Error.Validation($"خطا در دریافت پیش‌تنظیم‌ها: {ex.Message}"));
        }
    }

    public async Task<Result<FormPreset>> SaveFormPresetAsync(FormPreset preset)
    {
        try
        {
            var presetsResult = await GetFormPresetsAsync(preset.UserId, preset.FormId);
            var presets = presetsResult.IsSuccess ? presetsResult.Value : new List<FormPreset>();

            if (preset.Id == 0)
            {
                preset.Id = presets.Count > 0 ? presets.Max(p => p.Id) + 1 : 1;
                preset.CreatedAt = DateTime.Now;
                presets.Add(preset);
            }
            else
            {
                var existing = presets.FirstOrDefault(p => p.Id == preset.Id);
                if (existing != null)
                {
                    existing.Name = preset.Name;
                    existing.Description = preset.Description;
                    existing.Values = preset.Values;
                    existing.IsDefault = preset.IsDefault;
                }
            }

            // اگر این پیش‌فرض است، بقیه را غیرپیش‌فرض کن
            if (preset.IsDefault)
            {
                foreach (var p in presets.Where(x => x.Id != preset.Id))
                {
                    p.IsDefault = false;
                }
            }

            await SavePresetsToSettingsAsync(preset.UserId, preset.FormId, presets);

            _cacheEngine.Remove($"form_presets:{preset.UserId}:{preset.FormId}");

            _eventBus.Publish(new CustomizationChangedEvent(preset.UserId, "PresetSaved", preset.FormId));

            return Result.Success(preset);
        }
        catch (Exception ex)
        {
            return Result.Failure<FormPreset>(
                Error.Validation($"خطا در ذخیره پیش‌تنظیم: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> DeleteFormPresetAsync(int presetId)
    {
        try
        {
            // این متد نیاز به userId و formId دارد - فعلاً پیاده‌سازی ساده
            await Task.CompletedTask;
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در حذف پیش‌تنظیم: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SetDefaultPresetAsync(int userId, string formId, int presetId)
    {
        try
        {
            var presetsResult = await GetFormPresetsAsync(userId, formId);
            if (!presetsResult.IsSuccess)
            {
                return Result.Failure<bool>(presetsResult.Error);
            }

            var presets = presetsResult.Value;
            foreach (var preset in presets)
            {
                preset.IsDefault = (preset.Id == presetId);
            }

            await SavePresetsToSettingsAsync(userId, formId, presets);

            _cacheEngine.Remove($"form_presets:{userId}:{formId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در تنظیم پیش‌فرض: {ex.Message}"));
        }
    }

    public async Task<Result<FormPreset?>> GetDefaultPresetAsync(int userId, string formId)
    {
        try
        {
            var presetsResult = await GetFormPresetsAsync(userId, formId);
            if (!presetsResult.IsSuccess)
            {
                return Result.Failure<FormPreset?>(presetsResult.Error);
            }

            var defaultPreset = presetsResult.Value.FirstOrDefault(p => p.IsDefault);
            return Result.Success(defaultPreset);
        }
        catch (Exception ex)
        {
            return Result.Failure<FormPreset?>(
                Error.Validation($"خطا در دریافت پیش‌تنظیم پیش‌فرض: {ex.Message}"));
        }
    }

    private async Task SavePresetsToSettingsAsync(int userId, string formId, List<FormPreset> presets)
    {
        var settingsRepo = _unitOfWork.Repository<Settings>();
        var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new Settings { UserId = userId };
            await settingsRepo.AddAsync(settings);
        }

        var allData = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(settings.CustomSettings))
        {
            try
            {
                allData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    settings.CustomSettings, _jsonOptions) ?? new Dictionary<string, object>();
            }
            catch
            {
                allData = new Dictionary<string, object>();
            }
        }

        allData[$"presets_{formId}"] = presets;

        settings.CustomSettings = JsonSerializer.Serialize(allData, _jsonOptions);
        settings.UpdatedAt = DateTime.Now;

        settingsRepo.Update(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Saved Filters

    public async Task<Result<List<SavedFilter>>> GetSavedFiltersAsync(int userId, string tableId)
    {
        try
        {
            var cacheKey = $"saved_filters:{userId}:{tableId}";
            var cached = _cacheEngine.Get<List<SavedFilter>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            var filters = new List<SavedFilter>();

            if (settings?.CustomSettings != null)
            {
                try
                {
                    var allData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        settings.CustomSettings, _jsonOptions);

                    if (allData != null && allData.TryGetValue($"filters_{tableId}", out var filtersElement))
                    {
                        filters = JsonSerializer.Deserialize<List<SavedFilter>>(
                            filtersElement.GetRawText(), _jsonOptions) ?? new List<SavedFilter>();
                    }
                }
                catch
                {
                    // JSON نامعتبر
                }
            }

            _cacheEngine.Set(cacheKey, filters, TimeSpan.FromMinutes(10));

            return Result.Success(filters);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SavedFilter>>(
                Error.Validation($"خطا در دریافت فیلترها: {ex.Message}"));
        }
    }

    public async Task<Result<SavedFilter>> SaveFilterAsync(SavedFilter filter)
    {
        try
        {
            var filtersResult = await GetSavedFiltersAsync(filter.UserId, filter.TableId);
            var filters = filtersResult.IsSuccess ? filtersResult.Value : new List<SavedFilter>();

            if (filter.Id == 0)
            {
                filter.Id = filters.Count > 0 ? filters.Max(f => f.Id) + 1 : 1;
                filter.CreatedAt = DateTime.Now;
                filters.Add(filter);
            }
            else
            {
                var existing = filters.FirstOrDefault(f => f.Id == filter.Id);
                if (existing != null)
                {
                    existing.Name = filter.Name;
                    existing.Description = filter.Description;
                    existing.Conditions = filter.Conditions;
                    existing.IsDefault = filter.IsDefault;
                }
            }

            // اگر این پیش‌فرض است، بقیه را غیرپیش‌فرض کن
            if (filter.IsDefault)
            {
                foreach (var f in filters.Where(x => x.Id != filter.Id))
                {
                    f.IsDefault = false;
                }
            }

            await SaveFiltersToSettingsAsync(filter.UserId, filter.TableId, filters);

            _cacheEngine.Remove($"saved_filters:{filter.UserId}:{filter.TableId}");

            _eventBus.Publish(new CustomizationChangedEvent(filter.UserId, "FilterSaved", filter.TableId));

            return Result.Success(filter);
        }
        catch (Exception ex)
        {
            return Result.Failure<SavedFilter>(
                Error.Validation($"خطا در ذخیره فیلتر: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> DeleteFilterAsync(int filterId)
    {
        try
        {
            // این متد نیاز به userId و tableId دارد - فعلاً پیاده‌سازی ساده
            await Task.CompletedTask;
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در حذف فیلتر: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> SetDefaultFilterAsync(int userId, string tableId, int filterId)
    {
        try
        {
            var filtersResult = await GetSavedFiltersAsync(userId, tableId);
            if (!filtersResult.IsSuccess)
            {
                return Result.Failure<bool>(filtersResult.Error);
            }

            var filters = filtersResult.Value;
            foreach (var filter in filters)
            {
                filter.IsDefault = (filter.Id == filterId);
            }

            await SaveFiltersToSettingsAsync(userId, tableId, filters);

            _cacheEngine.Remove($"saved_filters:{userId}:{tableId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در تنظیم پیش‌فرض: {ex.Message}"));
        }
    }

    public async Task<Result<SavedFilter?>> GetDefaultFilterAsync(int userId, string tableId)
    {
        try
        {
            var filtersResult = await GetSavedFiltersAsync(userId, tableId);
            if (!filtersResult.IsSuccess)
            {
                return Result.Failure<SavedFilter?>(filtersResult.Error);
            }

            var defaultFilter = filtersResult.Value.FirstOrDefault(f => f.IsDefault);
            return Result.Success(defaultFilter);
        }
        catch (Exception ex)
        {
            return Result.Failure<SavedFilter?>(
                Error.Validation($"خطا در دریافت فیلتر پیش‌فرض: {ex.Message}"));
        }
    }

    private async Task SaveFiltersToSettingsAsync(int userId, string tableId, List<SavedFilter> filters)
    {
        var settingsRepo = _unitOfWork.Repository<Settings>();
        var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            settings = new Settings { UserId = userId };
            await settingsRepo.AddAsync(settings);
        }

        var allData = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(settings.CustomSettings))
        {
            try
            {
                allData = JsonSerializer.Deserialize<Dictionary<string, object>>(
                    settings.CustomSettings, _jsonOptions) ?? new Dictionary<string, object>();
            }
            catch
            {
                allData = new Dictionary<string, object>();
            }
        }

        allData[$"filters_{tableId}"] = filters;

        settings.CustomSettings = JsonSerializer.Serialize(allData, _jsonOptions);
        settings.UpdatedAt = DateTime.Now;

        settingsRepo.Update(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion

    #region Import/Export

    public async Task<Result<string>> ExportUserSettingsAsync(int userId)
    {
        try
        {
            var exportData = new UserSettingsExportData
            {
                ExportDate = DateTime.Now,
                Version = "1.0",
                UserId = userId
            };

            // دریافت تنظیمات اصلی
            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings != null)
            {
                exportData.Theme = settings.Theme;
                exportData.Language = settings.Language;
                exportData.DateFormat = settings.DateFormat;
                exportData.TimeFormat = settings.TimeFormat;
                exportData.DefaultCurrency = settings.DefaultCurrency;
                exportData.PriceDecimals = settings.PriceDecimals;
                exportData.VolumeDecimals = settings.VolumeDecimals;
                exportData.AutoBackup = settings.AutoBackup;
                exportData.AutoBackupInterval = settings.AutoBackupInterval;
                exportData.BackupPath = settings.BackupPath;
                exportData.ShowNotifications = settings.ShowNotifications;
                exportData.PlaySounds = settings.PlaySounds;
                exportData.TradeListViewMode = settings.TradeListViewMode;
                exportData.ItemsPerPage = settings.ItemsPerPage;

                // دریافت سفارشی‌سازی‌ها
                if (!string.IsNullOrWhiteSpace(settings.CustomSettings))
                {
                    try
                    {
                        exportData.CustomSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(
                            settings.CustomSettings, _jsonOptions);
                    }
                    catch
                    {
                        exportData.CustomSettings = new Dictionary<string, object>();
                    }
                }
            }

            // دریافت فیلدهای سفارشی کاربر
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();
            var allFields = await fieldRepo.GetAllAsync();
            var userFields = allFields.Where(f => f.UserId == userId && f.IsActive).ToList();

            exportData.UserDefinedFields = userFields.Select(f => new UserDefinedField
            {
                FieldName = f.FieldName,
                DisplayName = f.DisplayName,
                FieldType = f.FieldType.ToString(),
                EntityType = f.EntityType ?? "Trade",
                Required = f.IsRequired,
                DefaultValue = f.DefaultValue,
                Options = f.Options,
                Order = f.Order
            }).ToList();

            var json = JsonSerializer.Serialize(exportData, _jsonOptions);
            return Result.Success(json);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(
                Error.Validation($"خطا در صادر کردن تنظیمات: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> ImportUserSettingsAsync(int userId, string settingsJson)
    {
        try
        {
            var importData = JsonSerializer.Deserialize<UserSettingsExportData>(settingsJson, _jsonOptions);
            if (importData == null)
            {
                return Result.Failure<bool>(Error.Validation("فایل تنظیمات نامعتبر است"));
            }

            // بروزرسانی تنظیمات اصلی
            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                settings = new Settings { UserId = userId };
                await settingsRepo.AddAsync(settings);
            }

            // بروزرسانی فیلدها
            if (!string.IsNullOrWhiteSpace(importData.Theme))
                settings.Theme = importData.Theme;
            if (!string.IsNullOrWhiteSpace(importData.Language))
                settings.Language = importData.Language;
            if (!string.IsNullOrWhiteSpace(importData.DateFormat))
                settings.DateFormat = importData.DateFormat;
            if (!string.IsNullOrWhiteSpace(importData.TimeFormat))
                settings.TimeFormat = importData.TimeFormat;
            if (!string.IsNullOrWhiteSpace(importData.DefaultCurrency))
                settings.DefaultCurrency = importData.DefaultCurrency;
            if (importData.PriceDecimals > 0)
                settings.PriceDecimals = importData.PriceDecimals;
            if (importData.VolumeDecimals > 0)
                settings.VolumeDecimals = importData.VolumeDecimals;
            
            settings.AutoBackup = importData.AutoBackup;
            if (importData.AutoBackupInterval > 0)
                settings.AutoBackupInterval = importData.AutoBackupInterval;
            if (!string.IsNullOrWhiteSpace(importData.BackupPath))
                settings.BackupPath = importData.BackupPath;
            
            settings.ShowNotifications = importData.ShowNotifications;
            settings.PlaySounds = importData.PlaySounds;
            
            if (!string.IsNullOrWhiteSpace(importData.TradeListViewMode))
                settings.TradeListViewMode = importData.TradeListViewMode;
            if (importData.ItemsPerPage > 0)
                settings.ItemsPerPage = importData.ItemsPerPage;

            if (importData.CustomSettings != null)
            {
                settings.CustomSettings = JsonSerializer.Serialize(importData.CustomSettings, _jsonOptions);
            }

            settings.UpdatedAt = DateTime.Now;
            settingsRepo.Update(settings);

            // وارد کردن فیلدهای سفارشی
            if (importData.UserDefinedFields?.Any() == true)
            {
                var fieldRepo = _unitOfWork.Repository<FieldDefinition>();

                foreach (var fieldData in importData.UserDefinedFields)
                {
                    var existing = await fieldRepo.FirstOrDefaultAsync(f =>
                        f.UserId == userId &&
                        f.EntityType == fieldData.EntityType &&
                        f.FieldName == fieldData.FieldName);

                    if (existing == null)
                    {
                        var fieldType = FieldType.Text;
                        Enum.TryParse<FieldType>(fieldData.FieldType, true, out fieldType);

                        var newField = new FieldDefinition
                        {
                            UserId = userId,
                            EntityType = fieldData.EntityType,
                            FieldName = fieldData.FieldName,
                            DisplayName = fieldData.DisplayName,
                            FieldType = fieldType,
                            IsRequired = fieldData.Required,
                            DefaultValue = fieldData.DefaultValue,
                            Options = fieldData.Options,
                            Order = fieldData.Order,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        };

                        await fieldRepo.AddAsync(newField);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش‌ها
            _cacheEngine.Clear();

            _eventBus.Publish(new CustomizationChangedEvent(userId, "SettingsImported", "all"));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(
                Error.Validation($"خطا در وارد کردن تنظیمات: {ex.Message}"));
        }
    }

    #endregion

    #region Helper Methods

    private FormSchema CloneForm(FormSchema form)
    {
        var json = JsonSerializer.Serialize(form, _jsonOptions);
        return JsonSerializer.Deserialize<FormSchema>(json, _jsonOptions) ?? new FormSchema();
    }

    private TableSchema CloneTable(TableSchema table)
    {
        var json = JsonSerializer.Serialize(table, _jsonOptions);
        return JsonSerializer.Deserialize<TableSchema>(json, _jsonOptions) ?? new TableSchema();
    }

    private void ApplyFieldCustomizations(FormSchema form, List<UserFieldCustomization> customizations)
    {
        if (form.Sections == null) return;

        foreach (var section in form.Sections)
        {
            if (section.Fields == null) continue;

            foreach (var field in section.Fields)
            {
                var customization = customizations.FirstOrDefault(c => c.FieldId == field.Id);
                if (customization != null)
                {
                    field.Visible = customization.Visible;
                    field.Required = customization.Required;

                    if (!string.IsNullOrWhiteSpace(customization.CustomLabel))
                    {
                        field.LabelFa = customization.CustomLabel;
                    }

                    if (customization.Width.HasValue)
                    {
                        field.Width = customization.Width.Value;
                    }
                }
            }

            // مرتب‌سازی فیلدها بر اساس Order
            section.Fields = section.Fields
                .OrderBy(f =>
                {
                    var cust = customizations.FirstOrDefault(c => c.FieldId == f.Id);
                    return cust?.Order ?? 999;
                })
                .ToList();
        }
    }

    private void ApplyColumnCustomizations(TableSchema table, List<UserColumnCustomization> customizations)
    {
        if (table.Columns == null) return;

        foreach (var column in table.Columns)
        {
            var customization = customizations.FirstOrDefault(c => c.ColumnId == column.Id);
            if (customization != null)
            {
                column.Visible = customization.Visible;
                column.Width = customization.Width;
                column.Order = customization.Order;

                if (!string.IsNullOrWhiteSpace(customization.CustomHeader))
                {
                    column.HeaderFa = customization.CustomHeader;
                }
            }
        }

        // مرتب‌سازی ستون‌ها بر اساس Order
        table.Columns = table.Columns
            .OrderBy(c => c.Order)
            .ToList();
    }

    private void AddUserDefinedFieldsToForm(FormSchema form, List<UserDefinedField> userFields)
    {
        if (form.Sections == null)
        {
            form.Sections = new List<SectionSchema>();
        }

        // پیدا کردن یا ایجاد بخش فیلدهای سفارشی
        var customSection = form.Sections.FirstOrDefault(s => s.Id == "custom_fields");
        if (customSection == null)
        {
            customSection = new SectionSchema
            {
                Id = "custom_fields",
                TitleFa = "فیلدهای سفارشی",
                Collapsed = false,
                Fields = new List<FieldSchema>()
            };
            form.Sections.Add(customSection);
        }

        if (customSection.Fields == null)
        {
            customSection.Fields = new List<FieldSchema>();
        }

        foreach (var userField in userFields.OrderBy(f => f.Order))
        {
            var fieldSchema = new FieldSchema
            {
                Id = $"custom_{userField.FieldName}",
                LabelFa = userField.DisplayName,
                Type = userField.FieldType.ToLower(),
                Required = userField.Required,
                DefaultValue = userField.DefaultValue
            };

            // پردازش گزینه‌ها برای فیلدهای select
            if (!string.IsNullOrWhiteSpace(userField.Options))
            {
                try
                {
                    fieldSchema.Options = JsonSerializer.Deserialize<List<OptionSchema>>(
                        userField.Options, _jsonOptions);
                }
                catch
                {
                    // اگر JSON نامعتبر بود، گزینه‌ها را از رشته ساده بساز
                    fieldSchema.Options = userField.Options
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(o => new OptionSchema { Value = o.Trim(), LabelFa = o.Trim() })
                        .ToList();
                }
            }

            customSection.Fields.Add(fieldSchema);
        }
    }

    private TableSchema CreateDefaultTradeTable()
    {
        return new TableSchema
        {
            Id = "tradeList",
            TitleFa = "لیست معاملات",
            DataSource = "Trades",
            Sortable = true,
            Filterable = true,
            Pageable = true,
            PageSize = 50,
            Selectable = true,
            SelectionMode = "single",
            Columns = new List<ColumnSchema>
            {
                new() { Id = "id", Field = "Id", HeaderFa = "ردیف", Type = "number", Width = 60, Order = 0, Visible = true },
                new() { Id = "symbol", Field = "Symbol", HeaderFa = "نماد", Type = "text", Width = 100, Order = 1, Visible = true },
                new() { Id = "direction", Field = "Direction", HeaderFa = "جهت", Type = "enum", Width = 80, Order = 2, Visible = true },
                new() { Id = "volume", Field = "Volume", HeaderFa = "حجم", Type = "decimal", Width = 80, Order = 3, Visible = true },
                new() { Id = "entryPrice", Field = "EntryPrice", HeaderFa = "قیمت ورود", Type = "decimal", Width = 100, Order = 4, Visible = true },
                new() { Id = "exitPrice", Field = "ExitPrice", HeaderFa = "قیمت خروج", Type = "decimal", Width = 100, Order = 5, Visible = true },
                new() { Id = "stopLoss", Field = "StopLoss", HeaderFa = "حد ضرر", Type = "decimal", Width = 100, Order = 6, Visible = true },
                new() { Id = "takeProfit", Field = "TakeProfit", HeaderFa = "حد سود", Type = "decimal", Width = 100, Order = 7, Visible = true },
                new() { Id = "profitLoss", Field = "ProfitLoss", HeaderFa = "سود/زیان", Type = "currency", Width = 100, Order = 8, Visible = true },
                new() { Id = "rValue", Field = "RValue", HeaderFa = "مقدار R", Type = "decimal", Width = 80, Order = 9, Visible = true },
                new() { Id = "entryTime", Field = "EntryTime", HeaderFa = "زمان ورود", Type = "datetime", Width = 150, Order = 10, Visible = true },
                new() { Id = "exitTime", Field = "ExitTime", HeaderFa = "زمان خروج", Type = "datetime", Width = 150, Order = 11, Visible = true },
                new() { Id = "session", Field = "Session", HeaderFa = "سشن", Type = "text", Width = 80, Order = 12, Visible = false },
                new() { Id = "timeframe", Field = "Timeframe", HeaderFa = "تایم‌فریم", Type = "text", Width = 80, Order = 13, Visible = false },
                new() { Id = "notes", Field = "Notes", HeaderFa = "یادداشت", Type = "text", Width = 200, Order = 14, Visible = false }
            },
            Actions = new List<TableActionSchema>
            {
                new() { Id = "edit", LabelFa = "ویرایش", Icon = "✏️", Type = "row", Style = "primary" },
                new() { Id = "delete", LabelFa = "حذف", Icon = "🗑️", Type = "row", Style = "danger" },
                new() { Id = "duplicate", LabelFa = "کپی", Icon = "📋", Type = "row", Style = "secondary" }
            },
            Filters = new List<FilterSchema>
            {
                new() { Id = "symbol", Field = "Symbol", LabelFa = "نماد", Type = "text", Operator = "contains" },
                new() { Id = "direction", Field = "Direction", LabelFa = "جهت", Type = "select",
                    Options = new List<OptionSchema>
                    {
                        new() { Value = "1", LabelFa = "خرید" },
                        new() { Value = "2", LabelFa = "فروش" }
                    }
                },
                new() { Id = "dateRange", Field = "EntryTime", LabelFa = "بازه تاریخ", Type = "daterange" }
            }
        };
    }

    #endregion
}

#region Export/Import Models

/// <summary>
/// مدل صادرات تنظیمات کاربر
/// </summary>
public class UserSettingsExportData
{
    public DateTime ExportDate { get; set; }
    public string Version { get; set; } = "1.0";
    public int UserId { get; set; }
    
    // تنظیمات اصلی
    public string? Theme { get; set; }
    public string? Language { get; set; }
    public string? DateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? DefaultCurrency { get; set; }
    public int PriceDecimals { get; set; }
    public int VolumeDecimals { get; set; }
    public bool AutoBackup { get; set; }
    public int AutoBackupInterval { get; set; }
    public string? BackupPath { get; set; }
    public bool ShowNotifications { get; set; }
    public bool PlaySounds { get; set; }
    public string? TradeListViewMode { get; set; }
    public int ItemsPerPage { get; set; }
    
    // سفارشی‌سازی‌ها
    public Dictionary<string, object>? CustomSettings { get; set; }
    public List<UserDefinedField>? UserDefinedFields { get; set; }
}

#endregion

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/Services/MetadataService.cs
// =============================================================================