// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/LoadingIndicator.xaml.cs
// شماره فایل: 101
// توضیح: کد پشت کنترل لودینگ
// =============================================================================

using System.Windows;
using System.Windows.Controls;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل نمایش لودینگ
/// </summary>
public partial class LoadingIndicator : UserControl
{
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(LoadingIndicator),
            new PropertyMetadata("در حال بارگذاری...", OnMessageChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(LoadingIndicator),
            new PropertyMetadata(false, OnIsActiveChanged));

    public LoadingIndicator()
    {
        InitializeComponent();
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingIndicator indicator)
        {
            indicator.MessageText.Text = e.NewValue?.ToString() ?? "";
        }
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingIndicator indicator)
        {
            indicator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/LoadingIndicator.xaml.cs
// =============================================================================