// ═══════════════════════════════════════════════════════════════════════
// فایل: ScreenshotService.cs
// مسیر: src/AriaJournal.Core/Application/Services/ScreenshotService.cs
// توضیح: سرویس مدیریت تصاویر معاملات
// ═══════════════════════════════════════════════════════════════════════

using System.IO;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// نوع تصویر
/// </summary>
public enum ScreenshotType
{
    Entry = 1,
    HigherTimeframe = 2,
    Management = 3,
    Exit = 4,
    PreAnalysis = 5,
    PostAnalysis = 6,
    Setup = 7,
    Other = 99
}

/// <summary>
/// سرویس مدیریت تصاویر
/// </summary>
public class ScreenshotService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheEngine _cacheEngine;
    private readonly string _baseImagePath;

    public ScreenshotService(
        IUnitOfWork unitOfWork,
        ICacheEngine cacheEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));

        // مسیر پایه برای ذخیره تصاویر
        _baseImagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AriaJournal",
            "Images");

        // اطمینان از وجود پوشه
        if (!Directory.Exists(_baseImagePath))
        {
            Directory.CreateDirectory(_baseImagePath);
        }
    }

    /// <summary>
    /// ذخیره تصویر جدید
    /// </summary>
    public async Task<Result<Screenshot>> SaveScreenshotAsync(
        int tradeId,
        byte[] imageData,
        ScreenshotType type,
        string? description = null)
    {
        try
        {
            // دریافت اطلاعات معامله
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(tradeId);

            if (trade == null)
                return Result.Failure<Screenshot>(Error.TradeNotFound);

            // تولید نام و مسیر فایل
            var fileName = GenerateFileName(trade.Symbol, trade.EntryTime, type);
            var fullPath = GenerateFullPath(trade.AccountId, trade.Symbol, trade.EntryTime, type);

            // اطمینان از وجود پوشه
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // ذخیره فایل
            await File.WriteAllBytesAsync(fullPath, imageData);

            // ایجاد رکورد در دیتابیس
            var screenshot = new Screenshot
            {
                TradeId = tradeId,
                FileName = fileName,
                FilePath = fullPath,
                FileSize = imageData.Length,
                Type = type.ToString(),
                Description = description,
                CreatedAt = DateTime.Now
            };

            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            await screenshotRepo.AddAsync(screenshot);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success(screenshot);
        }
        catch (Exception ex)
        {
            return Result.Failure<Screenshot>(Error.Failure($"خطا در ذخیره تصویر: {ex.Message}"));
        }
    }

    /// <summary>
    /// ذخیره تصویر از مسیر فایل
    /// </summary>
    public async Task<Result<Screenshot>> SaveScreenshotFromFileAsync(
        int tradeId,
        string sourceFilePath,
        ScreenshotType type,
        string? description = null)
    {
        try
        {
            if (!File.Exists(sourceFilePath))
                return Result.Failure<Screenshot>(Error.NotFound("فایل یافت نشد"));

            var imageData = await File.ReadAllBytesAsync(sourceFilePath);
            return await SaveScreenshotAsync(tradeId, imageData, type, description);
        }
        catch (Exception ex)
        {
            return Result.Failure<Screenshot>(Error.Failure($"خطا در خواندن فایل: {ex.Message}"));
        }
    }

    /// <summary>
    /// دریافت تصویر
    /// </summary>
    public async Task<Result<byte[]>> GetScreenshotDataAsync(int screenshotId)
    {
        try
        {
            // بررسی کش
            var cacheKey = $"screenshot:{screenshotId}";
            var cached = _cacheEngine.Get<byte[]>(cacheKey);
            if (cached != null)
                return Result.Success(cached);

            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var screenshot = await screenshotRepo.GetByIdAsync(screenshotId);

            if (screenshot == null)
                return Result.Failure<byte[]>(Error.NotFound("تصویر یافت نشد"));

            if (!File.Exists(screenshot.FilePath))
                return Result.Failure<byte[]>(Error.NotFound("فایل تصویر یافت نشد"));

            var data = await File.ReadAllBytesAsync(screenshot.FilePath);

            // کش کردن
            _cacheEngine.Set(cacheKey, data, TimeSpan.FromMinutes(5));

            return Result.Success(data);
        }
        catch (Exception ex)
        {
            return Result.Failure<byte[]>(Error.Failure($"خطا در خواندن تصویر: {ex.Message}"));
        }
    }

    /// <summary>
    /// دریافت مسیر تصویر
    /// </summary>
    public async Task<Result<string>> GetScreenshotPathAsync(int screenshotId)
    {
        try
        {
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var screenshot = await screenshotRepo.GetByIdAsync(screenshotId);

            if (screenshot == null)
                return Result.Failure<string>(Error.NotFound("تصویر یافت نشد"));

            return Result.Success(screenshot.FilePath);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(Error.Failure($"خطا: {ex.Message}"));
        }
    }

    /// <summary>
    /// دریافت تصاویر یک معامله
    /// </summary>
    public async Task<Result<List<Screenshot>>> GetTradeScreenshotsAsync(int tradeId)
    {
        try
        {
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var screenshots = await screenshotRepo.GetAllAsync(s => s.TradeId == tradeId);

            return Result.Success(screenshots.OrderBy(s => s.CreatedAt).ToList());
        }
        catch (Exception ex)
        {
            return Result.Failure<List<Screenshot>>(Error.Failure($"خطا: {ex.Message}"));
        }
    }

    /// <summary>
    /// حذف تصویر
    /// </summary>
    public async Task<Result<bool>> DeleteScreenshotAsync(int screenshotId)
    {
        try
        {
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var screenshot = await screenshotRepo.GetByIdAsync(screenshotId);

            if (screenshot == null)
                return Result.Failure<bool>(Error.NotFound("تصویر یافت نشد"));

            // حذف فایل
            if (File.Exists(screenshot.FilePath))
            {
                File.Delete(screenshot.FilePath);
            }

            // حذف از دیتابیس
            screenshotRepo.Delete(screenshot);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"screenshot:{screenshotId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Failure($"خطا در حذف تصویر: {ex.Message}"));
        }
    }

    /// <summary>
    /// به‌روزرسانی توضیحات
    /// </summary>
    public async Task<Result<bool>> UpdateDescriptionAsync(int screenshotId, string description)
    {
        try
        {
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var screenshot = await screenshotRepo.GetByIdAsync(screenshotId);

            if (screenshot == null)
                return Result.Failure<bool>(Error.NotFound("تصویر یافت نشد"));

            screenshot.Description = description;
            screenshot.UpdatedAt = DateTime.Now;

            screenshotRepo.Update(screenshot);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Failure($"خطا: {ex.Message}"));
        }
    }

    /// <summary>
    /// تولید نام فایل استاندارد
    /// </summary>
    public string GenerateFileName(string symbol, DateTime time, ScreenshotType type)
    {
        var typeName = type switch
        {
            ScreenshotType.Entry => "entry",
            ScreenshotType.HigherTimeframe => "htf",
            ScreenshotType.Management => "manage",
            ScreenshotType.Exit => "exit",
            ScreenshotType.PreAnalysis => "pre",
            ScreenshotType.PostAnalysis => "post",
            ScreenshotType.Setup => "setup",
            _ => "other"
        };

        return $"{symbol}-{time:yyyyMMdd-HHmm}-{typeName}.png";
    }

    /// <summary>
    /// تولید مسیر کامل
    /// </summary>
    public string GenerateFullPath(int accountId, string symbol, DateTime time, ScreenshotType type)
    {
        var fileName = GenerateFileName(symbol, time, type);
        
        // ساختار: images/{accountId}/{symbol}/{YYYY}/{MM}/filename.png
        return Path.Combine(
            _baseImagePath,
            accountId.ToString(),
            symbol.ToUpperInvariant(),
            time.Year.ToString(),
            time.Month.ToString("D2"),
            fileName);
    }

    /// <summary>
    /// بررسی وجود تصویر
    /// </summary>
    public async Task<bool> ExistsAsync(int screenshotId)
    {
        var screenshotRepo = _unitOfWork.Repository<Screenshot>();
        return await screenshotRepo.AnyAsync(s => s.Id == screenshotId);
    }

    /// <summary>
    /// پاکسازی تصاویر بدون معامله
    /// </summary>
    public async Task<Result<int>> CleanupOrphanedScreenshotsAsync()
    {
        try
        {
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            var tradeRepo = _unitOfWork.Repository<Trade>();

            var allScreenshots = await screenshotRepo.GetAllAsync();
            var deletedCount = 0;

            foreach (var screenshot in allScreenshots)
            {
                var tradeExists = await tradeRepo.AnyAsync(t => t.Id == screenshot.TradeId);
                
                if (!tradeExists)
                {
                    // حذف فایل
                    if (File.Exists(screenshot.FilePath))
                    {
                        File.Delete(screenshot.FilePath);
                    }

                    // حذف رکورد
                    screenshotRepo.Delete(screenshot);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return Result.Success(deletedCount);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(Error.Failure($"خطا در پاکسازی: {ex.Message}"));
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: ScreenshotService.cs
// ═══════════════════════════════════════════════════════════════════════