// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/PluginEngine.cs
// شماره فایل: 60
// توضیح: موتور مدیریت پلاگین‌ها
// =============================================================================

using System.Collections.Concurrent;
using System.Reflection;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;
using System.IO;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور مدیریت پلاگین‌ها
/// </summary>
public class PluginEngine : IPluginEngine
{
    private readonly ConcurrentDictionary<string, IAriaPlugin> _loadedPlugins;
    private readonly ConcurrentDictionary<string, PluginInfo> _pluginInfos;
    private readonly AriaDbContext _dbContext;
    private readonly IEventBusEngine _eventBus;
    private string _pluginsFolder = string.Empty;

    public event EventHandler<PluginLoadedEventArgs>? PluginLoaded;
    public event EventHandler<PluginErrorEventArgs>? PluginError;

    public PluginEngine(AriaDbContext dbContext, IEventBusEngine eventBus)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _loadedPlugins = new ConcurrentDictionary<string, IAriaPlugin>();
        _pluginInfos = new ConcurrentDictionary<string, PluginInfo>();
    }

    public void LoadPlugins(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        }

        _pluginsFolder = folder;

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            System.Diagnostics.Debug.WriteLine($"پوشه پلاگین‌ها ایجاد شد: {folder}");
            return;
        }

        var dllFiles = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);

        foreach (var dllPath in dllFiles)
        {
            try
            {
                LoadPluginFromDll(dllPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری {Path.GetFileName(dllPath)}: {ex.Message}");
                PluginError?.Invoke(this, new PluginErrorEventArgs
                {
                    PluginId = Path.GetFileNameWithoutExtension(dllPath),
                    ErrorMessage = ex.Message,
                    Exception = ex
                });
            }
        }

        System.Diagnostics.Debug.WriteLine($"تعداد {_loadedPlugins.Count} پلاگین بارگذاری شد");
    }

    private void LoadPluginFromDll(string dllPath)
    {
        // بارگذاری Assembly
        var assembly = Assembly.LoadFrom(dllPath);

        // یافتن کلاس‌هایی که IAriaPlugin را پیاده‌سازی می‌کنند
        var pluginTypes = assembly.GetTypes()
            .Where(t => typeof(IAriaPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        foreach (var pluginType in pluginTypes)
        {
            try
            {
                // ایجاد نمونه
                var plugin = (IAriaPlugin)Activator.CreateInstance(pluginType)!;

                // بررسی تکراری نبودن
                if (_loadedPlugins.ContainsKey(plugin.PluginId))
                {
                    System.Diagnostics.Debug.WriteLine($"پلاگین {plugin.PluginId} قبلاً بارگذاری شده است");
                    continue;
                }

                // ذخیره پلاگین
                _loadedPlugins[plugin.PluginId] = plugin;

                // ذخیره اطلاعات
                var info = new PluginInfo
                {
                    PluginId = plugin.PluginId,
                    PluginName = plugin.PluginName,
                    Version = plugin.Version,
                    Description = plugin.Description,
                    Author = plugin.Author,
                    Dependencies = plugin.Dependencies,
                    IsLoaded = true,
                    IsEnabled = GetPluginEnabledState(plugin.PluginId)
                };

                _pluginInfos[plugin.PluginId] = info;

                System.Diagnostics.Debug.WriteLine($"پلاگین {plugin.PluginName} v{plugin.Version} بارگذاری شد");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در ایجاد نمونه از {pluginType.Name}: {ex.Message}");
            }
        }
    }

    public async Task InitializePluginsAsync(IServiceProvider services)
    {
        // مرتب‌سازی بر اساس وابستگی‌ها
        var sortedPlugins = TopologicalSort(_loadedPlugins.Values.ToList());

        foreach (var plugin in sortedPlugins)
        {
            if (!IsEnabled(plugin.PluginId))
            {
                System.Diagnostics.Debug.WriteLine($"پلاگین {plugin.PluginName} غیرفعال است");
                continue;
            }

            // بررسی وابستگی‌ها
            if (!CheckDependencies(plugin))
            {
                var info = _pluginInfos[plugin.PluginId];
                info.IsEnabled = false;
                info.ErrorMessage = "وابستگی‌های مورد نیاز یافت نشد";
                continue;
            }

            try
            {
                await plugin.InitializeAsync(services);

                PluginLoaded?.Invoke(this, new PluginLoadedEventArgs
                {
                    PluginId = plugin.PluginId,
                    PluginName = plugin.PluginName
                });

                _eventBus.Publish(new PluginLoadedEvent(plugin.PluginId, plugin.PluginName));

                System.Diagnostics.Debug.WriteLine($"پلاگین {plugin.PluginName} راه‌اندازی شد");
            }
            catch (Exception ex)
            {
                var info = _pluginInfos[plugin.PluginId];
                info.ErrorMessage = ex.Message;

                PluginError?.Invoke(this, new PluginErrorEventArgs
                {
                    PluginId = plugin.PluginId,
                    ErrorMessage = ex.Message,
                    Exception = ex
                });

                System.Diagnostics.Debug.WriteLine($"خطا در راه‌اندازی {plugin.PluginName}: {ex.Message}");
            }
        }
    }

    public List<PluginInfo> GetPlugins()
    {
        return _pluginInfos.Values.ToList();
    }

    public IAriaPlugin? GetPlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return null;

        _loadedPlugins.TryGetValue(pluginId, out var plugin);
        return plugin;
    }

    public Result<bool> EnablePlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return Result.Failure<bool>(Error.Validation("شناسه پلاگین نامعتبر است"));

        if (!_pluginInfos.TryGetValue(pluginId, out var info))
            return Result.Failure<bool>(Error.PluginNotFound);

        info.IsEnabled = true;
        SavePluginState(pluginId, true);

        _eventBus.Publish(new PluginEnabledEvent(pluginId));

        return Result.Success(true);
    }

    public Result<bool> DisablePlugin(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return Result.Failure<bool>(Error.Validation("شناسه پلاگین نامعتبر است"));

        if (!_pluginInfos.TryGetValue(pluginId, out var info))
            return Result.Failure<bool>(Error.PluginNotFound);

        info.IsEnabled = false;
        SavePluginState(pluginId, false);

        _eventBus.Publish(new PluginDisabledEvent(pluginId));

        return Result.Success(true);
    }

    public bool IsEnabled(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return false;

        if (_pluginInfos.TryGetValue(pluginId, out var info))
        {
            return info.IsEnabled;
        }

        return GetPluginEnabledState(pluginId);
    }

    public bool IsLoaded(string pluginId)
    {
        if (string.IsNullOrWhiteSpace(pluginId))
            return false;

        return _loadedPlugins.ContainsKey(pluginId);
    }

    public async Task ShutdownAllAsync()
    {
        foreach (var plugin in _loadedPlugins.Values)
        {
            try
            {
                await plugin.ShutdownAsync();
                _eventBus.Publish(new PluginUnloadedEvent(plugin.PluginId));
                System.Diagnostics.Debug.WriteLine($"پلاگین {plugin.PluginName} خاموش شد");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در خاموش کردن {plugin.PluginName}: {ex.Message}");
            }
        }

        _loadedPlugins.Clear();
        _pluginInfos.Clear();
    }

    public async Task ReloadPluginsAsync(IServiceProvider services)
    {
        await ShutdownAllAsync();
        LoadPlugins(_pluginsFolder);
        await InitializePluginsAsync(services);
    }

    public IEnumerable<IAriaPlugin> GetEnabledPlugins()
    {
        return _loadedPlugins.Values.Where(p => IsEnabled(p.PluginId));
    }

    #region Private Methods

    private bool GetPluginEnabledState(string pluginId)
    {
        try
        {
            var state = _dbContext.PluginStates
                .FirstOrDefault(p => p.PluginId == pluginId);

            return state?.IsEnabled ?? true; // پیش‌فرض فعال
        }
        catch
        {
            return true;
        }
    }

    private void SavePluginState(string pluginId, bool enabled)
    {
        try
        {
            var state = _dbContext.PluginStates
                .FirstOrDefault(p => p.PluginId == pluginId);

            if (state == null)
            {
                var plugin = GetPlugin(pluginId);
                state = new PluginState
                {
                    PluginId = pluginId,
                    PluginName = plugin?.PluginName ?? pluginId,
                    Version = plugin?.Version ?? "1.0",
                    IsEnabled = enabled,
                    InstalledAt = DateTime.Now
                };
                _dbContext.PluginStates.Add(state);
            }
            else
            {
                state.IsEnabled = enabled;
                if (enabled)
                    state.LastEnabledAt = DateTime.Now;
                else
                    state.LastDisabledAt = DateTime.Now;
            }

            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ذخیره وضعیت پلاگین: {ex.Message}");
        }
    }

    private bool CheckDependencies(IAriaPlugin plugin)
    {
        if (plugin.Dependencies == null || plugin.Dependencies.Length == 0)
            return true;

        foreach (var dependency in plugin.Dependencies)
        {
            if (dependency.Equals("core", StringComparison.OrdinalIgnoreCase))
                continue; // Core همیشه موجود است

            if (!_loadedPlugins.ContainsKey(dependency))
            {
                System.Diagnostics.Debug.WriteLine($"وابستگی {dependency} برای {plugin.PluginId} یافت نشد");
                return false;
            }

            if (!IsEnabled(dependency))
            {
                System.Diagnostics.Debug.WriteLine($"وابستگی {dependency} برای {plugin.PluginId} غیرفعال است");
                return false;
            }
        }

        return true;
    }

    private List<IAriaPlugin> TopologicalSort(List<IAriaPlugin> plugins)
    {
        var sorted = new List<IAriaPlugin>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        void Visit(IAriaPlugin plugin)
        {
            if (visited.Contains(plugin.PluginId))
                return;

            if (visiting.Contains(plugin.PluginId))
            {
                System.Diagnostics.Debug.WriteLine($"وابستگی دایره‌ای در پلاگین {plugin.PluginId}");
                return;
            }

            visiting.Add(plugin.PluginId);

            foreach (var depId in plugin.Dependencies ?? Array.Empty<string>())
            {
                var dep = plugins.FirstOrDefault(p => p.PluginId == depId);
                if (dep != null)
                {
                    Visit(dep);
                }
            }

            visiting.Remove(plugin.PluginId);
            visited.Add(plugin.PluginId);
            sorted.Add(plugin);
        }

        foreach (var plugin in plugins)
        {
            Visit(plugin);
        }

        return sorted;
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/PluginEngine.cs
// =============================================================================