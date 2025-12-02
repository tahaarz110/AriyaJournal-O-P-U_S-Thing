// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/StateEngine.cs
// شماره فایل: 52
// =============================================================================

using System.Collections.Concurrent;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور State مرکزی
/// </summary>
public class StateEngine : IStateEngine
{
    private readonly ConcurrentDictionary<string, object?> _state;
    private readonly ConcurrentDictionary<string, List<Delegate>> _subscribers;
    private readonly object _lock = new();

    public StateEngine()
    {
        _state = new ConcurrentDictionary<string, object?>();
        _subscribers = new ConcurrentDictionary<string, List<Delegate>>();
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        if (_state.TryGetValue(key, out var value))
        {
            if (value is T typed)
                return typed;
        }

        return default;
    }

    public void Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var oldValue = Get<T>(key);
        _state.AddOrUpdate(key, value, (_, _) => value);

        // اطلاع‌رسانی به Subscriber ها
        NotifySubscribers(key, oldValue, value);
    }

    public bool Has(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return _state.ContainsKey(key);
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (_state.TryRemove(key, out var oldValue))
        {
            NotifySubscribers(key, oldValue, default(object));
        }
    }

    public void Clear()
    {
        _state.Clear();
    }

    public IDisposable Subscribe<T>(string key, Action<T?> callback)
    {
        if (string.IsNullOrWhiteSpace(key) || callback == null)
            return new EmptyDisposable();

        lock (_lock)
        {
            if (!_subscribers.ContainsKey(key))
            {
                _subscribers[key] = new List<Delegate>();
            }

            // Wrapper که فقط مقدار جدید را می‌فرستد
            Action<object?, object?> wrapper = (oldVal, newVal) =>
            {
                if (newVal is T typed)
                    callback(typed);
                else
                    callback(default);
            };

            _subscribers[key].Add(wrapper);

            return new SubscriptionDisposable(() =>
            {
                lock (_lock)
                {
                    if (_subscribers.TryGetValue(key, out var list))
                    {
                        list.Remove(wrapper);
                    }
                }
            });
        }
    }

    public IDisposable Subscribe<T>(string key, Action<T?, T?> callback)
    {
        if (string.IsNullOrWhiteSpace(key) || callback == null)
            return new EmptyDisposable();

        lock (_lock)
        {
            if (!_subscribers.ContainsKey(key))
            {
                _subscribers[key] = new List<Delegate>();
            }

            // Wrapper که هم مقدار قبلی و هم جدید را می‌فرستد
            Action<object?, object?> wrapper = (oldVal, newVal) =>
            {
                T? oldTyped = oldVal is T o ? o : default;
                T? newTyped = newVal is T n ? n : default;
                callback(oldTyped, newTyped);
            };

            _subscribers[key].Add(wrapper);

            return new SubscriptionDisposable(() =>
            {
                lock (_lock)
                {
                    if (_subscribers.TryGetValue(key, out var list))
                    {
                        list.Remove(wrapper);
                    }
                }
            });
        }
    }

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        var value = Get<T>(key);
        return value ?? defaultValue;
    }

    public IEnumerable<string> GetAllKeys()
    {
        return _state.Keys.ToList();
    }

    private void NotifySubscribers(string key, object? oldValue, object? newValue)
    {
        if (!_subscribers.TryGetValue(key, out var subscribers))
            return;

        foreach (var subscriber in subscribers.ToList())
        {
            try
            {
                if (subscriber is Action<object?, object?> handler)
                {
                    handler(oldValue, newValue);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در State Subscriber: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// کلاس کمکی برای لغو اشتراک
    /// </summary>
    private class SubscriptionDisposable : IDisposable
    {
        private readonly Action _disposeAction;
        private bool _disposed;

        public SubscriptionDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposeAction?.Invoke();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// کلاس Disposable خالی
    /// </summary>
    private class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/StateEngine.cs
// =============================================================================