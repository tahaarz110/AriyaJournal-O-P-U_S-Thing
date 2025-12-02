// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IEventBusEngine.cs
// شماره فایل: 30
// =============================================================================

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور Event Bus
/// </summary>
public interface IEventBusEngine
{
    /// <summary>
    /// انتشار Event
    /// </summary>
    void Publish<T>(T eventData);

    /// <summary>
    /// انتشار Event به صورت async
    /// </summary>
    Task PublishAsync<T>(T eventData);

    /// <summary>
    /// اشتراک در Event
    /// </summary>
    void Subscribe<T>(Action<T> handler);

    /// <summary>
    /// اشتراک async در Event
    /// </summary>
    void Subscribe<T>(Func<T, Task> handler);

    /// <summary>
    /// لغو اشتراک
    /// </summary>
    void Unsubscribe<T>(Action<T> handler);

    /// <summary>
    /// لغو اشتراک async
    /// </summary>
    void Unsubscribe<T>(Func<T, Task> handler);

    /// <summary>
    /// لغو همه اشتراک‌های یک نوع
    /// </summary>
    void UnsubscribeAll<T>();

    /// <summary>
    /// لغو همه اشتراک‌ها
    /// </summary>
    void ClearAllSubscriptions();
}

// Event های استاندارد Core
public record UserLoggedInEvent(int UserId, string Username);
public record UserLoggedOutEvent(int UserId);
public record AccountChangedEvent(int AccountId, string Action);
public record AccountSelectedEvent(int AccountId);
public record TradeCreatedEvent(int TradeId, int AccountId);
public record TradeUpdatedEvent(int TradeId);
public record TradeDeletedEvent(int TradeId);
public record PluginLoadedEvent(string PluginId, string PluginName);
public record PluginUnloadedEvent(string PluginId);
public record PluginEnabledEvent(string PluginId);
public record PluginDisabledEvent(string PluginId);
public record SchemaReloadedEvent(string Module);
public record BackupCreatedEvent(string Path, DateTime CreatedAt);
public record BackupRestoredEvent(string Path);
public record ThemeChangedEvent(string Theme);
public record LanguageChangedEvent(string Language);
public record NavigationEvent(string FromView, string ToView);

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IEventBusEngine.cs
// =============================================================================