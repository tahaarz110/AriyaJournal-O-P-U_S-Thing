// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/AccountDto.cs
// شماره فایل: 64
// توضیح: DTO حساب معاملاتی
// =============================================================================

using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO برای نمایش حساب
/// </summary>
public class AccountDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string TypeDisplayName => Type switch
    {
        AccountType.Real => "واقعی",
        AccountType.Demo => "دمو",
        AccountType.Prop => "Prop",
        AccountType.Challenge => "چالش",
        _ => "نامشخص"
    };
    public string BrokerName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public int Leverage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TradesCount { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal WinRate { get; set; }

    /// <summary>
    /// نمایش موجودی با فرمت
    /// </summary>
    public string BalanceDisplay => $"{CurrentBalance:N2} {Currency}";

    /// <summary>
    /// نمایش سود/زیان با فرمت
    /// </summary>
    public string ProfitLossDisplay => TotalProfitLoss >= 0 
        ? $"+{TotalProfitLoss:N2}" 
        : $"{TotalProfitLoss:N2}";

    /// <summary>
    /// نمایش نرخ برد
    /// </summary>
    public string WinRateDisplay => $"{WinRate:N1}%";
}

/// <summary>
/// DTO برای ایجاد حساب
/// </summary>
public class AccountCreateDto
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; } = AccountType.Demo;
    public string BrokerName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Server { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public int Leverage { get; set; } = 100;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// DTO برای ویرایش حساب
/// </summary>
public class AccountUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string BrokerName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Server { get; set; }
    public decimal InitialBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public int Leverage { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// DTO برای لیست حساب‌ها (خلاصه)
/// </summary>
public class AccountSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BrokerName { get; set; } = string.Empty;
    public string TypeDisplayName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public string DisplayText => $"{Name} ({BrokerName}) - {CurrentBalance:N2} {Currency}";
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/AccountDto.cs
// =============================================================================