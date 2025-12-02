// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/TradeDto.cs
// شماره فایل: 65
// توضیح: DTO معامله
// =============================================================================

using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO برای نمایش معامله
/// </summary>
public class TradeDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string? Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeDirection Direction { get; set; }
    public string DirectionDisplay => Direction == TradeDirection.Buy ? "خرید" : "فروش";
    public string DirectionIcon => Direction == TradeDirection.Buy ? "▲" : "▼";
    public decimal Volume { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public decimal Commission { get; set; }
    public decimal Swap { get; set; }
    public decimal? ProfitLoss { get; set; }
    public decimal? ProfitLossPips { get; set; }
    public decimal? RiskRewardRatio { get; set; }
    public decimal? RiskPercent { get; set; }
    public bool IsClosed { get; set; }
    public string? PreTradeNotes { get; set; }
    public string? PostTradeNotes { get; set; }
    public string? EntryReason { get; set; }
    public string? ExitReason { get; set; }
    public string? Mistakes { get; set; }
    public string? Lessons { get; set; }
    public int? ExecutionRating { get; set; }
    public bool? IsImpulsive { get; set; }
    public bool? FollowedPlan { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ScreenshotsCount { get; set; }

    // فیلدهای سفارشی
    public Dictionary<string, object?> CustomFields { get; set; } = new();

    /// <summary>
    /// نمایش سود/زیان
    /// </summary>
    public string ProfitLossDisplay
    {
        get
        {
            if (!ProfitLoss.HasValue) return "-";
            var value = ProfitLoss.Value;
            return value >= 0 ? $"+{value:N2}" : $"{value:N2}";
        }
    }

    /// <summary>
    /// رنگ سود/زیان
    /// </summary>
    public string ProfitLossColor
    {
        get
        {
            if (!ProfitLoss.HasValue) return "Gray";
            return ProfitLoss.Value >= 0 ? "Green" : "Red";
        }
    }

    /// <summary>
    /// وضعیت معامله
    /// </summary>
    public string StatusDisplay => IsClosed ? "بسته" : "باز";

    /// <summary>
    /// مدت زمان معامله
    /// </summary>
    public string DurationDisplay
    {
        get
        {
            if (!ExitTime.HasValue) return "-";
            var duration = ExitTime.Value - EntryTime;
            if (duration.TotalDays >= 1)
                return $"{(int)duration.TotalDays} روز";
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours} ساعت";
            return $"{(int)duration.TotalMinutes} دقیقه";
        }
    }

    /// <summary>
    /// نمایش R:R
    /// </summary>
    public string RRDisplay => RiskRewardRatio.HasValue ? $"1:{RiskRewardRatio:N1}" : "-";
}

/// <summary>
/// DTO برای لیست معاملات (خلاصه)
/// </summary>
public class TradeSummaryDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeDirection Direction { get; set; }
    public decimal Volume { get; set; }
    public DateTime EntryTime { get; set; }
    public decimal? ProfitLoss { get; set; }
    public bool IsClosed { get; set; }

    public string DirectionIcon => Direction == TradeDirection.Buy ? "▲" : "▼";
    public string ProfitLossDisplay => ProfitLoss.HasValue 
        ? (ProfitLoss.Value >= 0 ? $"+{ProfitLoss:N2}" : $"{ProfitLoss:N2}") 
        : "-";
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/TradeDto.cs
// =============================================================================