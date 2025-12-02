// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/FieldDefinition.cs
// توضیح: موجودیت تعریف فیلد سفارشی - نسخه کامل با همه فیلدها
// =============================================================================

using System;
using System.Collections.Generic;
using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت تعریف فیلد سفارشی
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// نوع موجودیت (Trade, Account, ...)
    /// </summary>
    public string EntityType { get; set; } = "Trade";

    /// <summary>
    /// نام فیلد (انگلیسی)
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// نام نمایشی فارسی
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// نوع فیلد
    /// </summary>
    public FieldType FieldType { get; set; } = FieldType.Text;

    /// <summary>
    /// آیا الزامی است؟
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// مقدار پیش‌فرض
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// گزینه‌ها (برای فیلدهای انتخابی - JSON)
    /// </summary>
    public string? Options { get; set; }

    /// <summary>
    /// قوانین اعتبارسنجی (JSON)
    /// </summary>
    public string? ValidationRules { get; set; }

    /// <summary>
    /// متن راهنما
    /// </summary>
    public string? HelpText { get; set; }

    /// <summary>
    /// دسته‌بندی
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// ترتیب نمایش (قدیمی - برای سازگاری)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// ترتیب نمایش (جدید)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// آیا فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// آیا فیلد سیستمی است؟
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// آیا در فرم نمایش داده شود؟
    /// </summary>
    public bool ShowInForm { get; set; } = true;

    /// <summary>
    /// آیا در لیست نمایش داده شود؟
    /// </summary>
    public bool ShowInList { get; set; } = true;

    /// <summary>
    /// آیا قابل جستجو است؟
    /// </summary>
    public bool IsSearchable { get; set; } = false;

    /// <summary>
    /// آیا قابل فیلتر است؟
    /// </summary>
    public bool IsFilterable { get; set; } = false;

    /// <summary>
    /// آیا قابل مرتب‌سازی است؟
    /// </summary>
    public bool IsSortable { get; set; } = false;

    /// <summary>
    /// عرض ستون در لیست
    /// </summary>
    public int? ColumnWidth { get; set; }

    /// <summary>
    /// متادیتای اضافی (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    
    /// <summary>
    /// کاربر مالک
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// مقادیر سفارشی معاملات
    /// </summary>
    public virtual ICollection<TradeCustomField> TradeCustomFields { get; set; } = new List<TradeCustomField>();
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/FieldDefinition.cs
// =============================================================================