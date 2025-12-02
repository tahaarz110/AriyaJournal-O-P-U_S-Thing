// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/ShellViewModel.cs
// توضیح: ViewModel صفحه اصلی - نسخه کامل با پشتیبانی Meta-driven + GUI-driven
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.UI.Views;

using AriaJournal.Core.Domain.Events;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel صفحه اصلی
/// </summary>
public partial class ShellViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly AccountService _accountService;
    private readonly INavigationEngine _navigationEngine;
    private readonly IStateEngine _stateEngine;
    private readonly IEventBusEngine _eventBus;
    private readonly IPluginEngine _pluginEngine;

    private User? _currentUser;
    private AccountSummaryDto? _selectedAccount;
    private string _selectedMenuItem = "Trades";
    private ObservableCollection<AccountSummaryDto> _accounts;
    private ObservableCollection<MenuItemViewModel> _menuItems;
    private ObservableCollection<MenuItemViewModel> _pluginMenuItems;
    private ObservableCollection<MenuItemViewModel> _toolsMenuItems;

    public ShellViewModel(
        AuthService authService,
        AccountService accountService,
        INavigationEngine navigationEngine,
        IStateEngine stateEngine,
        IEventBusEngine eventBus,
        IPluginEngine pluginEngine)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _navigationEngine = navigationEngine ?? throw new ArgumentNullException(nameof(navigationEngine));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _pluginEngine = pluginEngine ?? throw new ArgumentNullException(nameof(pluginEngine));

        _accounts = new ObservableCollection<AccountSummaryDto>();
        _menuItems = new ObservableCollection<MenuItemViewModel>();
        _pluginMenuItems = new ObservableCollection<MenuItemViewModel>();
        _toolsMenuItems = new ObservableCollection<MenuItemViewModel>();

        Title = "آریا ژورنال";

        InitializeMenuItems();
        InitializeToolsMenuItems();
        SubscribeToEvents();
    }

    #region Properties

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public string UserDisplayName => CurrentUser?.Username ?? "کاربر";

    public ObservableCollection<AccountSummaryDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    public AccountSummaryDto? SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            if (SetProperty(ref _selectedAccount, value) && value != null)
            {
                _stateEngine.Set(StateKeys.CurrentAccountId, value.Id);
                _eventBus.Publish(new AccountSelectedEvent(value.Id));
            }
        }
    }

    public string SelectedMenuItem
    {
        get => _selectedMenuItem;
        set => SetProperty(ref _selectedMenuItem, value);
    }

    public ObservableCollection<MenuItemViewModel> MenuItems
    {
        get => _menuItems;
        set => SetProperty(ref _menuItems, value);
    }

    public ObservableCollection<MenuItemViewModel> PluginMenuItems
    {
        get => _pluginMenuItems;
        set => SetProperty(ref _pluginMenuItems, value);
    }

    public ObservableCollection<MenuItemViewModel> ToolsMenuItems
    {
        get => _toolsMenuItems;
        set => SetProperty(ref _toolsMenuItems, value);
    }

    public bool HasAccounts => Accounts.Count > 0;
    
    public bool HasToolsMenu => ToolsMenuItems.Count > 0;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task NavigateToAsync(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            return;

        try
        {
            SelectedMenuItem = viewName;
            await _navigationEngine.NavigateToAsync(viewName);
            System.Diagnostics.Debug.WriteLine($"✅ ناوبری به: {viewName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ خطا در ناوبری به {viewName}: {ex.Message}");
            ShowError($"خطا در باز کردن صفحه: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            await _authService.LogoutAsync();

            var loginView = App.GetService<LoginView>();
            var loginViewModel = App.GetService<LoginViewModel>();
            loginView.DataContext = loginViewModel;
            await loginViewModel.InitializeAsync();
            loginView.Show();

            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is ShellView shellView)
                {
                    shellView.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در خروج: {ex.Message}");
            ShowError($"خطا در خروج: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await NavigateToAsync("Settings");
    }

    [RelayCommand]
    private async Task OpenBackupAsync()
    {
        await NavigateToAsync("Backup");
    }

    [RelayCommand]
    private async Task ManageAccountsAsync()
    {
        await NavigateToAsync("Accounts");
    }

    [RelayCommand]
    private async Task NewTradeAsync()
    {
        if (SelectedAccount == null)
        {
            MessageBox.Show(
                "لطفاً ابتدا یک حساب انتخاب کنید یا حساب جدید ایجاد کنید.",
                "توجه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            await NavigateToAsync("Accounts");
            return;
        }
        await NavigateToAsync("TradeEntry");
    }

    [RelayCommand]
    private async Task RefreshAccountsAsync()
    {
        await LoadAccountsOnUIThreadAsync();
    }

    // =====================================================
    // Commands برای Meta-driven + GUI-driven
    // =====================================================

    [RelayCommand]
    private async Task NavigateToFieldEditorAsync()
    {
        await NavigateToAsync("FieldEditor");
    }

    [RelayCommand]
    private async Task NavigateToColumnEditorAsync()
    {
        await NavigateToAsync("ColumnEditor");
    }

    #endregion

    #region Private Methods

    private void InitializeMenuItems()
    {
        MenuItems.Clear();

        MenuItems.Add(new MenuItemViewModel
        {
            Name = "Trades",
            Title = "معاملات",
            Icon = "📊",
            Order = 1
        });

        MenuItems.Add(new MenuItemViewModel
        {
            Name = "Accounts",
            Title = "حساب‌ها",
            Icon = "🏦",
            Order = 2
        });
    }

    /// <summary>
    /// منوی ابزارها برای Meta-driven + GUI-driven
    /// </summary>
    private void InitializeToolsMenuItems()
    {
        ToolsMenuItems.Clear();

        ToolsMenuItems.Add(new MenuItemViewModel
        {
            Name = "FieldEditor",
            Title = "مدیریت فیلدها",
            Icon = "🔧",
            Order = 1
        });

        ToolsMenuItems.Add(new MenuItemViewModel
        {
            Name = "ColumnEditor",
            Title = "مدیریت ستون‌ها",
            Icon = "📊",
            Order = 2
        });

        OnPropertyChanged(nameof(HasToolsMenu));
    }

    private void LoadPluginMenuItems()
    {
        PluginMenuItems.Clear();

        try
        {
            var plugins = _pluginEngine.GetEnabledPlugins()
                .Where(p => p.ShowInMenu)
                .OrderBy(p => p.MenuOrder);

            foreach (var plugin in plugins)
            {
                PluginMenuItems.Add(new MenuItemViewModel
                {
                    Name = plugin.PluginId,
                    Title = plugin.MenuTitle,
                    Icon = plugin.MenuIcon,
                    Order = plugin.MenuOrder,
                    IsPlugin = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری منوی پلاگین‌ها: {ex.Message}");
        }
    }

    private async Task LoadAccountsOnUIThreadAsync()
    {
        if (CurrentUser == null) return;

        try
        {
            var result = await _accountService.GetAccountSummariesAsync(CurrentUser.Id);

            if (result.IsSuccess)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Accounts.Clear();
                    foreach (var account in result.Value)
                    {
                        Accounts.Add(account);
                    }

                    OnPropertyChanged(nameof(HasAccounts));

                    // انتخاب حساب پیش‌فرض
                    if (SelectedAccount == null || !Accounts.Any(a => a.Id == SelectedAccount.Id))
                    {
                        SelectedAccount = Accounts.FirstOrDefault(a => a.IsDefault)
                            ?? Accounts.FirstOrDefault();
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری حساب‌ها: {ex.Message}");
        }
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<AccountChangedEvent>(async e =>
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadAccountsOnUIThreadAsync();
            });
        });

        _eventBus.Subscribe<AccountSelectedEvent>(e =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var account = Accounts.FirstOrDefault(a => a.Id == e.AccountId);
                if (account != null && SelectedAccount?.Id != account.Id)
                {
                    _selectedAccount = account;
                    OnPropertyChanged(nameof(SelectedAccount));
                }
            });
        });

        _eventBus.Subscribe<PluginLoadedEvent>(e =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => LoadPluginMenuItems());
        });

        _eventBus.Subscribe<PluginUnloadedEvent>(e =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => LoadPluginMenuItems());
        });
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        CurrentUser = _authService.CurrentUser;
        OnPropertyChanged(nameof(UserDisplayName));

        await LoadAccountsOnUIThreadAsync();
        LoadPluginMenuItems();

        SelectedMenuItem = "Trades";
    }

    public override void Cleanup()
    {
        // پاکسازی منابع
    }

    #endregion
}

/// <summary>
/// ViewModel آیتم منو
/// </summary>
public class MenuItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsPlugin { get; set; }
    public bool IsSelected { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/ShellViewModel.cs
// =============================================================================