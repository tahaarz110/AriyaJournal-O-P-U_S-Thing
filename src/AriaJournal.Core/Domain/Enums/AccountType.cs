// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Enums/AccountType.cs
// شماره فایل: 3
// =============================================================================

namespace AriaJournal.Core.Domain.Enums;

/// <summary>
/// نوع حساب معاملاتی
/// </summary>
public enum AccountType
{
    /// <summary>
    /// حساب واقعی (ریل)
    /// </summary>
    Real = 1,

    /// <summary>
    /// حساب دمو (آزمایشی)
    /// </summary>
    Demo = 2,

    /// <summary>
    /// حساب Prop Trading
    /// </summary>
    Prop = 3,

    /// <summary>
    /// حساب چالش
    /// </summary>
    Challenge = 4
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Enums/AccountType.cs
// =============================================================================