// ═══════════════════════════════════════════════════════════════════════
// فایل: RegisterViewModel.cs
// مسیر: src/AriaJournal.Core/UI/ViewModels/RegisterViewModel.cs
// توضیح: ViewModel صفحه ثبت‌نام
// ═══════════════════════════════════════════════════════════════════════

using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.UI.Views;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel صفحه ثبت‌نام
/// </summary>
public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly INavigationEngine _navigationEngine;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _recoveryKey = string.Empty;
    private bool _showRecoveryKey;

    public RegisterViewModel(
        AuthService authService,
        INavigationEngine navigationEngine)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationEngine = navigationEngine ?? throw new ArgumentNullException(nameof(navigationEngine));

        Title = "ثبت‌نام";
    }

    #region Properties

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                OnPropertyChanged(nameof(CanRegister));
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                OnPropertyChanged(nameof(CanRegister));
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                OnPropertyChanged(nameof(CanRegister));
            }
        }
    }

    public string RecoveryKey
    {
        get => _recoveryKey;
        set => SetProperty(ref _recoveryKey, value);
    }

    public bool ShowRecoveryKey
    {
        get => _showRecoveryKey;
        set => SetProperty(ref _showRecoveryKey, value);
    }

    public bool CanRegister =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(Username) &&
        Username.Trim().Length >= 3 &&
        !string.IsNullOrWhiteSpace(Password) &&
        Password.Length >= 6 &&
        Password == ConfirmPassword;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RegisterAsync()
    {
        ClearMessages();

        // اعتبارسنجی
        if (string.IsNullOrWhiteSpace(Username) || Username.Trim().Length < 3)
        {
            ShowError("نام کاربری باید حداقل ۳ کاراکتر باشد");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            ShowError("رمز عبور باید حداقل ۶ کاراکتر باشد");
            return;
        }

        if (Password != ConfirmPassword)
        {
            ShowError("رمز عبور و تکرار آن مطابقت ندارند");
            return;
        }

        IsBusy = true;
        OnPropertyChanged(nameof(CanRegister));

        try
        {
            var result = await _authService.RegisterAsync(Username.Trim(), Password);

            if (result.IsSuccess)
            {
                RecoveryKey = result.Value.RecoveryKey;
                ShowRecoveryKey = true;
                ShowSuccess("ثبت‌نام با موفقیت انجام شد. کلید بازیابی را ذخیره کنید.");
            }
            else
            {
                ShowError(result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در ثبت‌نام: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanRegister));
        }
    }

    [RelayCommand]
    private void CopyRecoveryKey()
    {
        if (!string.IsNullOrEmpty(RecoveryKey))
        {
            Clipboard.SetText(RecoveryKey);
            ShowSuccess("کلید بازیابی کپی شد");
        }
    }

    [RelayCommand]
    private void GoToLogin()
    {
        try
        {
            var loginView = App.GetService<LoginView>();
            var loginViewModel = App.GetService<LoginViewModel>();
            loginView.DataContext = loginViewModel;
            loginView.Show();

            // بستن پنجره فعلی
            foreach (Window window in Application.Current.Windows)
            {
                if (window is RegisterView registerView)
                {
                    registerView.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا: {ex.Message}");
        }
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: RegisterViewModel.cs
// ═══════════════════════════════════════════════════════════════════════