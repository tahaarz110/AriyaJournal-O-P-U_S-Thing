// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/LogEngine.cs
// توضیح: پیاده‌سازی موتور لاگ‌گیری
// =============================================================================

using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور لاگ‌گیری
/// </summary>
public class LogEngine : ILogEngine, IDisposable
{
    private readonly AriaDbContext _dbContext;
    private readonly ConcurrentQueue<LogEntry> _logQueue;
    private readonly Timer _flushTimer;
    private readonly SemaphoreSlim _flushLock;
    private readonly string _logFilePath;
    
    private string? _correlationId;
    private int? _userId;
    private bool _disposed;

    private const int FlushIntervalMs = 5000; // هر 5 ثانیه
    private const int MaxQueueSize = 1000;

    public LogEngine(AriaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logQueue = new ConcurrentQueue<LogEntry>();
        _flushLock = new SemaphoreSlim(1, 1);

        // مسیر فایل لاگ
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        var logsPath = Path.Combine(appPath, "logs");
        Directory.CreateDirectory(logsPath);
        _logFilePath = Path.Combine(logsPath, $"aria_{DateTime.Now:yyyyMMdd}.log");

        // تایمر برای flush خودکار
        _flushTimer = new Timer(
            async _ => await FlushAsync(),
            null,
            FlushIntervalMs,
            FlushIntervalMs);
    }

    public void Trace(string message, object? data = null)
        => Log(LogLevel.Trace, message, null, data);

    public void Debug(string message, object? data = null)
        => Log(LogLevel.Debug, message, null, data);

    public void Info(string message, object? data = null)
        => Log(LogLevel.Information, message, null, data);

    public void Warning(string message, object? data = null)
        => Log(LogLevel.Warning, message, null, data);

    public void Error(string message, Exception? exception = null, object? data = null)
        => Log(LogLevel.Error, message, exception, data);

    public void Critical(string message, Exception? exception = null, object? data = null)
        => Log(LogLevel.Critical, message, exception, data);

    public void Log(LogLevel level, string message, Exception? exception = null, object? data = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Exception = exception?.ToString(),
            Source = GetCallerInfo(),
            UserId = _userId,
            CorrelationId = _correlationId,
            AdditionalData = data != null ? JsonSerializer.Serialize(data) : null
        };

        // اضافه به صف
        _logQueue.Enqueue(entry);

        // نوشتن در فایل (async)
        WriteToFileAsync(entry);

        // نوشتن در Debug Output
        System.Diagnostics.Debug.WriteLine($"[{entry.Level}] {entry.Message}");

        // اگر صف پر شد، flush کن
        if (_logQueue.Count >= MaxQueueSize)
        {
            Task.Run(FlushAsync);
        }
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
    }

    public void SetUserId(int userId)
    {
        _userId = userId;
    }

    public async Task<List<LogEntry>> GetRecentLogsAsync(int count = 100, LogLevel? minLevel = null)
    {
        // اول flush کن
        await FlushAsync();

        var query = _dbContext.Set<LogEntry>().AsQueryable();

        if (minLevel.HasValue)
        {
            query = query.Where(l => l.Level >= minLevel.Value);
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<LogEntry>> SearchLogsAsync(
        DateTime? from = null,
        DateTime? to = null,
        LogLevel? level = null,
        string? searchText = null,
        int maxResults = 500)
    {
        await FlushAsync();

        var query = _dbContext.Set<LogEntry>().AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        if (level.HasValue)
            query = query.Where(l => l.Level == level.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(l => l.Message.Contains(searchText) || 
                                     (l.Source != null && l.Source.Contains(searchText)));

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<int> PurgeOldLogsAsync(int daysToKeep = 30)
    {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        
        var oldLogs = await _dbContext.Set<LogEntry>()
            .Where(l => l.Timestamp < cutoffDate)
            .ToListAsync();

        if (oldLogs.Any())
        {
            _dbContext.Set<LogEntry>().RemoveRange(oldLogs);
            await _dbContext.SaveChangesAsync();
        }

        // پاکسازی فایل‌های لاگ قدیمی
        var logsPath = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(logsPath) && Directory.Exists(logsPath))
        {
            var oldFiles = Directory.GetFiles(logsPath, "aria_*.log")
                .Where(f => File.GetCreationTime(f) < cutoffDate)
                .ToList();

            foreach (var file in oldFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }

        return oldLogs.Count;
    }

    public async Task ExportLogsAsync(string filePath, DateTime? from = null, DateTime? to = null)
    {
        var logs = await SearchLogsAsync(from, to, maxResults: int.MaxValue);

        await using var writer = new StreamWriter(filePath);
        await writer.WriteLineAsync("Timestamp,Level,Source,Message,Exception,UserId,CorrelationId");

        foreach (var log in logs)
        {
            var line = $"\"{log.Timestamp:yyyy-MM-dd HH:mm:ss}\"," +
                      $"\"{log.Level}\"," +
                      $"\"{EscapeCsv(log.Source)}\"," +
                      $"\"{EscapeCsv(log.Message)}\"," +
                      $"\"{EscapeCsv(log.Exception)}\"," +
                      $"\"{log.UserId}\"," +
                      $"\"{log.CorrelationId}\"";
            
            await writer.WriteLineAsync(line);
        }
    }

    private async Task FlushAsync()
    {
        if (_disposed || _logQueue.IsEmpty)
            return;

        await _flushLock.WaitAsync();
        try
        {
            var entries = new List<LogEntry>();
            while (_logQueue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            if (entries.Any())
            {
                _dbContext.Set<LogEntry>().AddRange(entries);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در flush لاگ: {ex.Message}");
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private async void WriteToFileAsync(LogEntry entry)
    {
        try
        {
            var line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level}] [{entry.Source}] {entry.Message}";
            if (entry.Exception != null)
            {
                line += $"\n{entry.Exception}";
            }
            line += "\n";

            await File.AppendAllTextAsync(_logFilePath, line);
        }
        catch
        {
            // نادیده گرفتن خطا در نوشتن فایل
        }
    }

    private string GetCallerInfo()
    {
        var stackTrace = new System.Diagnostics.StackTrace(3, true);
        var frame = stackTrace.GetFrame(0);
        if (frame == null) return "Unknown";

        var method = frame.GetMethod();
        var className = method?.DeclaringType?.Name ?? "Unknown";
        var methodName = method?.Name ?? "Unknown";

        return $"{className}.{methodName}";
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        
        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _flushTimer.Dispose();
        
        // flush نهایی
        FlushAsync().GetAwaiter().GetResult();
        
        _flushLock.Dispose();
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/LogEngine.cs
// =============================================================================