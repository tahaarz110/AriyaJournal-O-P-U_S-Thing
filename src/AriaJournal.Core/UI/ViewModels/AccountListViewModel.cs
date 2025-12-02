// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/AccountListViewModel.cs
// توضیح: ViewModel لیست حساب‌ها - نسخه نهایی اصلاح‌شده
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces.Engines;

using AriaJournal.Core.Domain.Events;
namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel لیست حساب‌های معاملاتی
/// </summary>
public partial class AccountListViewModel : BaseViewModel
{
    private readonly AccountService _accountService;
    private readonly AuthService _authService;
    private readonly INavigationEngine _navigationEngine;
    private readonly IEventBusEngine _eventBus;

    private ObservableCollection<AccountDto> _accounts;
    private AccountDto? _selectedAccount;
    private bool _isDialogOpen;
    private bool _isEditMode;
    private AccountCreateDto _editingAccount;
    private int? _editingAccountId;

    public AccountListViewModel(
        AccountService accountService,
        AuthService authService,
        INavigationEngine navigationEngine,
        IEventBusEngine eventBus)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationEngine = navigationEngine ?? throw new ArgumentNullException(nameof(navigationEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _accounts = new ObservableCollection<AccountDto>();
        _editingAccount = new AccountCreateDto();

        Title = "حساب‌های معاملاتی";

        SubscribeToEvents();
    }

    #region Properties

    public ObservableCollection<AccountDto> Accounts
    {
        get => _accounts;
        set => SetProperty(ref _accounts, value);
    }

    public AccountDto? SelectedAccount
    {
        get => _selectedAccount;
        set => SetProperty(ref _selectedAccount, value);
    }

    public bool IsEmpty => !IsBusy && Accounts.Count == 0;

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetProperty(ref _isDialogOpen, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(DialogTitle));
            }
        }
    }

    public string DialogTitle => IsEditMode ? "ویرایش حساب" : "ایجاد حساب جدید";

    public AccountCreateDto EditingAccount
    {
        get => _editingAccount;
        set => SetProperty(ref _editingAccount, value);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void NewAccount()
    {
        EditingAccount = new AccountCreateDto
        {
            Currency = "USD",
            Leverage = 100,
            Type = AccountType.Demo,
            InitialBalance = 10000
        };
        _editingAccountId = null;
        IsEditMode = false;
        IsDialogOpen = true;
        ClearMessages();
    }

    [RelayCommand]
    private void EditAccount(AccountDto? account)
    {
        if (account == null) return;

        EditingAccount = new AccountCreateDto
        {
            Name = account.Name,
            Type = account.Type,
            BrokerName = account.BrokerName,
            AccountNumber = account.AccountNumber,
            InitialBalance = account.InitialBalance,
            Currency = account.Currency,
            Leverage = account.Leverage,
            Description = account.Description,
            IsDefault = account.IsDefault
        };

        _editingAccountId = account.Id;
        IsEditMode = true;
        IsDialogOpen = true;
        ClearMessages();
    }

    [RelayCommand]
    private async Task SaveAccountAsync()
    {
        if (!ValidateAccount()) return;

        IsBusy = true;

        try
        {
            var userId = _authService.CurrentUser?.Id ?? 0;

            if (IsEditMode && _editingAccountId.HasValue)
            {
                var updateDto = new AccountUpdateDto
                {
                    Id = _editingAccountId.Value,
                    Name = EditingAccount.Name,
                    Type = EditingAccount.Type,
                    BrokerName = EditingAccount.BrokerName,
                    AccountNumber = EditingAccount.AccountNumber,
                    InitialBalance = EditingAccount.InitialBalance,
                    Currency = EditingAccount.Currency,
                    Leverage = EditingAccount.Leverage,
                    Description = EditingAccount.Description,
                    IsDefault = EditingAccount.IsDefault,
                    IsActive = true
                };

                var result = await _accountService.UpdateAsync(updateDto);
                if (result.IsSuccess)
                {
                    IsDialogOpen = false;
                    await LoadAccountsOnUIThreadAsync();
                    ShowSuccess("حساب با موفقیت بروزرسانی شد");
                }
                else
                {
                    ShowError(result.Error.Message);
                }
            }
            else
            {
                var result = await _accountService.CreateAsync(userId, EditingAccount);
                if (result.IsSuccess)
                {
                    IsDialogOpen = false;
                    await LoadAccountsOnUIThreadAsync();
                    ShowSuccess("حساب با موفقیت ایجاد شد");
                    _eventBus.Publish(new AccountChangedEvent(result.Value.Id, "Created"));
                }
                else
                {
                    ShowError(result.Error.Message);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در ذخیره حساب: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelDialog()
    {
        IsDialogOpen = false;
        ClearMessages();
    }

    [RelayCommand]
    private async Task DeleteAccountAsync(AccountDto? account)
    {
        if (account == null) return;

        var result = MessageBox.Show(
            $"آیا از حذف حساب «{account.Name}» مطمئن هستید؟\nتمام معاملات این حساب نیز حذف خواهند شد.",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            IsBusy = true;
            try
            {
                var deleteResult = await _accountService.DeleteAsync(account.Id);
                if (deleteResult.IsSuccess)
                {
                    await LoadAccountsOnUIThreadAsync();
                    ShowSuccess("حساب با موفقیت حذف شد");
                    _eventBus.Publish(new AccountChangedEvent(account.Id, "Deleted"));
                }
                else
                {
                    ShowError(deleteResult.Error.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطا در حذف حساب: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private async Task SetDefaultAsync(AccountDto? account)
    {
        if (account == null || account.IsDefault) return;

        IsBusy = true;
        try
        {
            var userId = _authService.CurrentUser?.Id ?? 0;
            var result = await _accountService.SetDefaultAsync(userId, account.Id);

            if (result.IsSuccess)
            {
                await LoadAccountsOnUIThreadAsync();
                ShowSuccess($"حساب «{account.Name}» به عنوان پیش‌فرض تنظیم شد");
                _eventBus.Publish(new AccountSelectedEvent(account.Id));
            }
            else
            {
                ShowError(result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در تنظیم حساب پیش‌فرض: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAccountsOnUIThreadAsync();
    }

    #endregion

    #region Private Methods

    private async Task LoadAccountsOnUIThreadAsync()
    {
        var userId = _authService.CurrentUser?.Id;
        if (!userId.HasValue) return;

        try
        {
            var result = await _accountService.GetUserAccountsAsync(userId.Value);

            if (result.IsSuccess)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Accounts.Clear();
                    foreach (var account in result.Value)
                    {
                        Accounts.Add(account);
                    }
                    OnPropertyChanged(nameof(IsEmpty));
                });
            }
            else
            {
                ShowError(result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در بارگذاری حساب‌ها: {ex.Message}");
        }
    }

    private bool ValidateAccount()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(EditingAccount.Name))
        {
            ShowError("نام حساب را وارد کنید");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditingAccount.BrokerName))
        {
            ShowError("نام بروکر را وارد کنید");
            return false;
        }

        if (EditingAccount.InitialBalance < 0)
        {
            ShowError("موجودی اولیه نمی‌تواند منفی باشد");
            return false;
        }

        return true;
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
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadAccountsOnUIThreadAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل
// =============================================================================