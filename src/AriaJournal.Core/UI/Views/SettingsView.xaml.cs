// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/SettingsView.xaml.cs
// شماره فایل: 95
// توضیح: کد پشت صفحه تنظیمات
// =============================================================================

using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// صفحه تنظیمات
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        // اتصال PasswordBox ها به ViewModel
        CurrentPasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is SettingsViewModel vm)
                vm.CurrentPassword = CurrentPasswordBox.Password;
        };

        NewPasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is SettingsViewModel vm)
                vm.NewPassword = NewPasswordBox.Password;
        };

        ConfirmPasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is SettingsViewModel vm)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        };
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Views/SettingsView.xaml.cs
// =============================================================================