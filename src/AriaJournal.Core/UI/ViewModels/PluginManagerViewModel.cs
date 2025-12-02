// ═══════════════════════════════════════════════════════════════════════
// فایل: PluginManagerViewModel.cs
// مسیر: src/AriaJournal.Core/UI/ViewModels/PluginManagerViewModel.cs
// توضیح: ViewModel مدیریت پلاگین‌ها
// ═══════════════════════════════════════════════════════════════════════

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Events;
using Microsoft.Win32;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// مدل نمایش پلاگین
/// </summary>
public class PluginDisplayModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Icon { get; set; } = "🔌";
    public bool IsEnabled { get; set; }
    public bool HasSettings { get; set; }
}

/// <summary>
/// ViewModel مدیریت پلاگین‌ها
/// </summary>
public partial class PluginManagerViewModel : BaseViewModel
{
    private readonly IPluginEngine _pluginEngine;
    private readonly IEventBusEngine _eventBus;

    private ObservableCollection<PluginDisplayModel> _plugins;
    private PluginDisplayModel? _selectedPlugin;
    private string _searchText = string.Empty;
    private string _pluginFolder;

    public PluginManagerViewModel(
        IPluginEngine pluginEngine,
        IEventBusEngine eventBus)
    {
        _pluginEngine = pluginEngine ?? throw new ArgumentNullException(nameof(pluginEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _plugins = new ObservableCollection<PluginDisplayModel>();
        
        // مسیر پوشه پلاگین‌ها
        _pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");

        Title = "مدیریت پلاگین‌ها";

        // اطمینان از وجود پوشه
        if (!Directory.Exists(_pluginFolder))
        {
            Directory.CreateDirectory(_pluginFolder);
        }
    }

    #region Properties

    public ObservableCollection<PluginDisplayModel> Plugins
    {
        get => _plugins;
        set => SetProperty(ref _plugins, value);
    }

    public PluginDisplayModel? SelectedPlugin
    {
        get => _selectedPlugin;
        set => SetProperty(ref _selectedPlugin, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterPlugins();
            }
        }
    }

    public string PluginFolder
    {
        get => _pluginFolder;
        set => SetProperty(ref _pluginFolder, value);
    }

    public bool HasPlugins => Plugins.Count > 0;
    public bool IsEmpty => !IsBusy && Plugins.Count == 0;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPluginsAsync();
    }

    [RelayCommand]
    private async Task InstallPluginAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "انتخاب فایل پلاگین",
            Filter = "فایل‌های DLL (*.dll)|*.dll|فایل‌های ZIP (*.zip)|*.zip",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            try
            {
                var fileName = Path.GetFileName(dialog.FileName);
                var destPath = Path.Combine(_pluginFolder, fileName);

                // کپی فایل
                File.Copy(dialog.FileName, destPath, true);

                // بارگذاری مجدد پلاگین‌ها
                _pluginEngine.LoadPlugins(_pluginFolder);

                await LoadPluginsAsync();
                ShowSuccess($"پلاگین {fileName} با موفقیت نصب شد");

                _eventBus.Publish(new PluginLoadedEvent(fileName, fileName));
            }
            catch (Exception ex)
            {
                ShowError($"خطا در نصب پلاگین: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void TogglePlugin(PluginDisplayModel? plugin)
    {
        if (plugin == null) return;

        try
        {
            if (plugin.IsEnabled)
            {
                _pluginEngine.DisablePlugin(plugin.Id);
                plugin.IsEnabled = false;
                _eventBus.Publish(new PluginDisabledEvent(plugin.Id));
            }
            else
            {
                _pluginEngine.EnablePlugin(plugin.Id);
                plugin.IsEnabled = true;
                _eventBus.Publish(new PluginEnabledEvent(plugin.Id));
            }

            ShowSuccess($"وضعیت پلاگین {plugin.Name} تغییر کرد");
        }
        catch (Exception ex)
        {
            ShowError($"خطا: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenPluginSettings(PluginDisplayModel? plugin)
    {
        if (plugin == null) return;

        // TODO: باز کردن تنظیمات پلاگین
        MessageBox.Show($"تنظیمات پلاگین {plugin.Name} در نسخه‌های آینده", "اطلاع", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task UninstallPluginAsync(PluginDisplayModel? plugin)
    {
        if (plugin == null) return;

        var result = MessageBox.Show(
            $"آیا از حذف پلاگین «{plugin.Name}» مطمئن هستید؟",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            IsBusy = true;
            try
            {
                // غیرفعال کردن اول
                if (plugin.IsEnabled)
                {
                    _pluginEngine.DisablePlugin(plugin.Id);
                }

                // حذف فایل
                var pluginPath = Path.Combine(_pluginFolder, $"{plugin.Id}.dll");
                if (File.Exists(pluginPath))
                {
                    File.Delete(pluginPath);
                }

                _eventBus.Publish(new PluginUnloadedEvent(plugin.Id));

                await LoadPluginsAsync();
                ShowSuccess($"پلاگین {plugin.Name} حذف شد");
            }
            catch (Exception ex)
            {
                ShowError($"خطا در حذف پلاگین: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void OpenPluginFolder()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _pluginFolder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError($"خطا در باز کردن پوشه: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadPluginsAsync()
    {
        IsBusy = true;

        try
        {
            await Task.Run(() =>
            {
                var pluginInfos = _pluginEngine.GetPlugins();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Plugins.Clear();

                    foreach (var info in pluginInfos)
                    {
                        Plugins.Add(new PluginDisplayModel
                        {
                            Id = info.PluginId,
                            Name = info.Name,
                            Description = info.Description ?? string.Empty,
                            Version = info.Version,
                            Author = info.Author ?? "نامشخص",
                            Icon = info.Icon ?? "🔌",
                            IsEnabled = info.IsEnabled,
                            HasSettings = info.HasSettings
                        });
                    }

                    OnPropertyChanged(nameof(HasPlugins));
                    OnPropertyChanged(nameof(IsEmpty));
                });
            });
        }
        catch (Exception ex)
        {
            ShowError($"خطا در بارگذاری پلاگین‌ها: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterPlugins()
    {
        // TODO: پیاده‌سازی فیلتر
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadPluginsAsync();
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: PluginManagerViewModel.cs
// ═══════════════════════════════════════════════════════════════════════