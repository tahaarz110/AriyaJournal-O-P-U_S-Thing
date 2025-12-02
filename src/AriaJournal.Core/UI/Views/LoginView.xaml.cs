// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/LoginView.xaml.cs
// شماره فایل: 79
// توضیح: کد پشت صفحه ورود
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// صفحه ورود و ثبت‌نام
/// </summary>
public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// رویداد تغییر رمز عبور
    /// </summary>
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

    /// <summary>
    /// رویداد تغییر تکرار رمز عبور
    /// </summary>
    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

    /// <summary>
    /// رویداد تغییر رمز عبور جدید (بازیابی)
    /// </summary>
    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.NewPassword = NewPasswordBox.Password;
        }
    }

    /// <summary>
    /// رویداد تغییر تکرار رمز عبور جدید (بازیابی)
    /// </summary>
    private void ConfirmNewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.ConfirmNewPassword = ConfirmNewPasswordBox.Password;
        }
    }
}

// =============================================================================
// پایان فایل
// =============================================================================