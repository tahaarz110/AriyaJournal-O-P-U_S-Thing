// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/ServiceCollectionExtensions.cs
// توضیح: Extension Methods برای ثبت سرویس‌ها و موتورها در DI Container
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;
using AriaJournal.Core.Infrastructure.Engines;
using AriaJournal.Core.Infrastructure.Repositories;
using AriaJournal.Core.Infrastructure.Security;
using AriaJournal.Core.Application.Services;

namespace AriaJournal.Core.Infrastructure;

/// <summary>
/// Extension Methods برای ثبت سرویس‌ها
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ثبت همه سرویس‌های Core
    /// </summary>
    public static IServiceCollection AddAriaCoreServices(this IServiceCollection services)
    {
        // DbContext
        services.AddDbContext<AriaDbContext>();

        // Engines - Singleton (یکبار ساخته می‌شوند)
        services.AddSingleton<ICacheEngine, CacheEngine>();
        services.AddSingleton<IEventBusEngine, EventBusEngine>();
        services.AddSingleton<IStateEngine, StateEngine>();
        services.AddSingleton<INavigationEngine, NavigationEngine>();
        services.AddSingleton<IPluginEngine, PluginEngine>();
        
        // Schema & UI Engines
        services.AddSingleton<ISchemaEngine, SchemaEngine>();
        services.AddSingleton<IUIRendererEngine, UIRendererEngine>();
        services.AddSingleton<IRuleEngine, RuleEngine>();
        
        // Query & Aggregation Engines
        services.AddSingleton<IQueryEngine, QueryEngine>();
        services.AddSingleton<IAggregationEngine, AggregationEngine>();
        services.AddSingleton<IThemeEngine, ThemeEngine>();

        // Engines - Scoped (یکی برای هر درخواست)
        services.AddScoped<IDataEngine, DataEngine>();
        services.AddScoped<IAuthEngine, AuthEngine>();
        services.AddScoped<IMigrationEngine, MigrationEngine>();
        services.AddScoped<IBackupEngine, BackupEngine>();

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Security
        services.AddSingleton<PasswordHasher>();
        services.AddSingleton<AesEncryption>();
        services.AddSingleton<RecoveryKeyGenerator>();

        // Application Services
        services.AddScoped<AuthService>();
        services.AddScoped<AccountService>();
        services.AddScoped<TradeService>();
        services.AddScoped<FieldDefinitionService>();
        services.AddScoped<ScreenshotService>();
        services.AddScoped<ImportService>();
        services.AddScoped<MetadataService>();
        services.AddScoped<FolderWatcherService>();

        return services;
    }

    /// <summary>
    /// ثبت ViewModels
    /// </summary>
    public static IServiceCollection AddAriaViewModels(this IServiceCollection services)
    {
        // ViewModels - Transient (هربار جدید ساخته می‌شوند)
        services.AddTransient<UI.ViewModels.LoginViewModel>();
        services.AddTransient<UI.ViewModels.RegisterViewModel>();
        services.AddTransient<UI.ViewModels.ShellViewModel>();
        services.AddTransient<UI.ViewModels.AccountListViewModel>();
        services.AddTransient<UI.ViewModels.TradeListViewModel>();
        services.AddTransient<UI.ViewModels.TradeEntryViewModel>();
        services.AddTransient<UI.ViewModels.SettingsViewModel>();
        services.AddTransient<UI.ViewModels.BackupViewModel>();
        services.AddTransient<UI.ViewModels.FieldEditorViewModel>();
        services.AddTransient<UI.ViewModels.ColumnEditorViewModel>();
        services.AddTransient<UI.ViewModels.PluginManagerViewModel>();
        services.AddTransient<UI.ViewModels.DashboardViewModel>();
        services.AddTransient<UI.ViewModels.FilterBuilderViewModel>();

        return services;
    }

    /// <summary>
    /// ثبت Views در NavigationEngine
    /// </summary>
    public static void RegisterAriaViews(this IServiceProvider services)
    {
        var nav = services.GetRequiredService<INavigationEngine>();

        nav.RegisterView("Login", typeof(UI.Views.LoginView), typeof(UI.ViewModels.LoginViewModel));
        nav.RegisterView("Register", typeof(UI.Views.RegisterView), typeof(UI.ViewModels.RegisterViewModel));
        nav.RegisterView("Shell", typeof(UI.Views.ShellView), typeof(UI.ViewModels.ShellViewModel));
        nav.RegisterView("AccountList", typeof(UI.Views.AccountListView), typeof(UI.ViewModels.AccountListViewModel));
        nav.RegisterView("TradeList", typeof(UI.Views.TradeListView), typeof(UI.ViewModels.TradeListViewModel));
        nav.RegisterView("TradeEntry", typeof(UI.Views.TradeEntryView), typeof(UI.ViewModels.TradeEntryViewModel));
        nav.RegisterView("Settings", typeof(UI.Views.SettingsView), typeof(UI.ViewModels.SettingsViewModel));
        nav.RegisterView("Backup", typeof(UI.Views.BackupView), typeof(UI.ViewModels.BackupViewModel));
        nav.RegisterView("FieldEditor", typeof(UI.Views.FieldEditorView), typeof(UI.ViewModels.FieldEditorViewModel));
        nav.RegisterView("ColumnEditor", typeof(UI.Views.ColumnEditorView), typeof(UI.ViewModels.ColumnEditorViewModel));
        nav.RegisterView("PluginManager", typeof(UI.Views.PluginManagerView), typeof(UI.ViewModels.PluginManagerViewModel));
        nav.RegisterView("Dashboard", typeof(UI.Views.DashboardView), typeof(UI.ViewModels.DashboardViewModel));
        nav.RegisterView("FilterBuilder", typeof(UI.Views.FilterBuilderView), typeof(UI.ViewModels.FilterBuilderViewModel));
    }

    /// <summary>
    /// مقداردهی اولیه موتورها
    /// </summary>
    public static async Task InitializeAriaEnginesAsync(this IServiceProvider services)
    {
        // Schema Engine
        var schemaEngine = services.GetRequiredService<ISchemaEngine>();
        await schemaEngine.InitializeAsync();

        // Theme Engine
        var themeEngine = services.GetRequiredService<IThemeEngine>();
        await themeEngine.InitializeAsync();

        // Migration Engine
        using var scope = services.CreateScope();
        var migrationEngine = scope.ServiceProvider.GetRequiredService<IMigrationEngine>();
        await migrationEngine.MigrateAsync();

        // Plugin Engine
        var pluginEngine = services.GetRequiredService<IPluginEngine>();
        pluginEngine.LoadPlugins("plugins");
        await pluginEngine.InitializePluginsAsync(services);

        // Rule Engine - تنظیم UIRenderer
        var ruleEngine = services.GetRequiredService<IRuleEngine>();
        var uiRenderer = services.GetRequiredService<IUIRendererEngine>();
        if (ruleEngine is RuleEngine re)
        {
            re.SetUIRenderer(uiRenderer);
        }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/ServiceCollectionExtensions.cs
// =============================================================================