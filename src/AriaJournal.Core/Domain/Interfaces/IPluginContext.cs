// ═══════════════════════════════════════════════════════════════════════
// فایل: IPluginContext.cs
// مسیر: src/AriaJournal.Core/Domain/Interfaces/IPluginContext.cs
// توضیح: سرویس‌های در دسترس پلاگین‌ها
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// اینترفیس Context برای پلاگین‌ها
/// هر پلاگین از طریق این اینترفیس به سرویس‌های Core دسترسی دارد
/// </summary>
public interface IPluginContext
{
    /// <summary>
    /// شناسه کاربر فعلی
    /// </summary>
    int CurrentUserId { get; }

    /// <summary>
    /// شناسه حساب فعلی
    /// </summary>
    int CurrentAccountId { get; }

    /// <summary>
    /// دسترسی به Schema Engine
    /// </summary>
    ISchemaEngine SchemaEngine { get; }

    /// <summary>
    /// دسترسی به UI Renderer
    /// </summary>
    IUIRendererEngine UIRenderer { get; }

    /// <summary>
    /// دسترسی به Data Engine
    /// </summary>
    IDataEngine DataEngine { get; }

    /// <summary>
    /// دسترسی به Event Bus
    /// </summary>
    IEventBusEngine EventBus { get; }

    /// <summary>
    /// دسترسی به State Engine
    /// </summary>
    IStateEngine StateEngine { get; }

    /// <summary>
    /// دسترسی به Cache Engine
    /// </summary>
    ICacheEngine CacheEngine { get; }

    /// <summary>
    /// دسترسی به Navigation Engine
    /// </summary>
    INavigationEngine NavigationEngine { get; }

    /// <summary>
    /// ثبت Schema جدید
    /// </summary>
    Task<bool> RegisterSchemaAsync(SchemaDefinition schema);

    /// <summary>
    /// ثبت View در Navigation
    /// </summary>
    void RegisterView(string name, Type viewType, Type viewModelType);

    /// <summary>
    /// دریافت سرویس از DI
    /// </summary>
    T GetService<T>() where T : class;

    /// <summary>
    /// دریافت سرویس از DI (nullable)
    /// </summary>
    T? GetServiceOrDefault<T>() where T : class;

    /// <summary>
    /// ثبت لاگ
    /// </summary>
    void Log(string message, LogLevel level = LogLevel.Info);

    /// <summary>
    /// نمایش پیام به کاربر
    /// </summary>
    Task ShowMessageAsync(string title, string message, MessageType type = MessageType.Info);

    /// <summary>
    /// نمایش پنجره تأیید
    /// </summary>
    Task<bool> ShowConfirmAsync(string title, string message);

    /// <summary>
    /// دریافت مسیر پلاگین
    /// </summary>
    string GetPluginPath(string pluginId);

    /// <summary>
    /// ذخیره تنظیمات پلاگین
    /// </summary>
    Task SavePluginSettingsAsync(string pluginId, Dictionary<string, object> settings);

    /// <summary>
    /// خواندن تنظیمات پلاگین
    /// </summary>
    Task<Dictionary<string, object>> LoadPluginSettingsAsync(string pluginId);
}

/// <summary>
/// سطح لاگ
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// نوع پیام
/// </summary>
public enum MessageType
{
    Info,
    Success,
    Warning,
    Error
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: IPluginContext.cs
// ═══════════════════════════════════════════════════════════════════════