// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/IAriaPlugin.cs
// شماره فایل: 22
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// اینترفیس پلاگین آریا ژورنال
/// </summary>
public interface IAriaPlugin
{
    #region شناسه و اطلاعات

    /// <summary>
    /// شناسه یکتای پلاگین
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// نام پلاگین
    /// </summary>
    string PluginName { get; }

    /// <summary>
    /// نسخه پلاگین
    /// </summary>
    string Version { get; }

    /// <summary>
    /// توضیحات پلاگین
    /// </summary>
    string Description { get; }

    /// <summary>
    /// نام توسعه‌دهنده
    /// </summary>
    string Author { get; }

    #endregion

    #region وابستگی‌ها

    /// <summary>
    /// لیست شناسه پلاگین‌های وابسته
    /// </summary>
    string[] Dependencies { get; }

    #endregion

    #region منو

    /// <summary>
    /// عنوان منو (فارسی)
    /// </summary>
    string MenuTitle { get; }

    /// <summary>
    /// آیکون منو
    /// </summary>
    string MenuIcon { get; }

    /// <summary>
    /// ترتیب نمایش در منو
    /// </summary>
    int MenuOrder { get; }

    /// <summary>
    /// آیا در منو نمایش داده شود
    /// </summary>
    bool ShowInMenu { get; }

    #endregion

    #region UI

    /// <summary>
    /// نوع View اصلی پلاگین
    /// </summary>
    Type? MainViewType { get; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// راه‌اندازی پلاگین
    /// </summary>
    Task InitializeAsync(IServiceProvider services);

    /// <summary>
    /// خاموش کردن پلاگین
    /// </summary>
    Task ShutdownAsync();

    /// <summary>
    /// ثبت سرویس‌های پلاگین
    /// </summary>
    void RegisterServices(IServiceCollection services);

    /// <summary>
    /// پیکربندی دیتابیس
    /// </summary>
    void ConfigureDatabase(ModelBuilder modelBuilder);

    #endregion
}

/// <summary>
/// اطلاعات پلاگین
/// </summary>
public class PluginInfo
{
    public string PluginId { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string[] Dependencies { get; set; } = Array.Empty<string>();
    public bool IsEnabled { get; set; }
    public bool IsLoaded { get; set; }
    public string? ErrorMessage { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/IAriaPlugin.cs
// =============================================================================