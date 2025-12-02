// =============================================================================
// فایل: src/AriaJournal.Core/UI/App.xaml.cs
// توضیح: کد پشت فایل App.xaml - نقطه شروع برنامه
// نسخه کامل با پشتیبانی از Meta-driven + GUI-driven
// =============================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;
using AriaJournal.Core.Infrastructure.Engines;
using AriaJournal.Core.Infrastructure.Repositories;
using AriaJournal.Core.UI.ViewModels;
using AriaJournal.Core.UI.Views;

namespace AriaJournal.Core.UI;

/// <summary>
/// رویداد تغییر تم
/// </summary>
public record ThemeChangedEvent(string ThemeName);

/// <summary>
/// کلاس اصلی برنامه
/// </summary>
public partial class App : System.Windows.Application
{
    private IServiceProvider _serviceProvider = null!;
    private IServiceCollection _services = null!;

    /// <summary>
    /// دسترسی به ServiceProvider
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// پنجره اصلی برنامه
    /// </summary>
    public static Window? MainAppWindow { get; private set; }

    /// <summary>
    /// تم فعلی
    /// </summary>
    public static string CurrentTheme { get; private set; } = "Dark";

    /// <summary>
    /// لیست تم‌های موجود
    /// </summary>
    public static readonly string[] AvailableThemes = { "Dark", "Light", "Blue", "Green" };

    /// <summary>
    /// نقطه ورود برنامه
    /// </summary>
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // اضافه کردن Global Exception Handler
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"خطای غیرمنتظره:\n{ex?.Message}\n\n{ex?.InnerException?.Message}",
                "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show($"خطای UI:\n{args.Exception.Message}\n\n{args.Exception.InnerException?.Message}",
                "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        base.OnStartup(e);

        try
        {
            // پیکربندی سرویس‌ها
            _services = new ServiceCollection();
            ConfigureServices(_services);
            _serviceProvider = _services.BuildServiceProvider();
            Services = _serviceProvider;

            // راه‌اندازی موتورها
            await InitializeEnginesAsync();

            // بارگذاری تم ذخیره‌شده کاربر
            await LoadSavedThemeAsync();

            // نمایش پنجره ورود
            await ShowLoginWindowAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در راه‌اندازی برنامه:\n{ex.Message}",
                "خطا",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            // خاموش کردن پلاگین‌ها
            var pluginEngine = _serviceProvider.GetService<IPluginEngine>();
            if (pluginEngine != null)
            {
                await pluginEngine.ShutdownAllAsync();
            }

            // Dispose کردن DbContext
            var dbContext = _serviceProvider.GetService<AriaDbContext>();
            dbContext?.Dispose();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در خروج: {ex.Message}");
        }

        base.OnExit(e);
    }

    /// <summary>
    /// پیکربندی سرویس‌ها با DI
    /// </summary>
    private void ConfigureServices(IServiceCollection services)
    {
        // DbContext
        services.AddDbContext<AriaDbContext>(options =>
        {
            var dbPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AriaJournal",
                "aria.db");

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

            options.UseSqlite($"Data Source={dbPath}");
        }, ServiceLifetime.Scoped);

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Engines - Singleton
        services.AddSingleton<ICacheEngine, CacheEngine>();
        services.AddSingleton<IEventBusEngine, EventBusEngine>();
        services.AddSingleton<IStateEngine, StateEngine>();

        services.AddSingleton<ISchemaEngine>(sp =>
        {
            var cache = sp.GetRequiredService<ICacheEngine>();
            return new SchemaEngine(cache);
        });

        services.AddSingleton<INavigationEngine>(sp => new NavigationEngine(sp));

        // Engines - Scoped
        services.AddScoped<IDataEngine, DataEngine>();

        services.AddScoped<IAuthEngine>(sp =>
        {
            var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
            var eventBus = sp.GetRequiredService<IEventBusEngine>();
            var state = sp.GetRequiredService<IStateEngine>();
            var cache = sp.GetRequiredService<ICacheEngine>();
            return new AuthEngine(unitOfWork, eventBus, state, cache);
        });

        services.AddScoped<IMigrationEngine>(sp =>
        {
            var context = sp.GetRequiredService<AriaDbContext>();
            return new MigrationEngine(context);
        });

        services.AddScoped<IBackupEngine>(sp =>
        {
            var context = sp.GetRequiredService<AriaDbContext>();
            var eventBus = sp.GetRequiredService<IEventBusEngine>();
            return new BackupEngine(context, eventBus);
        });

        services.AddScoped<IPluginEngine>(sp =>
        {
            var context = sp.GetRequiredService<AriaDbContext>();
            var eventBus = sp.GetRequiredService<IEventBusEngine>();
            return new PluginEngine(context, eventBus);
        });

        // UIRenderer و RuleEngine (نیاز به یکدیگر دارند)
        services.AddSingleton<IRuleEngine>(sp =>
        {
            var schema = sp.GetRequiredService<ISchemaEngine>();
            return new RuleEngine(schema);
        });

        services.AddSingleton<IUIRendererEngine>(sp =>
        {
            var schema = sp.GetRequiredService<ISchemaEngine>();
            var rule = sp.GetRequiredService<IRuleEngine>();
            var renderer = new UIRendererEngine(schema, rule);

            // تنظیم UIRenderer در RuleEngine
            if (rule is RuleEngine ruleEngine)
            {
                ruleEngine.SetUIRenderer(renderer);
            }

            return renderer;
        });

        // =====================================================
        // Application Services
        // =====================================================
        services.AddScoped<AuthService>();
        services.AddScoped<AccountService>();
        services.AddScoped<TradeService>();
        services.AddScoped<FieldDefinitionService>();
        
        // سرویس متادیتا برای Meta-driven + GUI-driven
        services.AddScoped<IMetadataService, MetadataService>();

        // =====================================================
        // ViewModels
        // =====================================================
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ShellViewModel>();
        services.AddTransient<TradeListViewModel>();
        services.AddTransient<TradeEntryViewModel>();
        services.AddTransient<AccountListViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<BackupViewModel>();
        
        // ViewModels برای Meta-driven + GUI-driven
        services.AddTransient<FieldEditorViewModel>();
        services.AddTransient<ColumnEditorViewModel>();

        // =====================================================
        // Views
        // =====================================================
        services.AddTransient<LoginView>();
        services.AddTransient<ShellView>();
        services.AddTransient<TradeListView>();
        services.AddTransient<TradeEntryView>();
        services.AddTransient<AccountListView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<BackupView>();
        
        // Views برای Meta-driven + GUI-driven
        services.AddTransient<FieldEditorView>();
        services.AddTransient<ColumnEditorView>();
    }

    /// <summary>
    /// راه‌اندازی موتورها
    /// </summary>
    private async Task InitializeEnginesAsync()
    {
        // Schema Engine
        var schemaEngine = _serviceProvider.GetRequiredService<ISchemaEngine>();
        await schemaEngine.InitializeAsync();

        // Migration Engine
        using var scope = _serviceProvider.CreateScope();
        var migrationEngine = scope.ServiceProvider.GetRequiredService<IMigrationEngine>();
        var migrationResult = await migrationEngine.MigrateAsync();

        if (migrationResult.IsFailure)
        {
            throw new Exception($"خطا در مایگریشن دیتابیس: {migrationResult.Error.Message}");
        }

        // Plugin Engine
        var pluginEngine = scope.ServiceProvider.GetRequiredService<IPluginEngine>();
        pluginEngine.LoadPlugins("plugins");
        await pluginEngine.InitializePluginsAsync(_serviceProvider);

        // ثبت View ها در Navigation Engine
        RegisterViews();
    }

    /// <summary>
    /// ثبت View ها برای ناوبری
    /// </summary>
    private void RegisterViews()
    {
        var navEngine = _serviceProvider.GetRequiredService<INavigationEngine>();

        // View های اصلی
        navEngine.RegisterView("Login", typeof(LoginView), typeof(LoginViewModel));
        navEngine.RegisterView("Shell", typeof(ShellView), typeof(ShellViewModel));
        navEngine.RegisterView("Trades", typeof(TradeListView), typeof(TradeListViewModel));
        navEngine.RegisterView("TradeEntry", typeof(TradeEntryView), typeof(TradeEntryViewModel));
        navEngine.RegisterView("Accounts", typeof(AccountListView), typeof(AccountListViewModel));
        navEngine.RegisterView("Settings", typeof(SettingsView), typeof(SettingsViewModel));
        navEngine.RegisterView("Backup", typeof(BackupView), typeof(BackupViewModel));
        
        // View های Meta-driven + GUI-driven
        navEngine.RegisterView("FieldEditor", typeof(FieldEditorView), typeof(FieldEditorViewModel));
        navEngine.RegisterView("ColumnEditor", typeof(ColumnEditorView), typeof(ColumnEditorViewModel));

        // ثبت View های پلاگین‌ها
        var pluginEngine = _serviceProvider.GetRequiredService<IPluginEngine>();
        foreach (var plugin in pluginEngine.GetEnabledPlugins())
        {
            if (plugin.MainViewType != null)
            {
                navEngine.RegisterView(plugin.PluginId, plugin.MainViewType);
            }
        }
    }

    /// <summary>
    /// نمایش پنجره ورود
    /// </summary>
    private async Task ShowLoginWindowAsync()
    {
        try
        {
            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();

            loginView.DataContext = loginViewModel;
            await loginViewModel.InitializeAsync();

            MainAppWindow = loginView;
            loginView.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در نمایش پنجره ورود:\n{ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack: {ex.StackTrace}",
                "خطا",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw;
        }
    }

    /// <summary>
    /// بارگذاری تم ذخیره‌شده از دیتابیس
    /// </summary>
    private async Task LoadSavedThemeAsync()
    {
        try
        {
            // تم پیش‌فرض را استفاده کن تا کاربر لاگین کند
            // بعد از لاگین، تم کاربر بارگذاری می‌شود
            CurrentTheme = "Dark";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری تم: {ex.Message}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// تغییر تم برنامه - پشتیبانی از ۴ تم
    /// </summary>
    public static void ChangeTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
            themeName = "Dark";

        // نرمال‌سازی نام تم
        themeName = NormalizeThemeName(themeName);

        // اگر تم همان تم فعلی است، تغییری ندهد
        if (themeName == CurrentTheme)
            return;

        var themeUri = GetThemeUri(themeName);

        var currentApp = Current as App;
        if (currentApp == null)
            return;

        try
        {
            // لیست ResourceDictionary های موجود
            var mergedDicts = currentApp.Resources.MergedDictionaries;

            // پیدا کردن و حذف تم قبلی
            var existingThemes = mergedDicts
                .Where(d => d.Source != null && d.Source.ToString().Contains("Theme.xaml"))
                .ToList();

            foreach (var oldTheme in existingThemes)
            {
                mergedDicts.Remove(oldTheme);
            }

            // افزودن تم جدید در ابتدای لیست (قبل از Styles)
            var newTheme = new ResourceDictionary { Source = themeUri };
            mergedDicts.Insert(0, newTheme);

            // ذخیره تم فعلی
            CurrentTheme = themeName;

            System.Diagnostics.Debug.WriteLine($"✅ تم تغییر کرد به: {themeName}");

            // اعلام تغییر تم به همه پنجره‌ها
            RefreshAllWindows();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ خطا در تغییر تم: {ex.Message}");

            // در صورت خطا، برگشت به تم پیش‌فرض
            try
            {
                var defaultTheme = new ResourceDictionary
                {
                    Source = new Uri("Resources/DarkTheme.xaml", UriKind.Relative)
                };

                // حذف همه تم‌ها
                var allThemes = currentApp.Resources.MergedDictionaries
                    .Where(d => d.Source != null && d.Source.ToString().Contains("Theme.xaml"))
                    .ToList();

                foreach (var t in allThemes)
                {
                    currentApp.Resources.MergedDictionaries.Remove(t);
                }

                currentApp.Resources.MergedDictionaries.Insert(0, defaultTheme);
                CurrentTheme = "Dark";
            }
            catch
            {
                // نادیده گرفتن خطا
            }
        }
    }

    /// <summary>
    /// نرمال‌سازی نام تم
    /// </summary>
    private static string NormalizeThemeName(string themeName)
    {
        return themeName.ToLower() switch
        {
            "light" or "روشن" or "1" => "Light",
            "blue" or "آبی" or "2" => "Blue",
            "green" or "سبز" or "3" => "Green",
            "dark" or "تیره" or "0" or _ => "Dark"
        };
    }

    /// <summary>
    /// دریافت Uri فایل تم
    /// </summary>
    private static Uri GetThemeUri(string themeName)
    {
        return themeName switch
        {
            "Light" => new Uri("Resources/LightTheme.xaml", UriKind.Relative),
            "Blue" => new Uri("Resources/BlueTheme.xaml", UriKind.Relative),
            "Green" => new Uri("Resources/GreenTheme.xaml", UriKind.Relative),
            _ => new Uri("Resources/DarkTheme.xaml", UriKind.Relative)
        };
    }

    /// <summary>
    /// رفرش کردن همه پنجره‌ها بعد از تغییر تم
    /// </summary>
    private static void RefreshAllWindows()
    {
        foreach (Window window in Current.Windows)
        {
            // Force update of bindings
            var content = window.Content;
            window.Content = null;
            window.Content = content;
        }
    }

    /// <summary>
    /// دریافت ایندکس تم فعلی
    /// </summary>
    public static int GetThemeIndex()
    {
        return CurrentTheme switch
        {
            "Dark" => 0,
            "Light" => 1,
            "Blue" => 2,
            "Green" => 3,
            _ => 0
        };
    }

    /// <summary>
    /// تغییر تم با ایندکس
    /// </summary>
    public static void ChangeThemeByIndex(int index)
    {
        var themeName = GetThemeNameByIndex(index);
        ChangeTheme(themeName);
    }

    /// <summary>
    /// دریافت نام تم از ایندکس
    /// </summary>
    public static string GetThemeNameByIndex(int index)
    {
        return index switch
        {
            0 => "Dark",
            1 => "Light",
            2 => "Blue",
            3 => "Green",
            _ => "Dark"
        };
    }

    /// <summary>
    /// دریافت سرویس
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// ایجاد Scope جدید
    /// </summary>
    public static IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/App.xaml.cs
// =============================================================================