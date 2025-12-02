// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/LoginViewModel.cs
// =============================================================================

using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.UI.Views;

namespace AriaJournal.Core.UI.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly IStateEngine _stateEngine;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private bool _isRegisterMode;
    private string _confirmPassword = string.Empty;
    private bool _showRecoveryKey;
    private string _recoveryKey = string.Empty;
    private bool _showRecoveryForm;
    private string _recoveryKeyInput = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;

    public LoginViewModel(AuthService authService, IStateEngine stateEngine)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
        Title = "آریا ژورنال";
    }

    #region Properties

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public bool IsRegisterMode
    {
        get => _isRegisterMode;
        set
        {
            if (SetProperty(ref _isRegisterMode, value))
            {
                OnPropertyChanged(nameof(IsLoginMode));
                OnPropertyChanged(nameof(SubmitButtonText));
                OnPropertyChanged(nameof(SwitchModeText));
                ClearMessages();
            }
        }
    }

    public bool IsLoginMode => !IsRegisterMode && !ShowRecoveryForm;
    public string SubmitButtonText => IsRegisterMode ? "ثبت‌نام" : "ورود";
    public string SwitchModeText => IsRegisterMode ? "حساب کاربری دارید؟ وارد شوید" : "حساب کاربری ندارید؟ ثبت‌نام کنید";

    public bool ShowRecoveryKey
    {
        get => _showRecoveryKey;
        set => SetProperty(ref _showRecoveryKey, value);
    }

    public string RecoveryKey
    {
        get => _recoveryKey;
        set => SetProperty(ref _recoveryKey, value);
    }

    public bool ShowRecoveryForm
    {
        get => _showRecoveryForm;
        set
        {
            if (SetProperty(ref _showRecoveryForm, value))
                OnPropertyChanged(nameof(IsLoginMode));
        }
    }

    public string RecoveryKeyInput
    {
        get => _recoveryKeyInput;
        set => SetProperty(ref _recoveryKeyInput, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsRegisterMode)
            await RegisterAsync();
        else
            await LoginAsync();
    }

    [RelayCommand]
    private void SwitchMode()
    {
        IsRegisterMode = !IsRegisterMode;
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        ShowRecoveryKey = false;
        ShowRecoveryForm = false;
        RecoveryKey = string.Empty;
        ClearMessages();
    }

    [RelayCommand]
    private void ForgotPassword()
    {
        ShowRecoveryForm = true;
        ShowRecoveryKey = false;
        IsRegisterMode = false;
        ClearMessages();
    }

    [RelayCommand]
    private void BackToLogin()
    {
        ShowRecoveryForm = false;
        ShowRecoveryKey = false;
        IsRegisterMode = false;
        RecoveryKeyInput = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
        ClearMessages();
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
    private void ContinueAfterRegister()
    {
        ShowRecoveryKey = false;
        RecoveryKey = string.Empty;
        IsRegisterMode = false;
        Password = string.Empty;
        ShowSuccess("ثبت‌نام موفق بود. اکنون وارد شوید.");
    }

    [RelayCommand]
    private async Task RecoverPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("نام کاربری را وارد کنید");
            return;
        }

        if (string.IsNullOrWhiteSpace(RecoveryKeyInput))
        {
            ShowError("کلید بازیابی را وارد کنید");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ShowError("رمز عبور جدید را وارد کنید");
            return;
        }

        if (NewPassword.Length < 6)
        {
            ShowError("رمز عبور جدید باید حداقل ۶ کاراکتر باشد");
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ShowError("رمز عبور جدید و تکرار آن یکسان نیستند");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var dto = new RecoverPasswordDto
            {
                Username = Username,
                RecoveryKey = RecoveryKeyInput,
                NewPassword = NewPassword
            };

            var result = await _authService.RecoverPasswordAsync(dto);

            if (result.IsFailure)
            {
                ShowError(result.Error.Message);
                return;
            }

            ShowSuccess("رمز عبور با موفقیت تغییر کرد. اکنون وارد شوید.");
            BackToLogin();

        }, "خطا در بازیابی رمز عبور");
    }

    #endregion

    #region Private Methods

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("نام کاربری را وارد کنید");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("رمز عبور را وارد کنید");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var dto = new LoginDto
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _authService.LoginAsync(dto);

            if (result.IsFailure)
            {
                ShowError(result.Error.Message);
                return;
            }

            if (RememberMe)
                _stateEngine.Set("RememberedUsername", Username);
            else
                _stateEngine.Remove("RememberedUsername");

            OpenShellWindow();

        }, "خطا در ورود");
    }

    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("نام کاربری را وارد کنید");
            return;
        }

        if (Username.Length < 3)
        {
            ShowError("نام کاربری باید حداقل ۳ کاراکتر باشد");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("رمز عبور را وارد کنید");
            return;
        }

        if (Password.Length < 6)
        {
            ShowError("رمز عبور باید حداقل ۶ کاراکتر باشد");
            return;
        }

        if (Password != ConfirmPassword)
        {
            ShowError("رمز عبور و تکرار آن یکسان نیستند");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var dto = new RegisterDto
            {
                Username = Username,
                Password = Password,
                ConfirmPassword = ConfirmPassword
            };

            var result = await _authService.RegisterAsync(dto);

            if (result.IsFailure)
            {
                ShowError(result.Error.Message);
                return;
            }

            RecoveryKey = result.Value.RecoveryKey;
            ShowRecoveryKey = true;

        }, "خطا در ثبت‌نام");
    }

    private void OpenShellWindow()
    {
        

        try
        {
            var shellView = App.GetService<ShellView>();
            var shellViewModel = App.GetService<ShellViewModel>();
            shellView.DataContext = shellViewModel;

            shellView.Show();

            // بستن پنجره لاگین
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is LoginView)
                {
                    window.Close();
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در باز کردن صفحه اصلی:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "خطا",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion

    public override Task InitializeAsync()
    {
        var remembered = _stateEngine.Get<string>("RememberedUsername");
        if (!string.IsNullOrEmpty(remembered))
        {
            Username = remembered;
            RememberMe = true;
        }
        return Task.CompletedTask;
    }
}