// =============================================================================
// فایل: src/AriaJournal.Core/Application/DTOs/TradeCreateDto.cs
// شماره فایل: 66
// توضیح: DTO ایجاد و ویرایش معامله
// =============================================================================

using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Application.DTOs;

/// <summary>
/// DTO برای ایجاد معامله جدید
/// </summary>
public class TradeCreateDto
{
    public int AccountId { get; set; }
    public string? Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeDirection Direction { get; set; }
    public decimal Volume { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? ExitPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public DateTime EntryTime { get; set; } = DateTime.Now;
    public DateTime? ExitTime { get; set; }
    public decimal Commission { get; set; }
    public decimal Swap { get; set; }
    public decimal? ProfitLoss { get; set; }
    public string? PreTradeNotes { get; set; }
    public string? PostTradeNotes { get; set; }
    public string? EntryReason { get; set; }
    public string? ExitReason { get; set; }
    public string? Mistakes { get; set; }
    public string? Lessons { get; set; }
    public int? ExecutionRating { get; set; }
    public bool? IsImpulsive { get; set; }
    public bool? FollowedPlan { get; set; }

    /// <summary>
    /// فیلدهای سفارشی
    /// </summary>
    public Dictionary<string, object?> CustomFields { get; set; } = new();

    /// <summary>
    /// آیا معامله بسته است
    /// </summary>
    public bool IsClosed => ExitPrice.HasValue && ExitTime.HasValue;
}

/// <summary>
/// DTO برای ویرایش معامله
/// </summary>
public class TradeUpdateDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string? Ticket { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeDirection Direction { get; set; }
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
    public string? PreTradeNotes { get; set; }
    public string? PostTradeNotes { get; set; }
    public string? EntryReason { get; set; }
    public string? ExitReason { get; set; }
    public string? Mistakes { get; set; }
    public string? Lessons { get; set; }
    public int? ExecutionRating { get; set; }
    public bool? IsImpulsive { get; set; }
    public bool? FollowedPlan { get; set; }
    public bool IsClosed { get; set; }

    /// <summary>
    /// فیلدهای سفارشی
    /// </summary>
    public Dictionary<string, object?> CustomFields { get; set; } = new();
}

/// <summary>
/// DTO برای بستن معامله
/// </summary>
public class TradeCloseDto
{
    public int TradeId { get; set; }
    public decimal ExitPrice { get; set; }
    public DateTime ExitTime { get; set; } = DateTime.Now;
    public string? ExitReason { get; set; }
    public decimal? Commission { get; set; }
    public decimal? Swap { get; set; }
}

/// <summary>
/// DTO برای فیلتر معاملات
/// </summary>
public class TradeFilterDto
{
    public int? AccountId { get; set; }
    public string? Symbol { get; set; }
    public TradeDirection? Direction { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsClosed { get; set; }
    public bool? IsProfit { get; set; }
    public decimal? MinProfitLoss { get; set; }
    public decimal? MaxProfitLoss { get; set; }
    public int? MinRating { get; set; }
    public string? SearchText { get; set; }

    // صفحه‌بندی
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    // مرتب‌سازی
    public string SortBy { get; set; } = "EntryTime";
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// DTO برای نتیجه صفحه‌بندی شده معاملات
/// </summary>
public class TradePagedResultDto
{
    public List<TradeDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    // آمار کلی
    public decimal TotalProfitLoss { get; set; }
    public int WinCount { get; set; }
    public int LossCount { get; set; }
    public decimal WinRate => TotalCount > 0 ? (decimal)WinCount / TotalCount * 100 : 0;
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/DTOs/TradeCreateDto.cs
// =============================================================================