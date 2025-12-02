// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IBackupEngine.cs
// شماره فایل: 34
// =============================================================================

using AriaJournal.Core.Domain.Common;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور پشتیبان‌گیری
/// </summary>
public interface IBackupEngine
{
    /// <summary>
    /// ایجاد نسخه پشتیبان
    /// </summary>
    Task<Result<BackupResult>> CreateBackupAsync(string? password = null);

    /// <summary>
    /// بازیابی از نسخه پشتیبان
    /// </summary>
    Task<Result<RestoreResult>> RestoreAsync(string path, string? password = null);

    /// <summary>
    /// دریافت لیست نسخه‌های پشتیبان
    /// </summary>
    List<BackupInfo> GetBackups();

    /// <summary>
    /// حذف نسخه پشتیبان
    /// </summary>
    Task<Result<bool>> DeleteBackupAsync(string path);

    /// <summary>
    /// بررسی اعتبار فایل پشتیبان
    /// </summary>
    Task<Result<bool>> ValidateBackupAsync(string path, string? password = null);

    /// <summary>
    /// تنظیم مسیر پشتیبان‌گیری
    /// </summary>
    void SetBackupPath(string path);

    /// <summary>
    /// مسیر فعلی پشتیبان‌گیری
    /// </summary>
    string BackupPath { get; }

    /// <summary>
    /// پشتیبان‌گیری خودکار
    /// </summary>
    Task<Result<BackupResult>> AutoBackupAsync();

    /// <summary>
    /// آیا نیاز به پشتیبان‌گیری خودکار است
    /// </summary>
    bool NeedsAutoBackup(int intervalDays);
}

/// <summary>
/// نتیجه پشتیبان‌گیری
/// </summary>
public class BackupResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEncrypted { get; set; }
}

/// <summary>
/// نتیجه بازیابی
/// </summary>
public class RestoreResult
{
    public bool Success { get; set; }
    public int RestoredUsers { get; set; }
    public int RestoredAccounts { get; set; }
    public int RestoredTrades { get; set; }
    public DateTime BackupDate { get; set; }
}

/// <summary>
/// اطلاعات فایل پشتیبان
/// </summary>
public class BackupInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsEncrypted { get; set; }
    public string? Description { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IBackupEngine.cs
// =============================================================================