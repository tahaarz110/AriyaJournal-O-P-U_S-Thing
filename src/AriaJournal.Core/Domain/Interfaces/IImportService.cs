// ═══════════════════════════════════════════════════════════════════════
// فایل: IImportService.cs
// مسیر: src/AriaJournal.Core/Domain/Interfaces/IImportService.cs
// توضیح: اینترفیس سرویس Import داده
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// سرویس Import داده از فایل‌های خارجی
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Import از فایل CSV
    /// </summary>
    Task<Result<ImportResult>> ImportFromCsvAsync(string filePath, int accountId, ImportOptions? options = null);

    /// <summary>
    /// Import از فایل JSON
    /// </summary>
    Task<Result<ImportResult>> ImportFromJsonAsync(string filePath, int accountId, ImportOptions? options = null);

    /// <summary>
    /// Import از متن CSV
    /// </summary>
    Task<Result<ImportResult>> ImportFromCsvTextAsync(string csvText, int accountId, ImportOptions? options = null);

    /// <summary>
    /// Import از متن JSON
    /// </summary>
    Task<Result<ImportResult>> ImportFromJsonTextAsync(string jsonText, int accountId, ImportOptions? options = null);

    /// <summary>
    /// پیش‌نمایش Import
    /// </summary>
    Task<Result<ImportPreview>> PreviewImportAsync(string filePath, ImportOptions? options = null);

    /// <summary>
    /// دریافت Mapping پیش‌فرض
    /// </summary>
    Dictionary<string, string> GetDefaultMapping();

    /// <summary>
    /// اعتبارسنجی فایل
    /// </summary>
    Task<Result<bool>> ValidateFileAsync(string filePath);

    /// <summary>
    /// پشتیبانی از فرمت
    /// </summary>
    bool SupportsFormat(string extension);
}

/// <summary>
/// تنظیمات Import
/// </summary>
public class ImportOptions
{
    /// <summary>
    /// Mapping ستون‌ها (نام ستون فایل -> نام فیلد)
    /// </summary>
    public Dictionary<string, string>? ColumnMapping { get; set; }

    /// <summary>
    /// آیا ردیف اول Header است؟
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// جداکننده CSV
    /// </summary>
    public char CsvDelimiter { get; set; } = ',';

    /// <summary>
    /// فرمت تاریخ
    /// </summary>
    public string DateFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Timezone
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// آیا معاملات تکراری Skip شوند؟
    /// </summary>
    public bool SkipDuplicates { get; set; } = true;

    /// <summary>
    /// آیا خطاها نادیده گرفته شوند؟
    /// </summary>
    public bool IgnoreErrors { get; set; } = false;

    /// <summary>
    /// حداکثر تعداد خطا قبل از توقف
    /// </summary>
    public int MaxErrors { get; set; } = 100;

    /// <summary>
    /// تگ خودکار برای معاملات Import شده
    /// </summary>
    public string? AutoTag { get; set; }
}

/// <summary>
/// نتیجه Import
/// </summary>
public class ImportResult
{
    /// <summary>
    /// تعداد کل ردیف‌ها
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// تعداد Import موفق
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// تعداد Skip شده (تکراری)
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// تعداد خطا
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// لیست خطاها
    /// </summary>
    public List<ImportError> Errors { get; set; } = new();

    /// <summary>
    /// معاملات Import شده
    /// </summary>
    public List<Trade> ImportedTrades { get; set; } = new();

    /// <summary>
    /// زمان شروع
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// زمان پایان
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// مدت زمان
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;
}

/// <summary>
/// خطای Import
/// </summary>
public class ImportError
{
    /// <summary>
    /// شماره ردیف
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// نام ستون
    /// </summary>
    public string? ColumnName { get; set; }

    /// <summary>
    /// مقدار
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// پیام خطا
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// نوع خطا
    /// </summary>
    public ImportErrorType ErrorType { get; set; }
}

/// <summary>
/// نوع خطای Import
/// </summary>
public enum ImportErrorType
{
    InvalidFormat,
    MissingRequired,
    DuplicateEntry,
    InvalidValue,
    ParseError,
    Unknown
}

/// <summary>
/// پیش‌نمایش Import
/// </summary>
public class ImportPreview
{
    /// <summary>
    /// ستون‌های شناسایی شده
    /// </summary>
    public List<string> DetectedColumns { get; set; } = new();

    /// <summary>
    /// نمونه داده (۵ ردیف اول)
    /// </summary>
    public List<Dictionary<string, string>> SampleData { get; set; } = new();

    /// <summary>
    /// تعداد کل ردیف‌ها
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// Mapping پیشنهادی
    /// </summary>
    public Dictionary<string, string> SuggestedMapping { get; set; } = new();

    /// <summary>
    /// هشدارها
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: IImportService.cs
// ═══════════════════════════════════════════════════════════════════════