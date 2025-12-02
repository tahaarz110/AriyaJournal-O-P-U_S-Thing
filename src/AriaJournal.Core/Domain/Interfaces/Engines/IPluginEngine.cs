// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IPluginEngine.cs
// شماره فایل: 33
// =============================================================================

using AriaJournal.Core.Domain.Common;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور مدیریت پلاگین‌ها
/// </summary>
public interface IPluginEngine
{
    /// <summary>
    /// بارگذاری پلاگین‌ها از پوشه
    /// </summary>
    void LoadPlugins(string folder);

    /// <summary>
    /// راه‌اندازی پلاگین‌ها
    /// </summary>
    Task InitializePluginsAsync(IServiceProvider services);

    /// <summary>
    /// دریافت لیست پلاگین‌ها
    /// </summary>
    List<PluginInfo> GetPlugins();

    /// <summary>
    /// دریافت پلاگین با شناسه
    /// </summary>
    IAriaPlugin? GetPlugin(string pluginId);

    /// <summary>
    /// فعال کردن پلاگین
    /// </summary>
    Result<bool> EnablePlugin(string pluginId);

    /// <summary>
    /// غیرفعال کردن پلاگین
    /// </summary>
    Result<bool> DisablePlugin(string pluginId);

    /// <summary>
    /// بررسی فعال بودن پلاگین
    /// </summary>
    bool IsEnabled(string pluginId);

    /// <summary>
    /// بررسی بارگذاری پلاگین
    /// </summary>
    bool IsLoaded(string pluginId);

    /// <summary>
    /// خاموش کردن همه پلاگین‌ها
    /// </summary>
    Task ShutdownAllAsync();

    /// <summary>
    /// بارگذاری مجدد پلاگین‌ها
    /// </summary>
    Task ReloadPluginsAsync(IServiceProvider services);

    /// <summary>
    /// دریافت پلاگین‌های فعال
    /// </summary>
    IEnumerable<IAriaPlugin> GetEnabledPlugins();

    /// <summary>
    /// Event بارگذاری پلاگین
    /// </summary>
    event EventHandler<PluginLoadedEventArgs>? PluginLoaded;

    /// <summary>
    /// Event خطای پلاگین
    /// </summary>
    event EventHandler<PluginErrorEventArgs>? PluginError;
}

/// <summary>
/// آرگومان‌های Event بارگذاری پلاگین
/// </summary>
public class PluginLoadedEventArgs : EventArgs
{
    public string PluginId { get; set; } = string.Empty;
    public string PluginName { get; set; } = string.Empty;
}

/// <summary>
/// آرگومان‌های Event خطای پلاگین
/// </summary>
public class PluginErrorEventArgs : EventArgs
{
    public string PluginId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IPluginEngine.cs
// =============================================================================