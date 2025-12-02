// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ICacheEngine.cs
// توضیح: اینترفیس موتور کش - نسخه اصلاح‌شده با RemoveByPattern
// =============================================================================

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// اینترفیس موتور کش
/// </summary>
public interface ICacheEngine
{
    /// <summary>
    /// دریافت مقدار از کش
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// ذخیره مقدار در کش
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// حذف از کش
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// حذف بر اساس الگو (مثلاً schema:*)
    /// </summary>
    void RemoveByPattern(string pattern);

    /// <summary>
    /// پاک کردن همه کش
    /// </summary>
    void Clear();

    /// <summary>
    /// بررسی وجود کلید
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// دریافت یا ایجاد (اگر نبود، تابع را صدا بزن و کش کن)
    /// </summary>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null);

    /// <summary>
    /// دریافت یا ایجاد (نسخه async)
    /// </summary>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null);

    /// <summary>
    /// دریافت آمار کش
    /// </summary>
    CacheStatistics GetStatistics();

    /// <summary>
    /// دریافت همه کلیدها
    /// </summary>
    IEnumerable<string> GetAllKeys();
}

/// <summary>
/// آمار کش
/// </summary>
public class CacheStatistics
{
    /// <summary>
    /// تعداد آیتم‌ها
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// تعداد Hit ها
    /// </summary>
    public long HitCount { get; set; }

    /// <summary>
    /// تعداد Miss ها
    /// </summary>
    public long MissCount { get; set; }

    /// <summary>
    /// نرخ Hit
    /// </summary>
    public double HitRate => HitCount + MissCount > 0 
        ? (double)HitCount / (HitCount + MissCount) * 100 
        : 0;

    /// <summary>
    /// حجم تخمینی (بایت)
    /// </summary>
    public long EstimatedSizeBytes { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ICacheEngine.cs
// =============================================================================