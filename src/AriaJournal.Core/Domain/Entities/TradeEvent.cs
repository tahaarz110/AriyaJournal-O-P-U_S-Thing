// ═══════════════════════════════════════════════════════════════════════
// فایل: TradeEvent.cs
// مسیر: src/AriaJournal.Core/Domain/Entities/TradeEvent.cs
// توضیح: رویدادهای مدیریت معامله (Trail, BE, Scale)
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// رویداد مدیریت معامله
/// هر تغییر در معامله باز به عنوان یک Event ثبت می‌شود
/// </summary>
public class TradeEvent
{
    /// <summary>
    /// شناسه یکتا
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// شناسه معامله مرتبط
    /// </summary>
    public int TradeId { get; set; }

    /// <summary>
    /// معامله مرتبط
    /// </summary>
    public Trade? Trade { get; set; }

    /// <summary>
    /// نوع رویداد
    /// </summary>
    public TradeEventType EventType { get; set; }

    /// <summary>
    /// زمان رویداد
    /// </summary>
    public DateTime EventTime { get; set; }

    /// <summary>
    /// قیمت در زمان رویداد
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// مقدار قبلی (برای تغییرات)
    /// </summary>
    public decimal? OldValue { get; set; }

    /// <summary>
    /// مقدار جدید
    /// </summary>
    public decimal? NewValue { get; set; }

    /// <summary>
    /// حجم تغییر (برای ScaleIn/Out)
    /// </summary>
    public decimal? VolumeChange { get; set; }

    /// <summary>
    /// دلیل این رویداد
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// یادداشت
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// مسیر اسکرین‌شات این لحظه
    /// </summary>
    public string? ScreenshotPath { get; set; }

    /// <summary>
    /// شناسه احساس در این لحظه
    /// </summary>
    public int? EmotionId { get; set; }

    /// <summary>
    /// احساس مرتبط
    /// </summary>
    public Emotion? Emotion { get; set; }

    /// <summary>
    /// زمان ایجاد رکورد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// انواع رویداد مدیریت معامله
/// </summary>
public enum TradeEventType
{
    /// <summary>
    /// ورود به معامله
    /// </summary>
    Entry = 1,

    /// <summary>
    /// تغییر StopLoss
    /// </summary>
    StopLossModified = 2,

    /// <summary>
    /// تغییر TakeProfit
    /// </summary>
    TakeProfitModified = 3,

    /// <summary>
    /// انتقال به Break-Even
    /// </summary>
    BreakEven = 4,

    /// <summary>
    /// Trailing Stop
    /// </summary>
    TrailingStop = 5,

    /// <summary>
    /// اضافه کردن به پوزیشن
    /// </summary>
    ScaleIn = 6,

    /// <summary>
    /// کم کردن از پوزیشن
    /// </summary>
    ScaleOut = 7,

    /// <summary>
    /// بستن بخشی از پوزیشن
    /// </summary>
    PartialClose = 8,

    /// <summary>
    /// خروج کامل
    /// </summary>
    Exit = 9,

    /// <summary>
    /// خروج دستی
    /// </summary>
    ManualExit = 10,

    /// <summary>
    /// Stop Loss Hit
    /// </summary>
    StopLossHit = 11,

    /// <summary>
    /// Take Profit Hit
    /// </summary>
    TakeProfitHit = 12,

    /// <summary>
    /// یادداشت عمومی
    /// </summary>
    Note = 13,

    /// <summary>
    /// تغییر حجم
    /// </summary>
    VolumeModified = 14,

    /// <summary>
    /// سایر
    /// </summary>
    Other = 99
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: TradeEvent.cs
// ═══════════════════════════════════════════════════════════════════════