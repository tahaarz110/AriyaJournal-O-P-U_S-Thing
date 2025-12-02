// ═══════════════════════════════════════════════════════════════════════
// فایل: FolderWatcherService.cs
// مسیر: src/AriaJournal.Core/Application/Services/FolderWatcherService.cs
// توضیح: سرویس مانیتور پوشه برای Import خودکار
// ═══════════════════════════════════════════════════════════════════════

using System.IO;
using System.Timers;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using Timer = System.Timers.Timer;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// تنظیمات Watcher
/// </summary>
public class WatcherOptions
{
    public int IntervalSeconds { get; set; } = 60;
    public string FileFilter { get; set; } = "*.*";
    public List<string> AllowedExtensions { get; set; } = new() { ".csv", ".json" };
    public bool DeleteAfterProcess { get; set; } = false;
    public bool MoveAfterProcess { get; set; } = true;
    public string? ProcessedFolder { get; set; }
    public string? ErrorFolder { get; set; }
    public bool IncludeSubfolders { get; set; } = false;
    public long MinFileSize { get; set; } = 0;
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024;
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

/// <summary>
/// سرویس مانیتور پوشه
/// </summary>
public class FolderWatcherService : IDisposable
{
    private readonly ImportService _importService;
    private readonly IEventBusEngine _eventBus;
    
    private Timer? _timer;
    private WatcherOptions _options = new();
    private bool _isDisposed;
    private readonly object _lockObject = new();

    public bool IsWatching { get; private set; }
    public string? CurrentPath { get; private set; }
    public int? CurrentAccountId { get; private set; }
    public DateTime? LastCheckTime { get; private set; }
    public int ProcessedCount { get; private set; }
    public int ErrorCount { get; private set; }

    public event EventHandler<FileDetectedEventArgs>? FileDetected;
    public event EventHandler<FileProcessedEventArgs>? FileProcessed;
    public event EventHandler<FileErrorEventArgs>? FileError;

    public FolderWatcherService(
        ImportService importService,
        IEventBusEngine eventBus)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// شروع مانیتور پوشه
    /// </summary>
    public async Task<Result<bool>> StartWatchingAsync(
        string folderPath, 
        int accountId, 
        WatcherOptions? options = null)
    {
        if (string.IsNullOrEmpty(folderPath))
            return Result.Failure<bool>(Error.Validation("مسیر پوشه مشخص نشده است"));

        if (!Directory.Exists(folderPath))
            return Result.Failure<bool>(Error.NotFound("پوشه یافت نشد"));

        lock (_lockObject)
        {
            if (IsWatching)
            {
                StopWatchingAsync().Wait();
            }

            _options = options ?? new WatcherOptions();
            CurrentPath = folderPath;
            CurrentAccountId = accountId;
            ProcessedCount = 0;
            ErrorCount = 0;

            // ایجاد پوشه‌های کمکی
            if (_options.MoveAfterProcess)
            {
                _options.ProcessedFolder ??= Path.Combine(folderPath, "Processed");
                _options.ErrorFolder ??= Path.Combine(folderPath, "Errors");

                if (!Directory.Exists(_options.ProcessedFolder))
                    Directory.CreateDirectory(_options.ProcessedFolder);

                if (!Directory.Exists(_options.ErrorFolder))
                    Directory.CreateDirectory(_options.ErrorFolder);
            }

            // راه‌اندازی تایمر
            _timer = new Timer(_options.IntervalSeconds * 1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();

            IsWatching = true;
        }

        // پردازش اولیه
        await ProcessAllPendingAsync();

        return Result.Success(true);
    }

    /// <summary>
    /// توقف مانیتور
    /// </summary>
    public async Task StopWatchingAsync()
    {
        lock (_lockObject)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
            }

            IsWatching = false;
            CurrentPath = null;
            CurrentAccountId = null;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// پردازش دستی یک فایل
    /// </summary>
    public async Task<Result<bool>> ProcessFileAsync(string filePath)
    {
        if (!CurrentAccountId.HasValue)
            return Result.Failure<bool>(Error.Validation("حساب مشخص نشده است"));

        try
        {
            var fileInfo = new FileInfo(filePath);

            // بررسی اندازه
            if (fileInfo.Length < _options.MinFileSize || fileInfo.Length > _options.MaxFileSize)
            {
                return Result.Failure<bool>(Error.Validation("اندازه فایل خارج از محدوده مجاز است"));
            }

            // بررسی پسوند
            var extension = fileInfo.Extension.ToLower();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                return Result.Failure<bool>(Error.Validation($"پسوند {extension} پشتیبانی نمی‌شود"));
            }

            var startTime = DateTime.Now;
            Result<ImportResult>? result = null;

            // Import بر اساس نوع فایل
            if (extension == ".csv")
            {
                result = await _importService.ImportFromCsvAsync(
                    filePath, 
                    CurrentAccountId.Value, 
                    _options.ImportOptions);
            }
            else if (extension == ".json")
            {
                result = await _importService.ImportFromJsonAsync(
                    filePath, 
                    CurrentAccountId.Value, 
                    _options.ImportOptions);
            }

            if (result == null || !result.IsSuccess)
            {
                ErrorCount++;
                
                if (_options.MoveAfterProcess && !string.IsNullOrEmpty(_options.ErrorFolder))
                {
                    var errorPath = Path.Combine(_options.ErrorFolder, fileInfo.Name);
                    File.Move(filePath, errorPath, true);
                }

                FileError?.Invoke(this, new FileErrorEventArgs
                {
                    FilePath = filePath,
                    ErrorMessage = result?.Error.Message ?? "خطای نامشخص"
                });

                return Result.Failure<bool>(result?.Error ?? Error.Failure("خطا در Import"));
            }

            ProcessedCount++;

            // انتقال یا حذف فایل
            if (_options.DeleteAfterProcess)
            {
                File.Delete(filePath);
            }
            else if (_options.MoveAfterProcess && !string.IsNullOrEmpty(_options.ProcessedFolder))
            {
                var processedPath = Path.Combine(_options.ProcessedFolder, fileInfo.Name);
                File.Move(filePath, processedPath, true);
            }

            FileProcessed?.Invoke(this, new FileProcessedEventArgs
            {
                FilePath = filePath,
                ImportedCount = result.Value.SuccessCount,
                SkippedCount = result.Value.SkippedCount,
                Duration = DateTime.Now - startTime
            });

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            ErrorCount++;

            FileError?.Invoke(this, new FileErrorEventArgs
            {
                FilePath = filePath,
                ErrorMessage = ex.Message,
                Exception = ex
            });

            return Result.Failure<bool>(Error.Failure($"خطا در پردازش فایل: {ex.Message}"));
        }
    }

    /// <summary>
    /// پردازش همه فایل‌های موجود
    /// </summary>
    public async Task<Result<int>> ProcessAllPendingAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath) || !CurrentAccountId.HasValue)
            return Result.Failure<int>(Error.Validation("مانیتور فعال نیست"));

        try
        {
            var searchOption = _options.IncludeSubfolders 
                ? SearchOption.AllDirectories 
                : SearchOption.TopDirectoryOnly;

            var files = Directory.GetFiles(CurrentPath, _options.FileFilter, searchOption)
                .Where(f => _options.AllowedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            var processedCount = 0;

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                // پرش از پوشه‌های خاص
                if (file.Contains(_options.ProcessedFolder ?? "") || 
                    file.Contains(_options.ErrorFolder ?? ""))
                    continue;

                FileDetected?.Invoke(this, new FileDetectedEventArgs
                {
                    FilePath = file,
                    FileSize = fileInfo.Length,
                    DetectedAt = DateTime.Now
                });

                var result = await ProcessFileAsync(file);
                if (result.IsSuccess)
                {
                    processedCount++;
                }
            }

            LastCheckTime = DateTime.Now;
            return Result.Success(processedCount);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(Error.Failure($"خطا در پردازش: {ex.Message}"));
        }
    }

    private async void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (!IsWatching || string.IsNullOrEmpty(CurrentPath))
            return;

        try
        {
            await ProcessAllPendingAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در Timer: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        StopWatchingAsync().Wait();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: FolderWatcherService.cs
// ═══════════════════════════════════════════════════════════════════════