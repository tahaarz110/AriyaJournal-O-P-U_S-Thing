// ═══════════════════════════════════════════════════════════════════════
// فایل: IStateEngine.cs
// مسیر: src/AriaJournal.Core/Domain/Interfaces/Engines/IStateEngine.cs
// توضیح: اینترفیس موتور State - بدون StateKeys (منتقل به CoreEvents.cs)
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور مدیریت State مرکزی
/// </summary>
public interface IStateEngine
{
    /// <summary>
    /// دریافت مقدار State
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// تنظیم مقدار State
    /// </summary>
    void Set<T>(string key, T value);

    /// <summary>
    /// بررسی وجود کلید
    /// </summary>
    bool Has(string key);

    /// <summary>
    /// حذف کلید
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// پاک کردن همه State ها
    /// </summary>
    void Clear();

    /// <summary>
    /// اشتراک در تغییرات یک کلید
    /// </summary>
    IDisposable Subscribe<T>(string key, Action<T?> callback);

    /// <summary>
    /// اشتراک در تغییرات با مقدار قبلی
    /// </summary>
    IDisposable Subscribe<T>(string key, Action<T?, T?> callback);

    /// <summary>
    /// دریافت یا ایجاد مقدار پیش‌فرض
    /// </summary>
    T GetOrDefault<T>(string key, T defaultValue);

    /// <summary>
    /// دریافت همه کلیدها
    /// </summary>
    IEnumerable<string> GetAllKeys();
}

// ═══════════════════════════════════════════════════════════════════════
// ⚠️ StateKeys به Domain/Events/CoreEvents.cs منتقل شد
// برای استفاده: using AriaJournal.Core.Domain.Events;
// ═══════════════════════════════════════════════════════════════════════

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: IStateEngine.cs
// ═══════════════════════════════════════════════════════════════════════