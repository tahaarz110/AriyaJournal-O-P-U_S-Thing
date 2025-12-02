// ═══════════════════════════════════════════════════════════════════════
// فایل: Emotion.cs
// مسیر: src/AriaJournal.Core/Domain/Entities/Emotion.cs
// توضیح: ثبت احساسات تریدر در لحظات مختلف
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Entities;

/// <summary>
/// ثبت احساسات و وضعیت روحی تریدر
/// </summary>
public class Emotion
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
    /// کاربر مرتبط
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// شناسه معامله (اختیاری - ممکن است احساس مستقل باشد)
    /// </summary>
    public int? TradeId { get; set; }

    /// <summary>
    /// معامله مرتبط
    /// </summary>
    public Trade? Trade { get; set; }

    /// <summary>
    /// زمان ثبت احساس
    /// </summary>
    public DateTime RecordedAt { get; set; }

    /// <summary>
    /// نوع احساس
    /// </summary>
    public EmotionType Type { get; set; }

    /// <summary>
    /// شدت احساس (1-10)
    /// </summary>
    public int Intensity { get; set; } = 5;

    /// <summary>
    /// مرحله معامله
    /// </summary>
    public TradePhase Phase { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// تگ‌ها
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// آیا این احساس منجر به تصمیم اشتباه شد؟
    /// </summary>
    public bool? LedToMistake { get; set; }

    /// <summary>
    /// درس آموخته‌شده
    /// </summary>
    public string? LessonLearned { get; set; }

    /// <summary>
    /// زمان ایجاد رکورد
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// انواع احساسات
/// </summary>
public enum EmotionType
{
    /// <summary>
    /// اعتماد به نفس
    /// </summary>
    Confident = 1,

    /// <summary>
    /// ترس
    /// </summary>
    Fear = 2,

    /// <summary>
    /// طمع
    /// </summary>
    Greed = 3,

    /// <summary>
    /// امید
    /// </summary>
    Hope = 4,

    /// <summary>
    /// ناامیدی
    /// </summary>
    Despair = 5,

    /// <summary>
    /// هیجان
    /// </summary>
    Excitement = 6,

    /// <summary>
    /// اضطراب
    /// </summary>
    Anxiety = 7,

    /// <summary>
    /// عصبانیت
    /// </summary>
    Anger = 8,

    /// <summary>
    /// آرامش
    /// </summary>
    Calm = 9,

    /// <summary>
    /// خستگی
    /// </summary>
    Fatigue = 10,

    /// <summary>
    /// تمرکز
    /// </summary>
    Focused = 11,

    /// <summary>
    /// حواس‌پرتی
    /// </summary>
    Distracted = 12,

    /// <summary>
    /// پشیمانی
    /// </summary>
    Regret = 13,

    /// <summary>
    /// رضایت
    /// </summary>
    Satisfied = 14,

    /// <summary>
    /// FOMO
    /// </summary>
    FOMO = 15,

    /// <summary>
    /// انتقام‌جویی
    /// </summary>
    Revenge = 16,

    /// <summary>
    /// خنثی
    /// </summary>
    Neutral = 17,

    /// <summary>
    /// سایر
    /// </summary>
    Other = 99
}

/// <summary>
/// مرحله معامله برای ثبت احساس
/// </summary>
public enum TradePhase
{
    /// <summary>
    /// قبل از ورود
    /// </summary>
    PreEntry = 1,

    /// <summary>
    /// هنگام ورود
    /// </summary>
    AtEntry = 2,

    /// <summary>
    /// حین معامله
    /// </summary>
    During = 3,

    /// <summary>
    /// هنگام خروج
    /// </summary>
    AtExit = 4,

    /// <summary>
    /// بعد از خروج
    /// </summary>
    PostExit = 5,

    /// <summary>
    /// عمومی (نه مرتبط با معامله خاص)
    /// </summary>
    General = 6
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: Emotion.cs
// ═══════════════════════════════════════════════════════════════════════