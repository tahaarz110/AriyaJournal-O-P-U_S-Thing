// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/CacheEngine.cs
// توضیح: پیاده‌سازی موتور کش - نسخه اصلاح‌شده با RemoveByPattern
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور کش
/// </summary>
public class CacheEngine : ICacheEngine, IDisposable
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, DateTime> _keys;
    private readonly TimeSpan _defaultTtl;
    
    private long _hitCount;
    private long _missCount;
    private bool _disposed;

    public CacheEngine()
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 10000, // حداکثر 10000 آیتم
            ExpirationScanFrequency = TimeSpan.FromMinutes(5)
        });

        _keys = new ConcurrentDictionary<string, DateTime>();
        _defaultTtl = TimeSpan.FromMinutes(30);
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        if (_cache.TryGetValue(key, out T? value))
        {
            Interlocked.Increment(ref _hitCount);
            return value;
        }

        Interlocked.Increment(ref _missCount);
        return default;
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
            return;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? _defaultTtl,
            Size = 1,
            Priority = CacheItemPriority.Normal
        };

        // Callback برای حذف کلید از لیست
        options.RegisterPostEvictionCallback((k, v, reason, state) =>
        {
            _keys.TryRemove(k.ToString()!, out _);
        });

        _cache.Set(key, value, options);
        _keys.TryAdd(key, DateTime.Now);
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        _cache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    public void RemoveByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return;

        // تبدیل الگو به Regex
        // مثال: schema:* → ^schema:.*$
        var regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);

        var keysToRemove = _keys.Keys
            .Where(k => regex.IsMatch(k))
            .ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }
    }

    public void Clear()
    {
        foreach (var key in _keys.Keys.ToList())
        {
            Remove(key);
        }
    }

    public bool Exists(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return _cache.TryGetValue(key, out _);
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan? ttl = null)
    {
        var cached = Get<T>(key);
        if (cached != null)
            return cached;

        var value = factory();
        Set(key, value, ttl);
        return value;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null)
    {
        var cached = Get<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        Set(key, value, ttl);
        return value;
    }

    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            ItemCount = _keys.Count,
            HitCount = _hitCount,
            MissCount = _missCount,
            EstimatedSizeBytes = _keys.Count * 1024 // تخمین تقریبی
        };
    }

    public IEnumerable<string> GetAllKeys()
    {
        return _keys.Keys.ToList();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cache.Dispose();
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/CacheEngine.cs
// =============================================================================