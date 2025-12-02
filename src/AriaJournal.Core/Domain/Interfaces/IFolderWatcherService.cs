// ═══════════════════════════════════════════════════════════════════════
// فایل: IFolderWatcherService.cs
// مسیر: src/AriaJournal.Core/Domain/Interfaces/IFolderWatcherService.cs
// توضیح: اینترفیس مانیتور پوشه برای Import خودکار
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Common;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// سرویس مانیتور پوشه برای دریافت خودکار فایل‌ها
/// </summary>
public interface IFolderWatcherService : IDisposable
{
    /// <summary>
    /// شروع مانیتور پوشه
    /// </summary>
    Task<Result<bool>> StartWatchingAsync(string folderPath, int accountId, WatcherOptions? options = null);

    /// <summary>
    /// توقف مانیتور
    /// </summary>
    Task StopWatchingAsync();

    /// <summary>
    /// آیا در حال مانیتور است؟
    /// </summary>
    bool IsWatching { get; }

    /// <summary>
    /// مسیر پوشه فعلی
    /// </summary>
    string? CurrentPath { get; }

    /// <summary>
    /// شناسه حساب فعلی
    /// </summary>
    int? CurrentAccountId { get; }

    /// <summary>
    /// پردازش دستی یک فایل
    /// </summary>
    Task<Result<bool>> ProcessFileAsync(string filePath);

    /// <summary>
    /// پردازش همه فایل‌های موجود در پوشه
    /// </summary>
    Task<Result<int>> ProcessAllPendingAsync();

    /// <summary>
    /// رویداد فایل جدید
    /// </summary>
    event EventHandler<FileDetectedEventArgs>? FileDetected;

    /// <summary>
    /// رویداد پردازش موفق
    /// </summary>
    event EventHandler<FileProcessedEventArgs>? FileProcessed;

    /// <summary>
    /// رویداد خطا
    /// </summary>
    event EventHandler<FileErrorEventArgs>? FileError;

    /// <summary>
    /// آخرین زمان چک
    /// </summary>
    DateTime? LastCheckTime { get; }

    /// <summary>
    /// تعداد فایل‌های پردازش شده
    /// </summary>
    int ProcessedCount { get; }

    /// <summary>
    /// تعداد خطاها
    /// </summary>
    int ErrorCount { get; }
}

/// <summary>
/// تنظیمات Watcher
/// </summary>
public class WatcherOptions
{
    /// <summary>
    /// بازه زمانی چک (ثانیه)
    /// </summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>
    /// فیلتر فایل‌ها (مثلاً *.csv)
    /// </summary>
    public string FileFilter { get; set; } = "*.*";

    /// <summary>
    /// پسوندهای مجاز
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new() { ".csv", ".json" };

    /// <summary>
    /// آیا فایل‌ها پس از پردازش حذف شوند؟
    /// </summary>
    public bool DeleteAfterProcess { get; set; } = false;

    /// <summary>
    /// آیا فایل‌ها پس از پردازش به پوشه دیگری منتقل شوند؟
    /// </summary>
    public bool MoveAfterProcess { get; set; } = true;

    /// <summary>
    /// مسیر پوشه مقصد (برای انتقال)
    /// </summary>
    public string? ProcessedFolder { get; set; }

    /// <summary>
    /// مسیر پوشه خطا
    /// </summary>
    public string? ErrorFolder { get; set; }

    /// <summary>
    /// آیا زیرپوشه‌ها هم چک شوند؟
    /// </summary>
    public bool IncludeSubfolders { get; set; } = false;

    /// <summary>
    /// حداقل اندازه فایل (بایت)
    /// </summary>
    public long MinFileSize { get; set; } = 0;

    /// <summary>
    /// حداکثر اندازه فایل (بایت)
    /// </summary>
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// تنظیمات Import
    /// </summary>
    public ImportOptions? ImportOptions { get; set; }
}

/// <summary>
/// EventArgs برای فایل شناسایی شده
/// </summary>
public class FileDetectedEventArgs : EventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime DetectedAt { get; set; }
}

/// <summary>
/// EventArgs برای فایل پردازش شده
/// </summary>
public class FileProcessedEventArgs : EventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// EventArgs برای خطا
/// </summary>
public class FileErrorEventArgs : EventArgs
{
    public string FilePath { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: IFolderWatcherService.cs
// ═══════════════════════════════════════════════════════════════════════