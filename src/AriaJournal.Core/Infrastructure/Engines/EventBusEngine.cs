// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/EventBusEngine.cs
// شماره فایل: 51
// =============================================================================

using System.Collections.Concurrent;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور Event Bus
/// </summary>
public class EventBusEngine : IEventBusEngine
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _syncHandlers;
    private readonly ConcurrentDictionary<Type, List<Delegate>> _asyncHandlers;
    private readonly object _lock = new();

    public EventBusEngine()
    {
        _syncHandlers = new ConcurrentDictionary<Type, List<Delegate>>();
        _asyncHandlers = new ConcurrentDictionary<Type, List<Delegate>>();
    }

    public void Publish<T>(T eventData)
    {
        if (eventData == null) return;

        var eventType = typeof(T);

        // اجرای handler های sync
        if (_syncHandlers.TryGetValue(eventType, out var syncHandlers))
        {
            foreach (var handler in syncHandlers.ToList())
            {
                try
                {
                    ((Action<T>)handler)(eventData);
                }
                catch (Exception ex)
                {
                    // لاگ خطا
                    System.Diagnostics.Debug.WriteLine($"خطا در اجرای Event Handler: {ex.Message}");
                }
            }
        }

        // اجرای handler های async بدون انتظار
        if (_asyncHandlers.TryGetValue(eventType, out var asyncHandlers))
        {
            foreach (var handler in asyncHandlers.ToList())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ((Func<T, Task>)handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"خطا در اجرای Async Event Handler: {ex.Message}");
                    }
                });
            }
        }
    }

    public async Task PublishAsync<T>(T eventData)
    {
        if (eventData == null) return;

        var eventType = typeof(T);

        // اجرای handler های sync
        if (_syncHandlers.TryGetValue(eventType, out var syncHandlers))
        {
            foreach (var handler in syncHandlers.ToList())
            {
                try
                {
                    ((Action<T>)handler)(eventData);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"خطا در اجرای Event Handler: {ex.Message}");
                }
            }
        }

        // اجرای handler های async با انتظار
        if (_asyncHandlers.TryGetValue(eventType, out var asyncHandlers))
        {
            var tasks = asyncHandlers.Select(async handler =>
            {
                try
                {
                    await ((Func<T, Task>)handler)(eventData);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"خطا در اجرای Async Event Handler: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    public void Subscribe<T>(Action<T> handler)
    {
        if (handler == null) return;

        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_syncHandlers.ContainsKey(eventType))
            {
                _syncHandlers[eventType] = new List<Delegate>();
            }

            _syncHandlers[eventType].Add(handler);
        }
    }

    public void Subscribe<T>(Func<T, Task> handler)
    {
        if (handler == null) return;

        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_asyncHandlers.ContainsKey(eventType))
            {
                _asyncHandlers[eventType] = new List<Delegate>();
            }

            _asyncHandlers[eventType].Add(handler);
        }
    }

    public void Unsubscribe<T>(Action<T> handler)
    {
        if (handler == null) return;

        var eventType = typeof(T);

        lock (_lock)
        {
            if (_syncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public void Unsubscribe<T>(Func<T, Task> handler)
    {
        if (handler == null) return;

        var eventType = typeof(T);

        lock (_lock)
        {
            if (_asyncHandlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public void UnsubscribeAll<T>()
    {
        var eventType = typeof(T);

        lock (_lock)
        {
            _syncHandlers.TryRemove(eventType, out _);
            _asyncHandlers.TryRemove(eventType, out _);
        }
    }

    public void ClearAllSubscriptions()
    {
        lock (_lock)
        {
            _syncHandlers.Clear();
            _asyncHandlers.Clear();
        }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/EventBusEngine.cs
// =============================================================================