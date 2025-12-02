// ═══════════════════════════════════════════════════════════════════════
// فایل: RegisterView.xaml.cs
// مسیر: src/AriaJournal.Core/UI/Views/RegisterView.xaml.cs
// توضیح: Code-behind صفحه ثبت‌نام
// ═══════════════════════════════════════════════════════════════════════

using System.Windows;
using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// صفحه ثبت‌نام
/// </summary>
public partial class RegisterView : Window
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel)
        {
            viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: RegisterView.xaml.cs
// ═══════════════════════════════════════════════════════════════════════