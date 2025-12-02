// ═══════════════════════════════════════════════════════════════════════
// فایل: Trade.cs
// مسیر: src/AriaJournal.Core/Domain/Entities/Trade.cs
// توضیح: موجودیت معامله - نسخه کامل با رابطه‌های جدید
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Enums;

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// موجودیت معامله
/// </summary>
public class Trade
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه حساب
    /// </summary>
    public int AccountId { get; set; }

    /// <summary>
    /// شماره تیکت (از بروکر)
    /// </summary>
    public string? Ticket { get; set; }

    /// <summary>
    /// نماد معاملاتی
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// جهت معامله
    /// </summary>
    public TradeDirection Direction { get; set; }

    /// <summary>
    /// حجم معامله (لات)
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// قیمت ورود
    /// </summary>
    public decimal EntryPrice { get; set; }

    /// <summary>
    /// قیمت خروج
    /// </summary>
    public decimal? ExitPrice { get; set; }

    /// <summary>
    /// حد ضرر
    /// </summary>
    public decimal? StopLoss { get; set; }

    /// <summary>
    /// حد سود
    /// </summary>
    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// زمان ورود
    /// </summary>
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// زمان خروج
    /// </summary>
    public DateTime? ExitTime { get; set; }

    /// <summary>
    /// کمیسیون
    /// </summary>
    public decimal Commission { get; set; }

    /// <summary>
    /// سواپ
    /// </summary>
    public decimal Swap { get; set; }

    /// <summary>
    /// سود/زیان خالص
    /// </summary>
    public decimal? ProfitLoss { get; set; }

    /// <summary>
    /// سود/زیان به پیپ
    /// </summary>
    public decimal? ProfitLossPips { get; set; }

    /// <summary>
    /// ریسک به ریوارد واقعی
    /// </summary>
    public decimal? RiskRewardRatio { get; set; }

    /// <summary>
    /// درصد ریسک
    /// </summary>
    public decimal? RiskPercent { get; set; }

    /// <summary>
    /// آیا معامله بسته شده
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// یادداشت قبل از معامله
    /// </summary>
    public string? PreTradeNotes { get; set; }

    /// <summary>
    /// یادداشت بعد از معامله
    /// </summary>
    public string? PostTradeNotes { get; set; }

    /// <summary>
    /// دلیل ورود
    /// </summary>
    public string? EntryReason { get; set; }

    /// <summary>
    /// دلیل خروج
    /// </summary>
    public string? ExitReason { get; set; }

    /// <summary>
    /// اشتباهات
    /// </summary>
    public string? Mistakes { get; set; }

    /// <summary>
    /// درس‌های آموخته شده
    /// </summary>
    public string? Lessons { get; set; }

    /// <summary>
    /// امتیاز اجرا (۱ تا ۵)
    /// </summary>
    public int? ExecutionRating { get; set; }

    /// <summary>
    /// آیا ورود به سطله بوده
    /// </summary>
    public bool? IsImpulsive { get; set; }

    /// <summary>
    /// آیا طبق پلن بوده
    /// </summary>
    public bool? FollowedPlan { get; set; }

    /// <summary>
    /// تگ‌ها (جدا شده با کاما)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// سشن معاملاتی
    /// </summary>
    public string? Session { get; set; }

    /// <summary>
    /// تایم‌فریم
    /// </summary>
    public string? Timeframe { get; set; }

    /// <summary>
    /// نوع ستاپ
    /// </summary>
    public string? SetupType { get; set; }

    /// <summary>
    /// ساختار بازار
    /// </summary>
    public string? MarketStructure { get; set; }

    /// <summary>
    /// استراتژی
    /// </summary>
    public string? Strategy { get; set; }

    /// <summary>
    /// تاریخ ایجاد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// تاریخ آخرین ویرایش
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// آیا در سطل زباله است
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// تاریخ حذف
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    // ═══════════════════════════════════════════════════════════════
    // Navigation Properties
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// حساب معاملاتی
    /// </summary>
    public virtual Account? Account { get; set; }

    /// <summary>
    /// فیلدهای سفارشی
    /// </summary>
    public virtual ICollection<TradeCustomField> CustomFields { get; set; } = new List<TradeCustomField>();

    /// <summary>
    /// اسکرین‌شات‌ها
    /// </summary>
    public virtual ICollection<Screenshot> Screenshots { get; set; } = new List<Screenshot>();

    /// <summary>
    /// رویدادهای مدیریت معامله (Trail, BE, Scale, ...)
    /// </summary>
    public virtual ICollection<TradeEvent> Events { get; set; } = new List<TradeEvent>();

    /// <summary>
    /// احساسات ثبت‌شده در طول معامله
    /// </summary>
    public virtual ICollection<Emotion> Emotions { get; set; } = new List<Emotion>();
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: Trade.cs
// ═══════════════════════════════════════════════════════════════════════