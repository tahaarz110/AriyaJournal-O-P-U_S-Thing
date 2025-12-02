// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Entities/Account.cs
// شماره فایل: 7
// =============================================================================

using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت حساب معاملاتی
/// </summary>
public class Account
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه کاربر مالک
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// نام حساب
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// نوع حساب
    /// </summary>
    public AccountType Type { get; set; } = AccountType.Demo;

    /// <summary>
    /// نام بروکر
    /// </summary>
    public string BrokerName { get; set; } = string.Empty;

    /// <summary>
    /// شماره حساب در بروکر
    /// </summary>
    public string? AccountNumber { get; set; }

    /// <summary>
    /// سرور بروکر
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// موجودی اولیه
    /// </summary>
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// موجودی فعلی
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// واحد ارز
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// لوریج
    /// </summary>
    public int Leverage { get; set; } = 100;

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// فعال بودن حساب
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// حساب پیش‌فرض
    /// </summary>
    public bool IsDefault { get; set; }

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
    /// لیست معاملات
    /// </summary>
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Entities/Account.cs
// =============================================================================