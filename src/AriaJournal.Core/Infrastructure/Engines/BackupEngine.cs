// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/BackupEngine.cs
// شماره فایل: 61
// توضیح: موتور پشتیبان‌گیری
// =============================================================================
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;
using AriaJournal.Core.Infrastructure.Security;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور پشتیبان‌گیری
/// </summary>
public class BackupEngine : IBackupEngine
{
    private readonly AriaDbContext _dbContext;
    private readonly IEventBusEngine _eventBus;
    private string _backupPath;
    private const string BackupExtension = ".ariabackup";
    private const string EncryptedExtension = ".ariasecure";

    public string BackupPath => _backupPath;

    public BackupEngine(AriaDbContext dbContext, IEventBusEngine eventBus)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        // مسیر پیش‌فرض
        _backupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AriaJournal",
            "Backups");

        Directory.CreateDirectory(_backupPath);
    }

    public async Task<Result<BackupResult>> CreateBackupAsync(string? password = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var isEncrypted = !string.IsNullOrWhiteSpace(password);
            var extension = isEncrypted ? EncryptedExtension : BackupExtension;
            var fileName = $"AriaJournal_Backup_{timestamp}{extension}";
            var filePath = Path.Combine(_backupPath, fileName);

            // کپی دیتابیس
            var dbPath = _dbContext.GetDatabasePath();
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"aria_temp_{Guid.NewGuid()}.db");

            // Checkpoint برای اطمینان از ذخیره تمام تغییرات
            await _dbContext.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");

            File.Copy(dbPath, tempDbPath, true);

            // ایجاد فایل ZIP
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"aria_backup_{Guid.NewGuid()}.zip");

            using (var zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
            {
                // افزودن دیتابیس
                zip.CreateEntryFromFile(tempDbPath, "aria.db");

                // افزودن اطلاعات بکاپ
                var backupInfo = new BackupMetadata
                {
                    Version = "1.0",
                    CreatedAt = DateTime.Now,
                    AppVersion = "1.0.0",
                    IsEncrypted = isEncrypted
                };

                var entry = zip.CreateEntry("backup.json");
                using (var writer = new StreamWriter(entry.Open()))
                {
                    await writer.WriteAsync(JsonSerializer.Serialize(backupInfo));
                }

                // افزودن پوشه تصاویر اگر وجود دارد
                var imagesPath = Path.Combine(AriaDbContext.GetAppDataPath(), "Screenshots");
                if (Directory.Exists(imagesPath))
                {
                    foreach (var imgFile in Directory.GetFiles(imagesPath, "*.*", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(imagesPath, imgFile);
                        zip.CreateEntryFromFile(imgFile, $"Screenshots/{relativePath}");
                    }
                }
            }

            // رمزنگاری در صورت نیاز
            if (isEncrypted)
            {
                await AesEncryption.EncryptFileAsync(tempZipPath, filePath, password!);
                File.Delete(tempZipPath);
            }
            else
            {
                File.Move(tempZipPath, filePath, true);
            }

            // پاکسازی
            File.Delete(tempDbPath);

            var fileInfo = new FileInfo(filePath);
            var result = new BackupResult
            {
                FilePath = filePath,
                FileName = fileName,
                FileSize = fileInfo.Length,
                CreatedAt = DateTime.Now,
                IsEncrypted = isEncrypted
            };

            _eventBus.Publish(new BackupCreatedEvent(filePath, DateTime.Now));

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد بکاپ: {ex.Message}");
            return Result.Failure<BackupResult>(Error.BackupFailed);
        }
    }

    public async Task<Result<RestoreResult>> RestoreAsync(string path, string? password = null)
    {
        if (!File.Exists(path))
        {
            return Result.Failure<RestoreResult>(Error.FileNotFound);
        }

        try
        {
            var isEncrypted = path.EndsWith(EncryptedExtension);

            // اگر رمزنگاری شده، رمزگشایی کن
            var zipPath = path;
            if (isEncrypted)
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return Result.Failure<RestoreResult>(Error.Validation("رمز عبور برای بازیابی لازم است"));
                }

                zipPath = Path.Combine(Path.GetTempPath(), $"aria_restore_{Guid.NewGuid()}.zip");
                await AesEncryption.DecryptFileAsync(path, zipPath, password!);
            }

            // استخراج به پوشه موقت
            var extractPath = Path.Combine(Path.GetTempPath(), $"aria_extract_{Guid.NewGuid()}");
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // خواندن metadata
            var metadataPath = Path.Combine(extractPath, "backup.json");
            BackupMetadata? metadata = null;
            if (File.Exists(metadataPath))
            {
                var json = await File.ReadAllTextAsync(metadataPath);
                metadata = JsonSerializer.Deserialize<BackupMetadata>(json);
            }

            // بستن اتصال فعلی
            await _dbContext.Database.GetDbConnection().CloseAsync();

            // جایگزینی دیتابیس
            var dbPath = _dbContext.GetDatabasePath();
            var backupDbPath = Path.Combine(extractPath, "aria.db");

            if (File.Exists(backupDbPath))
            {
                // بکاپ از دیتابیس فعلی
                var currentBackupPath = dbPath + ".before_restore";
                if (File.Exists(dbPath))
                {
                    File.Copy(dbPath, currentBackupPath, true);
                }

                File.Copy(backupDbPath, dbPath, true);
            }

            // بازیابی تصاویر
            var screenshotsPath = Path.Combine(extractPath, "Screenshots");
            if (Directory.Exists(screenshotsPath))
            {
                var targetPath = Path.Combine(AriaDbContext.GetAppDataPath(), "Screenshots");
                Directory.CreateDirectory(targetPath);

                foreach (var file in Directory.GetFiles(screenshotsPath, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(screenshotsPath, file);
                    var targetFile = Path.Combine(targetPath, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile)!);
                    File.Copy(file, targetFile, true);
                }
            }

            // پاکسازی
            Directory.Delete(extractPath, true);
            if (isEncrypted && zipPath != path)
            {
                File.Delete(zipPath);
            }

            // بازگشایی اتصال
            await _dbContext.Database.GetDbConnection().OpenAsync();

            var result = new RestoreResult
            {
                Success = true,
                BackupDate = metadata?.CreatedAt ?? DateTime.Now,
                RestoredUsers = 0,
                RestoredAccounts = 0,
                RestoredTrades = 0
            };

            _eventBus.Publish(new BackupRestoredEvent(path));

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بازیابی: {ex.Message}");
            return Result.Failure<RestoreResult>(Error.RestoreFailed);
        }
    }

    public List<BackupInfo> GetBackups()
    {
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(_backupPath))
            return backups;

        var files = Directory.GetFiles(_backupPath)
            .Where(f => f.EndsWith(BackupExtension) || f.EndsWith(EncryptedExtension))
            .ToList();

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupInfo
            {
                FilePath = file,
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length,
                CreatedAt = fileInfo.CreationTime,
                IsEncrypted = file.EndsWith(EncryptedExtension)
            });
        }

        return backups.OrderByDescending(b => b.CreatedAt).ToList();
    }

    public async Task<Result<bool>> DeleteBackupAsync(string path)
    {
        if (!File.Exists(path))
        {
            return Result.Failure<bool>(Error.FileNotFound);
        }

        try
        {
            File.Delete(path);
            return await Task.FromResult(Result.Success(true));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در حذف بکاپ: {ex.Message}");
            return Result.Failure<bool>(Error.DeleteFailed);
        }
    }

    public async Task<Result<bool>> ValidateBackupAsync(string path, string? password = null)
    {
        if (!File.Exists(path))
        {
            return Result.Failure<bool>(Error.FileNotFound);
        }

        try
        {
            var isEncrypted = path.EndsWith(EncryptedExtension);

            if (isEncrypted && string.IsNullOrWhiteSpace(password))
            {
                return Result.Failure<bool>(Error.Validation("این فایل رمزنگاری شده است"));
            }

            // سعی در خواندن فایل
            var zipPath = path;
            if (isEncrypted)
            {
                zipPath = Path.Combine(Path.GetTempPath(), $"aria_validate_{Guid.NewGuid()}.zip");
                await AesEncryption.DecryptFileAsync(path, zipPath, password!);
            }

            using var archive = ZipFile.OpenRead(zipPath);
            var hasDb = archive.Entries.Any(e => e.Name == "aria.db");
            var hasMetadata = archive.Entries.Any(e => e.Name == "backup.json");

            if (isEncrypted && zipPath != path)
            {
                File.Delete(zipPath);
            }

            if (!hasDb)
            {
                return Result.Failure<bool>(Error.InvalidFileFormat);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در اعتبارسنجی بکاپ: {ex.Message}");
            return Result.Failure<bool>(Error.InvalidFileFormat);
        }
    }

    public void SetBackupPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        _backupPath = path;
        Directory.CreateDirectory(_backupPath);
    }

    public async Task<Result<BackupResult>> AutoBackupAsync()
    {
        return await CreateBackupAsync(null);
    }

    public bool NeedsAutoBackup(int intervalDays)
    {
        var backups = GetBackups();
        if (!backups.Any())
            return true;

        var lastBackup = backups.First();
        var daysSinceLastBackup = (DateTime.Now - lastBackup.CreatedAt).TotalDays;

        return daysSinceLastBackup >= intervalDays;
    }
}

/// <summary>
/// اطلاعات متادیتا بکاپ
/// </summary>
internal class BackupMetadata
{
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; }
    public string AppVersion { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
}

// =============================================================================
// پایان فایل
// =============================================================================