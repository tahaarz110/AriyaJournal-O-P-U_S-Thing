// ═══════════════════════════════════════════════════════════════════════
// فایل: IScreenshotService.cs
// مسیر: src/AriaJournal.Core/Domain/Interfaces/IScreenshotService.cs
// توضیح: اینترفیس مدیریت تصاویر معاملات
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// سرویس مدیریت تصاویر (Screenshot) معاملات
/// </summary>
public interface IScreenshotService
{
    /// <summary>
    /// ذخیره تصویر جدید
    /// </summary>
    Task<Result<Screenshot>> SaveScreenshotAsync(
        int tradeId,
        byte[] imageData,
        ScreenshotType type,
        string? description = null);

    /// <summary>
    /// ذخیره تصویر از مسیر فایل
    /// </summary>
    Task<Result<Screenshot>> SaveScreenshotFromFileAsync(
        int tradeId,
        string sourceFilePath,
        ScreenshotType type,
        string? description = null);

    /// <summary>
    /// ذخیره تصویر از Clipboard
    /// </summary>
    Task<Result<Screenshot>> SaveScreenshotFromClipboardAsync(
        int tradeId,
        ScreenshotType type,
        string? description = null);

    /// <summary>
    /// دریافت تصویر
    /// </summary>
    Task<Result<byte[]>> GetScreenshotDataAsync(int screenshotId);

    /// <summary>
    /// دریافت مسیر تصویر
    /// </summary>
    Task<Result<string>> GetScreenshotPathAsync(int screenshotId);

    /// <summary>
    /// دریافت تصاویر یک معامله
    /// </summary>
    Task<Result<List<Screenshot>>> GetTradeScreenshotsAsync(int tradeId);

    /// <summary>
    /// حذف تصویر
    /// </summary>
    Task<Result<bool>> DeleteScreenshotAsync(int screenshotId);

    /// <summary>
    /// به‌روزرسانی توضیحات
    /// </summary>
    Task<Result<bool>> UpdateDescriptionAsync(int screenshotId, string description);

    /// <summary>
    /// تغییر نوع تصویر
    /// </summary>
    Task<Result<bool>> UpdateTypeAsync(int screenshotId, ScreenshotType type);

    /// <summary>
    /// تولید نام فایل استاندارد
    /// </summary>
    string GenerateFileName(string symbol, DateTime time, ScreenshotType type);

    /// <summary>
    /// تولید مسیر کامل
    /// </summary>
    string GenerateFullPath(int accountId, string symbol, DateTime time, ScreenshotType type);

    /// <summary>
    /// بررسی وجود تصویر
    /// </summary>
    Task<bool> ExistsAsync(int screenshotId);

    /// <summary>
    /// تغییر اندازه تصویر
    /// </summary>
    Task<Result<byte[]>> ResizeImageAsync(byte[] imageData, int maxWidth, int maxHeight);

    /// <summary>
    /// تولید Thumbnail
    /// </summary>
    Task<Result<byte[]>> GenerateThumbnailAsync(int screenshotId, int width = 200, int height = 150);

    /// <summary>
    /// پاکسازی تصاویر بدون معامله
    /// </summary>
    Task<Result<int>> CleanupOrphanedScreenshotsAsync();
}

/// <summary>
/// نوع تصویر
/// </summary>
public enum ScreenshotType
{
    /// <summary>
    /// لحظه ورود
    /// </summary>
    Entry = 1,

    /// <summary>
    /// تایم‌فریم بالاتر
    /// </summary>
    HigherTimeframe = 2,

    /// <summary>
    /// لحظه مدیریت
    /// </summary>
    Management = 3,

    /// <summary>
    /// لحظه خروج
    /// </summary>
    Exit = 4,

    /// <summary>
    /// تحلیل قبل از ورود
    /// </summary>
    PreAnalysis = 5,

    /// <summary>
    /// تحلیل بعد از خروج
    /// </summary>
    PostAnalysis = 6,

    /// <summary>
    /// ستاپ
    /// </summary>
    Setup = 7,

    /// <summary>
    /// سایر
    /// </summary>
    Other = 99
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: IScreenshotService.cs
// ═══════════════════════════════════════════════════════════════════════