// =============================================================================
// فایل: src/AriaJournal.Core/UI/Views/ShellView.xaml.cs
// توضیح: کد پشت صفحه اصلی - اصلاح ترتیب لود
// =============================================================================

using System.Windows;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.UI.Views;

/// <summary>
/// صفحه اصلی برنامه
/// </summary>
public partial class ShellView : Window
{
    private readonly INavigationEngine _navigationEngine;

    public ShellView()
    {
        InitializeComponent();

        // دریافت NavigationEngine از DI
        _navigationEngine = App.GetService<INavigationEngine>();

        // تنظیم MainFrame برای ناوبری
        _navigationEngine.SetMainFrame(MainContent);

        // وقتی پنجره لود شد
        Loaded += ShellView_Loaded;
    }

    private async void ShellView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // ابتدا ViewModel را Initialize کن (حساب‌ها لود شوند)
            if (DataContext is ShellViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }

            // سپس به صفحه معاملات برو
            await _navigationEngine.NavigateToAsync("Trades");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در لود صفحه اولیه: {ex.Message}");
        }
    }
}

// =============================================================================
// پایان فایل
// =============================================================================