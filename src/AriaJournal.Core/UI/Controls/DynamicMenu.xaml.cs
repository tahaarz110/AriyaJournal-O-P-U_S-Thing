// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicMenu.xaml.cs
// توضیح: کد پشت کنترل منوی داینامیک
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل منوی داینامیک از Schema
/// </summary>
public partial class DynamicMenu : UserControl
{
    private MenuSchema? _schema;
    private string? _activeItemId;
    private readonly Dictionary<string, Button> _menuButtons;

    #region Dependency Properties

    public static readonly DependencyProperty ActiveItemIdProperty =
        DependencyProperty.Register(
            nameof(ActiveItemId),
            typeof(string),
            typeof(DynamicMenu),
            new PropertyMetadata(null, OnActiveItemIdChanged));

    public static readonly DependencyProperty ShowIconsProperty =
        DependencyProperty.Register(
            nameof(ShowIcons),
            typeof(bool),
            typeof(DynamicMenu),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CollapsedProperty =
        DependencyProperty.Register(
            nameof(Collapsed),
            typeof(bool),
            typeof(DynamicMenu),
            new PropertyMetadata(false, OnCollapsedChanged));

    #endregion

    #region Properties

    /// <summary>
    /// شناسه آیتم فعال
    /// </summary>
    public string? ActiveItemId
    {
        get => (string?)GetValue(ActiveItemIdProperty);
        set => SetValue(ActiveItemIdProperty, value);
    }

    /// <summary>
    /// نمایش آیکون‌ها
    /// </summary>
    public bool ShowIcons
    {
        get => (bool)GetValue(ShowIconsProperty);
        set => SetValue(ShowIconsProperty, value);
    }

    /// <summary>
    /// حالت جمع‌شده
    /// </summary>
    public bool Collapsed
    {
        get => (bool)GetValue(CollapsedProperty);
        set => SetValue(CollapsedProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<MenuItemClickEventArgs>? OnItemClick;
    public event EventHandler<string>? OnActiveItemChanged;

    #endregion

    public DynamicMenu()
    {
        InitializeComponent();
        _menuButtons = new Dictionary<string, Button>();
    }

    #region Schema Loading

    /// <summary>
    /// بارگذاری منو از Schema
    /// </summary>
    public void LoadFromSchema(MenuSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));

        MenuContainer.Children.Clear();
        _menuButtons.Clear();

        foreach (var group in schema.Groups)
        {
            RenderGroup(group);
        }

        // تنظیم آیتم فعال اولیه
        if (!string.IsNullOrEmpty(schema.DefaultActiveItem))
        {
            SetActiveItem(schema.DefaultActiveItem);
        }
    }

    /// <summary>
    /// افزودن آیتم‌های منو
    /// </summary>
    public void AddItems(IEnumerable<MenuItemSchema> items, string? groupTitle = null)
    {
        if (!string.IsNullOrEmpty(groupTitle))
        {
            var header = new TextBlock
            {
                Text = groupTitle,
                Style = (Style)FindResource("GroupHeaderStyle")
            };
            MenuContainer.Children.Add(header);

            MenuContainer.Children.Add(new Separator
            {
                Margin = new Thickness(15, 0, 15, 5),
                Background = (Brush)FindResource("BorderBrush")
            });
        }

        foreach (var item in items)
        {
            var button = CreateMenuItem(item);
            MenuContainer.Children.Add(button);
        }
    }

    /// <summary>
    /// حذف آیتم منو
    /// </summary>
    public void RemoveItem(string itemId)
    {
        if (_menuButtons.TryGetValue(itemId, out var button))
        {
            MenuContainer.Children.Remove(button);
            _menuButtons.Remove(itemId);
        }
    }

    /// <summary>
    /// پاک کردن منو
    /// </summary>
    public void Clear()
    {
        MenuContainer.Children.Clear();
        _menuButtons.Clear();
        _activeItemId = null;
    }

    #endregion

    #region Rendering

    private void RenderGroup(MenuGroupSchema group)
    {
        // هدر گروه
        if (!string.IsNullOrEmpty(group.TitleFa))
        {
            var header = new TextBlock
            {
                Text = group.TitleFa,
                Style = (Style)FindResource("GroupHeaderStyle")
            };

            if (!string.IsNullOrEmpty(group.Icon))
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(new TextBlock
                {
                    Text = group.Icon,
                    Margin = new Thickness(0, 0, 5, 0)
                });
                panel.Children.Add(header);
                MenuContainer.Children.Add(panel);
            }
            else
            {
                MenuContainer.Children.Add(header);
            }
        }

        // آیتم‌های گروه
        foreach (var item in group.Items)
        {
            if (!item.Visible) continue;

            var button = CreateMenuItem(item);
            MenuContainer.Children.Add(button);
            _menuButtons[item.Id] = button;
        }

        // جداکننده
        if (group.ShowSeparator)
        {
            MenuContainer.Children.Add(new Separator
            {
                Margin = new Thickness(15, 10, 15, 10),
                Background = (Brush)FindResource("BorderBrush")
            });
        }
    }

    private Button CreateMenuItem(MenuItemSchema item)
    {
        var button = new Button
        {
            Tag = item.Id,
            Style = (Style)FindResource("MenuItemStyle"),
            IsEnabled = item.Enabled,
            ToolTip = item.Tooltip,
            Margin = new Thickness(5, 2, 5, 2)
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        // آیکون
        if (ShowIcons && !string.IsNullOrEmpty(item.Icon))
        {
            panel.Children.Add(new TextBlock
            {
                Text = item.Icon,
                FontSize = 18,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        // متن
        if (!Collapsed)
        {
            panel.Children.Add(new TextBlock
            {
                Text = item.TitleFa ?? item.Id,
                VerticalAlignment = VerticalAlignment.Center
            });

            // Badge
            if (!string.IsNullOrEmpty(item.Badge))
            {
                panel.Children.Add(new Border
                {
                    Background = (Brush)FindResource("AccentBrush"),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(8, 2, 8, 2),
                    Margin = new Thickness(10, 0, 0, 0),
                    Child = new TextBlock
                    {
                        Text = item.Badge,
                        FontSize = 11,
                        Foreground = Brushes.White
                    }
                });
            }
        }

        button.Content = panel;

        // رویداد کلیک
        button.Click += (s, e) =>
        {
            SetActiveItem(item.Id);
            OnItemClick?.Invoke(this, new MenuItemClickEventArgs
            {
                ItemId = item.Id,
                Item = item
            });
        };

        // زیرمنو
        if (item.Children?.Any() == true)
        {
            button.ContextMenu = CreateSubMenu(item.Children);
        }

        return button;
    }

    private ContextMenu CreateSubMenu(List<MenuItemSchema> items)
    {
        var menu = new ContextMenu
        {
            FlowDirection = FlowDirection.RightToLeft
        };

        foreach (var item in items)
        {
            if (!item.Visible) continue;

            var menuItem = new MenuItem
            {
                Header = item.TitleFa ?? item.Id,
                Tag = item.Id,
                IsEnabled = item.Enabled
            };

            if (!string.IsNullOrEmpty(item.Icon))
            {
                menuItem.Icon = new TextBlock { Text = item.Icon };
            }

            menuItem.Click += (s, e) =>
            {
                OnItemClick?.Invoke(this, new MenuItemClickEventArgs
                {
                    ItemId = item.Id,
                    Item = item
                });
            };

            menu.Items.Add(menuItem);
        }

        return menu;
    }

    #endregion

    #region Active Item

    /// <summary>
    /// تنظیم آیتم فعال
    /// </summary>
    public void SetActiveItem(string itemId)
    {
        // حذف استایل فعال از آیتم قبلی
        if (!string.IsNullOrEmpty(_activeItemId) && _menuButtons.TryGetValue(_activeItemId, out var prevButton))
        {
            prevButton.Style = (Style)FindResource("MenuItemStyle");
        }

        _activeItemId = itemId;
        ActiveItemId = itemId;

        // اعمال استایل فعال به آیتم جدید
        if (_menuButtons.TryGetValue(itemId, out var button))
        {
            button.Style = (Style)FindResource("ActiveMenuItemStyle");
        }

        OnActiveItemChanged?.Invoke(this, itemId);
    }

    private static void OnActiveItemIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicMenu menu && e.NewValue is string newId)
        {
            menu.SetActiveItem(newId);
        }
    }

    #endregion

    #region Collapsed Mode

    private static void OnCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicMenu menu)
        {
            menu.UpdateCollapsedState();
        }
    }

    private void UpdateCollapsedState()
    {
        if (_schema != null)
        {
            LoadFromSchema(_schema);
        }
    }

    #endregion

    #region Badge

    /// <summary>
    /// تنظیم Badge برای آیتم
    /// </summary>
    public void SetBadge(string itemId, string? badge)
    {
        if (_menuButtons.TryGetValue(itemId, out var button))
        {
            if (button.Content is StackPanel panel)
            {
                // حذف badge قبلی
                var existingBadge = panel.Children.OfType<Border>().FirstOrDefault();
                if (existingBadge != null)
                    panel.Children.Remove(existingBadge);

                // افزودن badge جدید
                if (!string.IsNullOrEmpty(badge))
                {
                    panel.Children.Add(new Border
                    {
                        Background = (Brush)FindResource("AccentBrush"),
                        CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(8, 2, 8, 2),
                        Margin = new Thickness(10, 0, 0, 0),
                        Child = new TextBlock
                        {
                            Text = badge,
                            FontSize = 11,
                            Foreground = Brushes.White
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// فعال/غیرفعال کردن آیتم
    /// </summary>
    public void SetItemEnabled(string itemId, bool enabled)
    {
        if (_menuButtons.TryGetValue(itemId, out var button))
        {
            button.IsEnabled = enabled;
        }
    }

    /// <summary>
    /// نمایش/مخفی کردن آیتم
    /// </summary>
    public void SetItemVisibility(string itemId, bool visible)
    {
        if (_menuButtons.TryGetValue(itemId, out var button))
        {
            button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    #endregion
}

/// <summary>
/// آرگومان رویداد کلیک آیتم منو
/// </summary>
public class MenuItemClickEventArgs : EventArgs
{
    public string ItemId { get; set; } = string.Empty;
    public MenuItemSchema? Item { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicMenu.xaml.cs
// =============================================================================