// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicTab.xaml.cs
// توضیح: کد پشت کنترل تب داینامیک
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل تب داینامیک از Schema
/// </summary>
public partial class DynamicTab : UserControl
{
    private TabSchema? _schema;
    private readonly Dictionary<string, TabItem> _tabs;
    private readonly Dictionary<string, Func<FrameworkElement>> _contentFactories;

    #region Dependency Properties

    public static readonly DependencyProperty SelectedTabIdProperty =
        DependencyProperty.Register(
            nameof(SelectedTabId),
            typeof(string),
            typeof(DynamicTab),
            new PropertyMetadata(null, OnSelectedTabIdChanged));

    public static readonly DependencyProperty CanAddTabsProperty =
        DependencyProperty.Register(
            nameof(CanAddTabs),
            typeof(bool),
            typeof(DynamicTab),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanCloseTabsProperty =
        DependencyProperty.Register(
            nameof(CanCloseTabs),
            typeof(bool),
            typeof(DynamicTab),
            new PropertyMetadata(false));

    #endregion

    #region Properties

    /// <summary>
    /// شناسه تب انتخاب‌شده
    /// </summary>
    public string? SelectedTabId
    {
        get => (string?)GetValue(SelectedTabIdProperty);
        set => SetValue(SelectedTabIdProperty, value);
    }

    /// <summary>
    /// امکان افزودن تب
    /// </summary>
    public bool CanAddTabs
    {
        get => (bool)GetValue(CanAddTabsProperty);
        set => SetValue(CanAddTabsProperty, value);
    }

    /// <summary>
    /// امکان بستن تب‌ها
    /// </summary>
    public bool CanCloseTabs
    {
        get => (bool)GetValue(CanCloseTabsProperty);
        set => SetValue(CanCloseTabsProperty, value);
    }

    /// <summary>
    /// تعداد تب‌ها
    /// </summary>
    public int TabCount => _tabs.Count;

    #endregion

    #region Events

    public event EventHandler<TabChangedEventArgs>? OnTabChanged;
    public event EventHandler<TabClosingEventArgs>? OnTabClosing;
    public event EventHandler? OnAddTabRequested;

    #endregion

    public DynamicTab()
    {
        InitializeComponent();
        _tabs = new Dictionary<string, TabItem>();
        _contentFactories = new Dictionary<string, Func<FrameworkElement>>();
        DataContext = this;
    }

    #region Schema Loading

    /// <summary>
    /// بارگذاری از Schema
    /// </summary>
    public void LoadFromSchema(TabSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));

        MainTabControl.Items.Clear();
        _tabs.Clear();

        CanAddTabs = schema.CanAddTabs;
        CanCloseTabs = schema.CanCloseTabs;

        if (CanAddTabs)
        {
            AddTabButton.Visibility = Visibility.Visible;
        }

        foreach (var tab in schema.Tabs)
        {
            if (!tab.Visible) continue;
            AddTab(tab);
        }

        // انتخاب تب پیش‌فرض
        if (!string.IsNullOrEmpty(schema.DefaultTabId))
        {
            SelectTab(schema.DefaultTabId);
        }
        else if (_tabs.Any())
        {
            SelectTab(_tabs.Keys.First());
        }
    }

    #endregion

    #region Tab Management

    /// <summary>
    /// افزودن تب
    /// </summary>
    public void AddTab(TabItemSchema tabSchema)
    {
        var tabItem = CreateTabItem(tabSchema);
        MainTabControl.Items.Add(tabItem);
        _tabs[tabSchema.Id] = tabItem;
    }

    /// <summary>
    /// افزودن تب با محتوای سفارشی
    /// </summary>
    public void AddTab(string id, string title, FrameworkElement content, string? icon = null, bool closeable = false)
    {
        var tabSchema = new TabItemSchema
        {
            Id = id,
            TitleFa = title,
            Icon = icon,
            Closeable = closeable
        };

        var tabItem = CreateTabItem(tabSchema);
        
        // ذخیره محتوا برای Lazy Loading
        _contentFactories[id] = () => content;
        
        MainTabControl.Items.Add(tabItem);
        _tabs[id] = tabItem;
    }

    /// <summary>
    /// حذف تب
    /// </summary>
    public bool RemoveTab(string tabId)
    {
        if (!_tabs.TryGetValue(tabId, out var tabItem))
            return false;

        // رویداد Closing
        var closingArgs = new TabClosingEventArgs { TabId = tabId };
        OnTabClosing?.Invoke(this, closingArgs);

        if (closingArgs.Cancel)
            return false;

        MainTabControl.Items.Remove(tabItem);
        _tabs.Remove(tabId);
        _contentFactories.Remove(tabId);

        // انتخاب تب دیگر
        if (_tabs.Any() && SelectedTabId == tabId)
        {
            SelectTab(_tabs.Keys.First());
        }

        return true;
    }

    /// <summary>
    /// انتخاب تب
    /// </summary>
    public void SelectTab(string tabId)
    {
        if (!_tabs.TryGetValue(tabId, out var tabItem))
            return;

        MainTabControl.SelectedItem = tabItem;
        SelectedTabId = tabId;

        // بارگذاری محتوا
        LoadTabContent(tabId);
    }

    /// <summary>
    /// تنظیم محتوای تب
    /// </summary>
    public void SetTabContent(string tabId, FrameworkElement content)
    {
        _contentFactories[tabId] = () => content;

        if (SelectedTabId == tabId)
        {
            TabContent.Content = content;
        }
    }

    /// <summary>
    /// تنظیم Content Factory برای Lazy Loading
    /// </summary>
    public void SetTabContentFactory(string tabId, Func<FrameworkElement> factory)
    {
        _contentFactories[tabId] = factory;
    }

    private void LoadTabContent(string tabId)
    {
        if (_contentFactories.TryGetValue(tabId, out var factory))
        {
            TabContent.Content = factory();
        }
        else
        {
            TabContent.Content = new TextBlock
            {
                Text = $"محتوای تب '{tabId}' تعریف نشده است",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = Brushes.Gray
            };
        }
    }

    #endregion

    #region Tab Creation

    private TabItem CreateTabItem(TabItemSchema schema)
    {
        var tabItem = new TabItem
        {
            Tag = schema.Id,
            IsEnabled = schema.Enabled,
            Style = (Style)FindResource("DynamicTabItemStyle")
        };

        // هدر
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // آیکون
        if (!string.IsNullOrEmpty(schema.Icon))
        {
            headerPanel.Children.Add(new TextBlock
            {
                Text = schema.Icon,
                FontSize = 16,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        // عنوان
        headerPanel.Children.Add(new TextBlock
        {
            Text = schema.TitleFa ?? schema.Id,
            VerticalAlignment = VerticalAlignment.Center
        });

        // Badge
        if (!string.IsNullOrEmpty(schema.Badge))
        {
            headerPanel.Children.Add(new Border
            {
                Background = (Brush)FindResource("AccentBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(8, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = schema.Badge,
                    FontSize = 10,
                    Foreground = Brushes.White
                }
            });
        }

        // دکمه بستن
        if (CanCloseTabs && schema.Closeable)
        {
            var closeButton = new Button
            {
                Content = "✕",
                FontSize = 10,
                Margin = new Thickness(10, 0, 0, 0),
                Padding = new Thickness(4, 2, 4, 2),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                Tag = schema.Id
            };
            closeButton.Click += CloseButton_Click;
            headerPanel.Children.Add(closeButton);
        }

        tabItem.Header = headerPanel;

        return tabItem;
    }

    #endregion

    #region Event Handlers

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabControl.SelectedItem is TabItem selectedTab)
        {
            var tabId = selectedTab.Tag?.ToString();
            if (!string.IsNullOrEmpty(tabId) && tabId != SelectedTabId)
            {
                var previousId = SelectedTabId;
                SelectedTabId = tabId;
                LoadTabContent(tabId);
                OnTabChanged?.Invoke(this, new TabChangedEventArgs
                {
                    TabId = tabId,
                    PreviousTabId = previousId
                });
            }
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tabId)
        {
            RemoveTab(tabId);
        }
    }

    private void AddTabButton_Click(object sender, RoutedEventArgs e)
    {
        OnAddTabRequested?.Invoke(this, EventArgs.Empty);
    }

    private static void OnSelectedTabIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicTab tab && e.NewValue is string newId)
        {
            tab.SelectTab(newId);
        }
    }

    #endregion

    #region Tab Visibility & State

    /// <summary>
    /// نمایش/مخفی کردن تب
    /// </summary>
    public void SetTabVisibility(string tabId, bool visible)
    {
        if (_tabs.TryGetValue(tabId, out var tabItem))
        {
            tabItem.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// فعال/غیرفعال کردن تب
    /// </summary>
    public void SetTabEnabled(string tabId, bool enabled)
    {
        if (_tabs.TryGetValue(tabId, out var tabItem))
        {
            tabItem.IsEnabled = enabled;
        }
    }

    /// <summary>
    /// تغییر عنوان تب
    /// </summary>
    public void SetTabTitle(string tabId, string title)
    {
        if (_tabs.TryGetValue(tabId, out var tabItem))
        {
            if (tabItem.Header is StackPanel panel)
            {
                var textBlock = panel.Children.OfType<TextBlock>().Skip(1).FirstOrDefault() 
                                ?? panel.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock != null)
                {
                    textBlock.Text = title;
                }
            }
        }
    }

    /// <summary>
    /// تنظیم Badge تب
    /// </summary>
    public void SetTabBadge(string tabId, string? badge)
    {
        if (!_tabs.TryGetValue(tabId, out var tabItem)) return;
        if (tabItem.Header is not StackPanel panel) return;

        // حذف badge قبلی
        var existingBadge = panel.Children.OfType<Border>().FirstOrDefault();
        if (existingBadge != null)
            panel.Children.Remove(existingBadge);

        // افزودن badge جدید
        if (!string.IsNullOrEmpty(badge))
        {
            // پیدا کردن موقعیت مناسب (قبل از دکمه بستن)
            var insertIndex = panel.Children.Count;
            for (int i = 0; i < panel.Children.Count; i++)
            {
                if (panel.Children[i] is Button)
                {
                    insertIndex = i;
                    break;
                }
            }

            panel.Children.Insert(insertIndex, new Border
            {
                Background = (Brush)FindResource("AccentBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(6, 2, 6, 2),
                Margin = new Thickness(8, 0, 0, 0),
                Child = new TextBlock
                {
                    Text = badge,
                    FontSize = 10,
                    Foreground = Brushes.White
                }
            });
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// دریافت لیست شناسه تب‌ها
    /// </summary>
    public IEnumerable<string> GetTabIds()
    {
        return _tabs.Keys.ToList();
    }

    /// <summary>
    /// بررسی وجود تب
    /// </summary>
    public bool HasTab(string tabId)
    {
        return _tabs.ContainsKey(tabId);
    }

    /// <summary>
    /// پاک کردن همه تب‌ها
    /// </summary>
    public void ClearTabs()
    {
        MainTabControl.Items.Clear();
        _tabs.Clear();
        _contentFactories.Clear();
        TabContent.Content = null;
    }

    #endregion
}

/// <summary>
/// آرگومان رویداد تغییر تب
/// </summary>
public class TabChangedEventArgs : EventArgs
{
    public string TabId { get; set; } = string.Empty;
    public string? PreviousTabId { get; set; }
}

/// <summary>
/// آرگومان رویداد بستن تب
/// </summary>
public class TabClosingEventArgs : EventArgs
{
    public string TabId { get; set; } = string.Empty;
    public bool Cancel { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicTab.xaml.cs
// =============================================================================