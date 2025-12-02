// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/BackupView.xaml.cs
// توضیح: کد پشت صفحه پشتیبان‌گیری - اصلاح‌شده
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// صفحه پشتیبان‌گیری
/// </summary>
public partial class BackupView : UserControl
{
    public BackupView()
    {
        InitializeComponent();
    }

    private void ConfirmPassword_Click(object sender, RoutedEventArgs e)
    {
        // انتقال رمز عبور از PasswordBox به ViewModel
        if (DataContext is BackupViewModel vm)
        {
            vm.PasswordInput = PasswordBox.Password;
        }
    }
}

// =============================================================================
// پایان فایل
// =============================================================================