// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ILogEngine.cs
// توضیح: اینترفیس موتور لاگ‌گیری
// =============================================================================

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// سطح لاگ
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

/// <summary>
/// رکورد لاگ
/// </summary>
public class LogEntry
{
    /// <summary>
    /// شناسه
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// زمان
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// سطح
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// پیام
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// استثنا
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// منبع (نام کلاس/متد)
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// شناسه کاربر
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// داده اضافی (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// شناسه درخواست (برای ردیابی)
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// اینترفیس موتور لاگ‌گیری
/// </summary>
public interface ILogEngine
{
    /// <summary>
    /// لاگ Trace
    /// </summary>
    void Trace(string message, object? data = null);

    /// <summary>
    /// لاگ Debug
    /// </summary>
    void Debug(string message, object? data = null);

    /// <summary>
    /// لاگ Information
    /// </summary>
    void Info(string message, object? data = null);

    /// <summary>
    /// لاگ Warning
    /// </summary>
    void Warning(string message, object? data = null);

    /// <summary>
    /// لاگ Error
    /// </summary>
    void Error(string message, Exception? exception = null, object? data = null);

    /// <summary>
    /// لاگ Critical
    /// </summary>
    void Critical(string message, Exception? exception = null, object? data = null);

    /// <summary>
    /// لاگ با سطح دلخواه
    /// </summary>
    void Log(LogLevel level, string message, Exception? exception = null, object? data = null);

    /// <summary>
    /// تنظیم شناسه همبستگی
    /// </summary>
    void SetCorrelationId(string correlationId);

    /// <summary>
    /// تنظیم شناسه کاربر
    /// </summary>
    void SetUserId(int userId);

    /// <summary>
    /// دریافت لاگ‌های اخیر
    /// </summary>
    Task<List<LogEntry>> GetRecentLogsAsync(int count = 100, LogLevel? minLevel = null);

    /// <summary>
    /// جستجو در لاگ‌ها
    /// </summary>
    Task<List<LogEntry>> SearchLogsAsync(
        DateTime? from = null, 
        DateTime? to = null, 
        LogLevel? level = null, 
        string? searchText = null,
        int maxResults = 500);

    /// <summary>
    /// پاکسازی لاگ‌های قدیمی
    /// </summary>
    Task<int> PurgeOldLogsAsync(int daysToKeep = 30);

    /// <summary>
    /// خروجی لاگ‌ها به فایل
    /// </summary>
    Task ExportLogsAsync(string filePath, DateTime? from = null, DateTime? to = null);
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/ILogEngine.cs
// =============================================================================