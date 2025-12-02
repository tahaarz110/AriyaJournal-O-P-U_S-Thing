// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicWidget.xaml.cs
// توضیح: کد پشت کنترل ویجت داینامیک - نسخه اصلاح‌شده
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل ویجت داینامیک برای داشبورد
/// </summary>
public partial class DynamicWidget : UserControl
{
    private WidgetSchema? _schema;
    private Func<Task<object?>>? _dataProvider;
    private DateTime _lastUpdate;
    private bool _isLoading;

    #region Dependency Properties

    public static readonly DependencyProperty WidgetIdProperty =
        DependencyProperty.Register(
            nameof(WidgetId),
            typeof(string),
            typeof(DynamicWidget),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CanRemoveProperty =
        DependencyProperty.Register(
            nameof(CanRemove),
            typeof(bool),
            typeof(DynamicWidget),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowSettingsProperty =
        DependencyProperty.Register(
            nameof(ShowSettings),
            typeof(bool),
            typeof(DynamicWidget),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AutoRefreshIntervalProperty =
        DependencyProperty.Register(
            nameof(AutoRefreshInterval),
            typeof(int),
            typeof(DynamicWidget),
            new PropertyMetadata(0, OnAutoRefreshIntervalChanged));

    #endregion

    #region Properties

    /// <summary>
    /// شناسه ویجت
    /// </summary>
    public string WidgetId
    {
        get => (string)GetValue(WidgetIdProperty);
        set => SetValue(WidgetIdProperty, value);
    }

    /// <summary>
    /// امکان حذف ویجت
    /// </summary>
    public bool CanRemove
    {
        get => (bool)GetValue(CanRemoveProperty);
        set => SetValue(CanRemoveProperty, value);
    }

    /// <summary>
    /// نمایش دکمه تنظیمات
    /// </summary>
    public bool ShowSettings
    {
        get => (bool)GetValue(ShowSettingsProperty);
        set => SetValue(ShowSettingsProperty, value);
    }

    /// <summary>
    /// بازه بروزرسانی خودکار (ثانیه) - 0 یعنی غیرفعال
    /// </summary>
    public int AutoRefreshInterval
    {
        get => (int)GetValue(AutoRefreshIntervalProperty);
        set => SetValue(AutoRefreshIntervalProperty, value);
    }

    /// <summary>
    /// آیا در حال بارگذاری است
    /// </summary>
    public bool IsLoading => _isLoading;

    /// <summary>
    /// زمان آخرین بروزرسانی
    /// </summary>
    public DateTime LastUpdate => _lastUpdate;

    #endregion

    #region Events

    public event EventHandler? OnRefreshRequested;
    public event EventHandler? OnSettingsRequested;
    public event EventHandler? OnRemoveRequested;
    public event EventHandler<Exception>? OnError;

    #endregion

    private System.Timers.Timer? _autoRefreshTimer;

    public DynamicWidget()
    {
        InitializeComponent();
        DataContext = this;
    }

    #region Initialization

    /// <summary>
    /// مقداردهی اولیه با Schema
    /// </summary>
    public void Initialize(WidgetSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));

        WidgetId = schema.Id;
        WidgetTitle.Text = schema.TitleFa ?? schema.Id;
        WidgetIcon.Text = schema.Icon ?? "📊";

        // اندازه
        if (schema.Width > 0)
            this.Width = schema.Width;
        if (schema.Height > 0)
            this.Height = schema.Height;

        // MinSize
        if (schema.MinWidth > 0)
            this.MinWidth = schema.MinWidth;
        if (schema.MinHeight > 0)
            this.MinHeight = schema.MinHeight;

        // رنگ پس‌زمینه
        if (!string.IsNullOrEmpty(schema.BackgroundColor))
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(schema.BackgroundColor);
                WidgetBorder.Background = new SolidColorBrush(color);
            }
            catch { }
        }

        // فوتر
        if (schema.ShowFooter)
        {
            WidgetFooter.Visibility = Visibility.Visible;
        }

        // Auto Refresh
        if (schema.RefreshInterval > 0)
        {
            AutoRefreshInterval = schema.RefreshInterval;
        }
    }

    /// <summary>
    /// تنظیم تامین‌کننده داده
    /// </summary>
    public void SetDataProvider(Func<Task<object?>> dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// تنظیم محتوای ویجت
    /// </summary>
    public void SetContent(FrameworkElement content)
    {
        WidgetContent.Content = content;
        HideAllOverlays();
    }

    #endregion

    #region Data Loading

    /// <summary>
    /// بارگذاری داده‌ها
    /// </summary>
    public async Task LoadDataAsync()
    {
        if (_dataProvider == null) return;

        await ShowLoadingAsync();

        try
        {
            var data = await _dataProvider();

            if (data == null)
            {
                ShowNoData();
            }
            else
            {
                HideAllOverlays();
                OnDataLoaded(data);
            }

            _lastUpdate = DateTime.Now;
            UpdateLastUpdateText();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            OnError?.Invoke(this, ex);
        }
    }

    /// <summary>
    /// بروزرسانی داده‌ها
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadDataAsync();
        OnRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnDataLoaded(object data)
    {
        // می‌تواند در کلاس‌های فرزند Override شود
    }

    #endregion

    #region State Management

    private async Task ShowLoadingAsync()
    {
        _isLoading = true;
        await Dispatcher.InvokeAsync(() =>
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            ErrorOverlay.Visibility = Visibility.Collapsed;
            NoDataOverlay.Visibility = Visibility.Collapsed;
        });
    }

    private void HideAllOverlays()
    {
        _isLoading = false;
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            ErrorOverlay.Visibility = Visibility.Collapsed;
            NoDataOverlay.Visibility = Visibility.Collapsed;
        });
    }

    private void ShowError(string message)
    {
        _isLoading = false;
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            ErrorOverlay.Visibility = Visibility.Visible;
            NoDataOverlay.Visibility = Visibility.Collapsed;
            ErrorMessage.Text = message;
        });
    }

    private void ShowNoData()
    {
        _isLoading = false;
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            ErrorOverlay.Visibility = Visibility.Collapsed;
            NoDataOverlay.Visibility = Visibility.Visible;
        });
    }

    private void UpdateLastUpdateText()
    {
        Dispatcher.Invoke(() =>
        {
            LastUpdateText.Text = $"آخرین بروزرسانی: {_lastUpdate:HH:mm:ss}";
        });
    }

    #endregion

    #region Auto Refresh

    private static void OnAutoRefreshIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicWidget widget)
        {
            widget.SetupAutoRefresh((int)e.NewValue);
        }
    }

    private void SetupAutoRefresh(int intervalSeconds)
    {
        // متوقف کردن تایمر قبلی
        _autoRefreshTimer?.Stop();
        _autoRefreshTimer?.Dispose();
        _autoRefreshTimer = null;

        if (intervalSeconds <= 0) return;

        _autoRefreshTimer = new System.Timers.Timer(intervalSeconds * 1000);
        _autoRefreshTimer.Elapsed += async (s, e) =>
        {
            try
            {
                await Dispatcher.InvokeAsync(async () => await RefreshAsync());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در Auto Refresh: {ex.Message}");
            }
        };
        _autoRefreshTimer.AutoReset = true;
        _autoRefreshTimer.Start();
    }

    #endregion

    #region Event Handlers

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        OnSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "آیا از حذف این ویجت اطمینان دارید؟",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (result == MessageBoxResult.Yes)
        {
            OnRemoveRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Widget Types

    /// <summary>
    /// ایجاد ویجت مقدار ساده (KPI)
    /// </summary>
    public void SetAsValueWidget(string value, string? subtitle = null, string? trend = null)
    {
        var panel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var valueBlock = new TextBlock
        {
            Text = value,
            FontSize = 36,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = (Brush)FindResource("ForegroundBrush")
        };
        panel.Children.Add(valueBlock);

        if (!string.IsNullOrEmpty(subtitle))
        {
            var subtitleBlock = new TextBlock
            {
                Text = subtitle,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)FindResource("SecondaryForegroundBrush"),
                Margin = new Thickness(0, 5, 0, 0)
            };
            panel.Children.Add(subtitleBlock);
        }

        if (!string.IsNullOrEmpty(trend))
        {
            var trendBlock = new TextBlock
            {
                Text = trend,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = trend.StartsWith("+") ? Brushes.Green : Brushes.Red,
                Margin = new Thickness(0, 5, 0, 0)
            };
            panel.Children.Add(trendBlock);
        }

        SetContent(panel);
    }

    /// <summary>
    /// ایجاد ویجت لیست
    /// </summary>
    public void SetAsListWidget(IEnumerable<ListWidgetItem> items)
    {
        var listBox = new ListBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            ItemsSource = items
        };

        listBox.ItemTemplate = CreateListItemTemplate();
        SetContent(listBox);
    }

    private DataTemplate CreateListItemTemplate()
    {
        var template = new DataTemplate();

        var factory = new FrameworkElementFactory(typeof(Border));
        factory.SetValue(Border.PaddingProperty, new Thickness(10, 8, 10, 8));
        factory.SetValue(Border.BorderBrushProperty, FindResource("BorderBrush"));
        factory.SetValue(Border.BorderThicknessProperty, new Thickness(0, 0, 0, 1));

        var gridFactory = new FrameworkElementFactory(typeof(Grid));

        var col1 = new FrameworkElementFactory(typeof(ColumnDefinition));
        col1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        var col2 = new FrameworkElementFactory(typeof(ColumnDefinition));
        col2.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);

        gridFactory.AppendChild(col1);
        gridFactory.AppendChild(col2);

        var titleFactory = new FrameworkElementFactory(typeof(TextBlock));
        titleFactory.SetBinding(TextBlock.TextProperty, new Binding("Title"));
        titleFactory.SetValue(Grid.ColumnProperty, 0);
        gridFactory.AppendChild(titleFactory);

        var valueFactory = new FrameworkElementFactory(typeof(TextBlock));
        valueFactory.SetBinding(TextBlock.TextProperty, new Binding("Value"));
        valueFactory.SetValue(Grid.ColumnProperty, 1);
        valueFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        gridFactory.AppendChild(valueFactory);

        factory.AppendChild(gridFactory);
        template.VisualTree = factory;

        return template;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// پاکسازی منابع
    /// </summary>
    public void Cleanup()
    {
        _autoRefreshTimer?.Stop();
        _autoRefreshTimer?.Dispose();
        _autoRefreshTimer = null;
        _dataProvider = null;
        _schema = null;
    }

    #endregion
}

/// <summary>
/// آیتم لیست ویجت
/// </summary>
public class ListWidgetItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public object? Tag { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicWidget.xaml.cs
// =============================================================================