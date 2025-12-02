// =============================================================================
// فایل: src/AriaJournal.Core/Application/Services/FieldDefinitionService.cs
// توضیح: سرویس مدیریت فیلدهای سفارشی - اصلاح‌شده
// =============================================================================

using System.Text.Json;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// سرویس مدیریت فیلدهای سفارشی
/// </summary>
public class FieldDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly ICacheEngine _cacheEngine;

    public FieldDefinitionService(
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        ICacheEngine cacheEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));
    }

    /// <summary>
    /// دریافت همه فیلدهای کاربر
    /// </summary>
    public async Task<Result<List<FieldDefinitionDto>>> GetUserFieldsAsync(int userId)
    {
        try
        {
            // بررسی کش
            var cacheKey = $"fields:user:{userId}";
            var cached = _cacheEngine.Get<List<FieldDefinitionDto>>(cacheKey);
            if (cached != null)
            {
                return Result.Success(cached);
            }

            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();
            var fields = await fieldRepo.GetAllAsync(f => f.UserId == userId && f.IsActive);

            var result = fields
                .OrderBy(f => f.DisplayOrder)
                .ThenBy(f => f.DisplayName)
                .Select(MapToDto)
                .ToList();

            _cacheEngine.Set(cacheKey, result, TimeSpan.FromMinutes(30));

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت فیلدها: {ex.Message}");
            return Result.Failure<List<FieldDefinitionDto>>(Error.Failure("خطا در دریافت فیلدهای سفارشی"));
        }
    }

    /// <summary>
    /// دریافت فیلدها بر اساس دسته‌بندی
    /// </summary>
    public async Task<Result<Dictionary<string, List<FieldDefinitionDto>>>> GetFieldsByCategoryAsync(int userId)
    {
        try
        {
            var fieldsResult = await GetUserFieldsAsync(userId);
            if (fieldsResult.IsFailure)
            {
                return Result.Failure<Dictionary<string, List<FieldDefinitionDto>>>(fieldsResult.Error);
            }

            var grouped = fieldsResult.Value
                .GroupBy(f => f.Category ?? "سایر")
                .ToDictionary(g => g.Key, g => g.ToList());

            return Result.Success(grouped);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت فیلدها: {ex.Message}");
            return Result.Failure<Dictionary<string, List<FieldDefinitionDto>>>(Error.Failure("خطا در دریافت فیلدها"));
        }
    }

    /// <summary>
    /// ایجاد فیلد جدید
    /// </summary>
    public async Task<Result<FieldDefinitionDto>> CreateAsync(int userId, FieldDefinitionCreateDto dto)
    {
        // اعتبارسنجی
        if (string.IsNullOrWhiteSpace(dto.FieldName))
        {
            return Result.Failure<FieldDefinitionDto>(Error.Validation("نام فیلد الزامی است"));
        }

        if (string.IsNullOrWhiteSpace(dto.DisplayName))
        {
            return Result.Failure<FieldDefinitionDto>(Error.Validation("نام نمایشی الزامی است"));
        }

        try
        {
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();

            // بررسی تکراری نبودن - استفاده از AnyAsync
            var exists = await fieldRepo.AnyAsync(f =>
                f.UserId == userId && f.FieldName.ToLower() == dto.FieldName.ToLower());

            if (exists)
            {
                return Result.Failure<FieldDefinitionDto>(Error.DuplicateFieldName);
            }

            // دریافت بالاترین DisplayOrder
            var allFields = await fieldRepo.GetAllAsync(f => f.UserId == userId);
            var maxOrder = allFields.Any() ? allFields.Max(f => f.DisplayOrder) : 0;

            var field = new FieldDefinition
            {
                UserId = userId,
                FieldName = dto.FieldName.ToLowerInvariant().Replace(" ", "_"),
                DisplayName = dto.DisplayName,
                FieldType = dto.FieldType,
                Options = dto.Options != null ? JsonSerializer.Serialize(dto.Options) : null,
                DefaultValue = dto.DefaultValue,
                IsRequired = dto.IsRequired,
                HelpText = dto.HelpText,
                DisplayOrder = maxOrder + 1,
                Order = maxOrder + 1,
                Category = dto.Category,
                IsActive = true,
                IsSystem = false,
                CreatedAt = DateTime.Now
            };

            await fieldRepo.AddAsync(field);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"fields:user:{userId}");

            return Result.Success(MapToDto(field));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد فیلد: {ex.Message}");
            return Result.Failure<FieldDefinitionDto>(Error.Failure("خطا در ذخیره فیلد"));
        }
    }

    /// <summary>
    /// ویرایش فیلد
    /// </summary>
    public async Task<Result<FieldDefinitionDto>> UpdateAsync(FieldDefinitionUpdateDto dto)
    {
        try
        {
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();
            var field = await fieldRepo.GetByIdAsync(dto.Id);

            if (field == null)
            {
                return Result.Failure<FieldDefinitionDto>(Error.FieldNotFound);
            }

            if (field.IsSystem)
            {
                return Result.Failure<FieldDefinitionDto>(Error.Validation("فیلدهای سیستمی قابل ویرایش نیستند"));
            }

            field.DisplayName = dto.DisplayName;
            field.Options = dto.Options != null ? JsonSerializer.Serialize(dto.Options) : null;
            field.DefaultValue = dto.DefaultValue;
            field.IsRequired = dto.IsRequired;
            field.HelpText = dto.HelpText;
            field.DisplayOrder = dto.DisplayOrder;
            field.Order = dto.DisplayOrder;
            field.Category = dto.Category;
            field.UpdatedAt = DateTime.Now;

            fieldRepo.Update(field);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"fields:user:{field.UserId}");

            return Result.Success(MapToDto(field));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ویرایش فیلد: {ex.Message}");
            return Result.Failure<FieldDefinitionDto>(Error.Failure("خطا در ذخیره فیلد"));
        }
    }

    /// <summary>
    /// حذف فیلد
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int fieldId)
    {
        try
        {
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();
            var field = await fieldRepo.GetByIdAsync(fieldId);

            if (field == null)
            {
                return Result.Failure<bool>(Error.FieldNotFound);
            }

            if (field.IsSystem)
            {
                return Result.Failure<bool>(Error.CannotDeleteSystemField);
            }

            var userId = field.UserId;

            // غیرفعال کردن به جای حذف
            field.IsActive = false;
            field.UpdatedAt = DateTime.Now;

            fieldRepo.Update(field);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"fields:user:{userId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در حذف فیلد: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در حذف فیلد"));
        }
    }

    /// <summary>
    /// بروزرسانی گزینه‌های فیلد Select
    /// </summary>
    public async Task<Result<bool>> UpdateOptionsAsync(int fieldId, List<string> options)
    {
        try
        {
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();
            var field = await fieldRepo.GetByIdAsync(fieldId);

            if (field == null)
            {
                return Result.Failure<bool>(Error.FieldNotFound);
            }

            if (field.FieldType != FieldType.Select && field.FieldType != FieldType.MultiSelect)
            {
                return Result.Failure<bool>(Error.Validation("این فیلد از نوع انتخابی نیست"));
            }

            field.Options = JsonSerializer.Serialize(options);
            field.UpdatedAt = DateTime.Now;

            fieldRepo.Update(field);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"fields:user:{field.UserId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بروزرسانی گزینه‌ها: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در ذخیره تغییرات"));
        }
    }

    /// <summary>
    /// تغییر ترتیب فیلدها
    /// </summary>
    public async Task<Result<bool>> ReorderFieldsAsync(int userId, List<int> orderedFieldIds)
    {
        try
        {
            var fieldRepo = _unitOfWork.Repository<FieldDefinition>();

            for (int i = 0; i < orderedFieldIds.Count; i++)
            {
                var field = await fieldRepo.GetByIdAsync(orderedFieldIds[i]);
                if (field != null && field.UserId == userId)
                {
                    field.DisplayOrder = i + 1;
                    field.Order = i + 1;
                    fieldRepo.Update(field);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"fields:user:{userId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در تغییر ترتیب: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در ذخیره تغییرات"));
        }
    }

    #region Private Methods

    private static FieldDefinitionDto MapToDto(FieldDefinition field)
    {
        List<string>? options = null;
        if (!string.IsNullOrEmpty(field.Options))
        {
            try
            {
                options = JsonSerializer.Deserialize<List<string>>(field.Options);
            }
            catch
            {
                options = null;
            }
        }

        return new FieldDefinitionDto
        {
            Id = field.Id,
            FieldName = field.FieldName,
            DisplayName = field.DisplayName,
            FieldType = field.FieldType,
            Options = options,
            DefaultValue = field.DefaultValue,
            IsRequired = field.IsRequired,
            HelpText = field.HelpText,
            DisplayOrder = field.DisplayOrder,
            Category = field.Category,
            IsSystem = field.IsSystem,
            IsActive = field.IsActive
        };
    }

    #endregion
}

/// <summary>
/// DTO فیلد سفارشی
/// </summary>
public class FieldDefinitionDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public List<string>? Options { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public string? HelpText { get; set; }
    public int DisplayOrder { get; set; }
    public string? Category { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }

    public string FieldTypeDisplay => FieldType switch
    {
        FieldType.Text => "متن",
        FieldType.Integer => "عدد صحیح",
        FieldType.Decimal => "عدد اعشاری",
        FieldType.Date => "تاریخ",
        FieldType.DateTime => "تاریخ و زمان",
        FieldType.Boolean => "بله/خیر",
        FieldType.Select => "لیست انتخابی",
        FieldType.MultiSelect => "چند انتخابی",
        FieldType.TextArea => "متن چند خطی",
        FieldType.Rating => "امتیاز",
        _ => "نامشخص"
    };
}

/// <summary>
/// DTO ایجاد فیلد
/// </summary>
public class FieldDefinitionCreateDto
{
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public FieldType FieldType { get; set; }
    public List<string>? Options { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public string? HelpText { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// DTO ویرایش فیلد
/// </summary>
public class FieldDefinitionUpdateDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public string? HelpText { get; set; }
    public int DisplayOrder { get; set; }
    public string? Category { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/Services/FieldDefinitionService.cs
// =============================================================================