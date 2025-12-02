// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicDataGrid.xaml.cs
// توضیح: کد پشت کنترل گرید داینامیک - نسخه کامل
// =============================================================================

using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل گرید داینامیک از Schema
/// </summary>
public partial class DynamicDataGrid : UserControl, INotifyPropertyChanged, IDisposable
{
    #region Fields

    private DataGridSchema? _currentSchema;
    private readonly Dictionary<string, object?> _activeFilters;
    private string? _currentSortColumn;
    private bool _sortAscending = true;
    private int _currentPage = 1;
    private int _pageSize = 50;
    private int _totalItems;
    private int _totalPages;
    private bool _isLoading;

    #endregion

    #region Events

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<object?>? OnRowDoubleClick;
    public event EventHandler<object?>? OnViewDetails;
    public event EventHandler<object?>? OnEdit;
    public event EventHandler<object?>? OnDelete;
    public event EventHandler<SelectionChangedEventArgs>? OnSelectionChanged;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(object),
            typeof(DynamicDataGrid),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SchemaIdProperty =
        DependencyProperty.Register(
            nameof(SchemaId),
            typeof(string),
            typeof(DynamicDataGrid),
            new PropertyMetadata(null, OnSchemaIdChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(DynamicDataGrid),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(DynamicDataGrid),
            new PropertyMetadata(1, OnCurrentPageChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(DynamicDataGrid),
            new PropertyMetadata(50, OnPageSizeChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(
            nameof(TotalPages),
            typeof(int),
            typeof(DynamicDataGrid),
            new PropertyMetadata(1));

    public static readonly DependencyProperty ShowPaginationProperty =
        DependencyProperty.Register(
            nameof(ShowPagination),
            typeof(bool),
            typeof(DynamicDataGrid),
            new PropertyMetadata(true, OnShowPaginationChanged));

    #endregion

    #region Properties

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? SchemaId
    {
        get => (string?)GetValue(SchemaIdProperty);
        set => SetValue(SchemaIdProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public bool ShowPagination
    {
        get => (bool)GetValue(ShowPaginationProperty);
        set => SetValue(ShowPaginationProperty, value);
    }

    public IList<object> SelectedItems => MainDataGrid?.SelectedItems.Cast<object>().ToList() ?? new List<object>();

    #endregion

    #region Constructor

    public DynamicDataGrid()
    {
        InitializeComponent();
        _activeFilters = new Dictionary<string, object?>();

        // رویدادها
        MainDataGrid.Sorting += OnGridSorting;
        MainDataGrid.MouseDoubleClick += OnGridDoubleClick;
        MainDataGrid.SelectionChanged += OnGridSelectionChanged;

        // Context Menu
        CreateContextMenu();
    }

    #endregion

    #region Property Changed Handlers

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicDataGrid grid)
        {
            grid.RefreshData();
        }
    }

    private static void OnSchemaIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicDataGrid grid && e.NewValue is string schemaId)
        {
            grid.LoadSchema(schemaId);
        }
    }

    private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicDataGrid grid)
        {
            grid.OnPropertyChanged(nameof(CurrentPage));
            grid.UpdatePaginationInfo();
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicDataGrid grid)
        {
            grid._pageSize = (int)e.NewValue;
            grid.OnPropertyChanged(nameof(PageSize));
            grid.RefreshData();
        }
    }

    private static void OnShowPaginationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicDataGrid grid)
        {
            grid.PaginationPanel.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Schema Loading

    /// <summary>
    /// بارگذاری Schema
    /// </summary>
    public void LoadSchema(string schemaId)
    {
        // در آینده از SchemaEngine بخوانید
        // فعلاً یک schema پیش‌فرض
        _currentSchema = new DataGridSchema
        {
            Id = schemaId,
            AllowSort = true,
            AllowFilter = true,
            AllowPaging = true,
            PageSize = 50
        };

        RenderGrid();
    }

    /// <summary>
    /// تنظیم Schema مستقیم
    /// </summary>
    public void SetSchema(DataGridSchema schema)
    {
        _currentSchema = schema ?? throw new ArgumentNullException(nameof(schema));
        _pageSize = schema.PageSize;
        RenderGrid();
    }

    /// <summary>
    /// رندر ستون‌های گرید
    /// </summary>
    private void RenderGrid()
    {
        if (_currentSchema == null) return;

        MainDataGrid.Columns.Clear();

        // ستون شماره ردیف
        if (_currentSchema.ShowRowNumbers)
        {
            MainDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "#",
                Binding = new Binding { RelativeSource = new RelativeSource(RelativeSourceMode.Self), Path = new PropertyPath("AlternationIndex") },
                Width = new DataGridLength(50),
                IsReadOnly = true
            });
        }

        // ستون‌های از Schema
        foreach (var columnSchema in _currentSchema.Columns.Where(c => c.Visible).OrderBy(c => c.Order))
        {
            var column = CreateColumn(columnSchema);
            MainDataGrid.Columns.Add(column);
        }

        // تنظیمات صفحه‌بندی
        if (!_currentSchema.AllowPaging)
        {
            PaginationPanel.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// ساخت ستون از Schema
    /// </summary>
    private DataGridColumn CreateColumn(DataGridColumnSchema schema)
    {
        DataGridColumn column;

        switch (schema.Type)
        {
            case ColumnType.Boolean:
                column = new DataGridCheckBoxColumn
                {
                    Header = schema.HeaderFa ?? schema.Id,
                    Binding = new Binding(schema.BindingPath ?? schema.Id),
                    IsReadOnly = true
                };
                break;

            case ColumnType.Date:
            case ColumnType.DateTime:
                column = new DataGridTextColumn
                {
                    Header = schema.HeaderFa ?? schema.Id,
                    Binding = new Binding(schema.BindingPath ?? schema.Id)
                    {
                        StringFormat = schema.Format ?? (schema.Type == ColumnType.Date ? "yyyy/MM/dd" : "yyyy/MM/dd HH:mm")
                    },
                    IsReadOnly = true
                };
                break;

            case ColumnType.Currency:
                column = new DataGridTextColumn
                {
                    Header = schema.HeaderFa ?? schema.Id,
                    Binding = new Binding(schema.BindingPath ?? schema.Id)
                    {
                        StringFormat = schema.Format ?? "N2"
                    },
                    IsReadOnly = true
                };
                break;

            case ColumnType.Percent:
                column = new DataGridTextColumn
                {
                    Header = schema.HeaderFa ?? schema.Id,
                    Binding = new Binding(schema.BindingPath ?? schema.Id)
                    {
                        StringFormat = schema.Format ?? "P2"
                    },
                    IsReadOnly = true
                };
                break;

            default:
                column = new DataGridTextColumn
                {
                    Header = schema.HeaderFa ?? schema.Id,
                    Binding = new Binding(schema.BindingPath ?? schema.Id)
                    {
                        StringFormat = schema.Format
                    },
                    IsReadOnly = true
                };
                break;
        }

        // تنظیم عرض
        if (schema.Width > 0)
        {
            column.Width = new DataGridLength(schema.Width);
        }
        column.MinWidth = schema.MinWidth;
        column.MaxWidth = schema.MaxWidth;

        // قابلیت مرتب‌سازی
        column.CanUserSort = schema.Sortable && _currentSchema?.AllowSort == true;

        // قابلیت تغییر اندازه
        column.CanUserResize = schema.Resizable;

        return column;
    }

    #endregion

    #region Data Operations

    /// <summary>
    /// بروزرسانی داده‌ها
    /// </summary>
    public void RefreshData()
    {
        if (ItemsSource == null)
        {
            MainDataGrid.ItemsSource = null;
            ShowNoData();
            return;
        }

        ShowLoading();

        try
        {
            IEnumerable<object> data;

            if (ItemsSource is IEnumerable<object> enumerable)
            {
                data = enumerable;
            }
            else if (ItemsSource is IEnumerable nonGeneric)
            {
                data = nonGeneric.Cast<object>();
            }
            else
            {
                data = new[] { ItemsSource };
            }

            // اعمال فیلترها
            data = ApplyFiltersToData(data);

            // اعمال مرتب‌سازی
            data = ApplySortToData(data);

            // اعمال صفحه‌بندی
            data = ApplyPaginationToData(data);

            var dataList = data.ToList();

            if (!dataList.Any())
            {
                ShowNoData();
            }
            else
            {
                HideOverlays();
                MainDataGrid.ItemsSource = dataList;
            }

            UpdatePaginationInfo();
        }
        finally
        {
            HideLoading();
        }
    }

    #endregion

    #region Filtering

    /// <summary>
    /// اعمال فیلتر روی داده‌ها
    /// </summary>
    public void ApplyFilter(string columnId, object? value)
    {
        if (string.IsNullOrWhiteSpace(columnId)) return;

        _activeFilters[columnId] = value;
        _currentPage = 1;
        RefreshData();
    }

    /// <summary>
    /// حذف فیلتر یک ستون
    /// </summary>
    public void RemoveFilter(string columnId)
    {
        if (_activeFilters.ContainsKey(columnId))
        {
            _activeFilters.Remove(columnId);
            _currentPage = 1;
            RefreshData();
        }
    }

    /// <summary>
    /// پاک کردن همه فیلترها
    /// </summary>
    public void ClearAllFilters()
    {
        _activeFilters.Clear();
        _currentPage = 1;
        RefreshData();
    }

    /// <summary>
    /// دریافت فیلترهای فعال
    /// </summary>
    public Dictionary<string, object?> GetActiveFilters()
    {
        return new Dictionary<string, object?>(_activeFilters);
    }

    private IEnumerable<object> ApplyFiltersToData(IEnumerable<object> data)
    {
        if (!_activeFilters.Any()) return data;

        return data.Where(item =>
        {
            foreach (var filter in _activeFilters)
            {
                var property = item.GetType().GetProperty(filter.Key);
                if (property == null) continue;

                var value = property.GetValue(item);
                if (filter.Value == null) continue;

                var filterStr = filter.Value.ToString()?.ToLower() ?? "";
                var valueStr = value?.ToString()?.ToLower() ?? "";

                if (!valueStr.Contains(filterStr))
                    return false;
            }
            return true;
        });
    }

    #endregion

    #region Sorting

    /// <summary>
    /// مرتب‌سازی داده‌ها
    /// </summary>
    public void Sort(string columnId, bool ascending = true)
    {
        _currentSortColumn = columnId;
        _sortAscending = ascending;
        RefreshData();
    }

    /// <summary>
    /// تغییر جهت مرتب‌سازی
    /// </summary>
    public void ToggleSort(string columnId)
    {
        if (_currentSortColumn == columnId)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _currentSortColumn = columnId;
            _sortAscending = true;
        }
        RefreshData();
    }

    private IEnumerable<object> ApplySortToData(IEnumerable<object> data)
    {
        if (string.IsNullOrWhiteSpace(_currentSortColumn)) return data;

        return _sortAscending
            ? data.OrderBy(x => GetPropertyValue(x, _currentSortColumn))
            : data.OrderByDescending(x => GetPropertyValue(x, _currentSortColumn));
    }

    private object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }

    private void OnGridSorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var columnName = e.Column.SortMemberPath ?? e.Column.Header?.ToString();
        if (!string.IsNullOrEmpty(columnName))
        {
            ToggleSort(columnName);
        }
    }

    #endregion

    #region Pagination

    /// <summary>
    /// رفتن به صفحه بعد
    /// </summary>
    public void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            RefreshData();
        }
    }

    /// <summary>
    /// رفتن به صفحه قبل
    /// </summary>
    public void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            RefreshData();
        }
    }

    /// <summary>
    /// رفتن به صفحه خاص
    /// </summary>
    public void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            RefreshData();
        }
    }

    /// <summary>
    /// رفتن به صفحه اول
    /// </summary>
    public void GoToFirstPage()
    {
        GoToPage(1);
    }

    /// <summary>
    /// رفتن به صفحه آخر
    /// </summary>
    public void GoToLastPage()
    {
        GoToPage(TotalPages);
    }

    private IEnumerable<object> ApplyPaginationToData(IEnumerable<object> data)
    {
        if (_pageSize <= 0) return data;

        var dataList = data.ToList();
        _totalItems = dataList.Count;
        TotalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);

        if (_currentPage > TotalPages && TotalPages > 0)
        {
            _currentPage = TotalPages;
        }

        return dataList
            .Skip((_currentPage - 1) * _pageSize)
            .Take(_pageSize);
    }

    private void UpdatePaginationInfo()
    {
        if (PaginationPanel == null) return;

        var startItem = (_totalItems == 0) ? 0 : ((_currentPage - 1) * _pageSize + 1);
        var endItem = Math.Min(_currentPage * _pageSize, _totalItems);

        if (PageInfoText != null)
        {
            PageInfoText.Text = $"نمایش {startItem} تا {endItem} از {_totalItems} رکورد";
        }

        if (PrevButton != null)
            PrevButton.IsEnabled = _currentPage > 1;

        if (NextButton != null)
            NextButton.IsEnabled = _currentPage < TotalPages;

        if (FirstPageButton != null)
            FirstPageButton.IsEnabled = _currentPage > 1;

        if (LastPageButton != null)
            LastPageButton.IsEnabled = _currentPage < TotalPages;

        if (PageNumberText != null)
            PageNumberText.Text = $"صفحه {_currentPage} از {TotalPages}";
    }

    #endregion

    #region Selection

    /// <summary>
    /// دریافت آیتم‌های انتخاب‌شده
    /// </summary>
    public List<T> GetSelectedItems<T>() where T : class
    {
        return MainDataGrid.SelectedItems.Cast<T>().ToList();
    }

    /// <summary>
    /// پاک کردن انتخاب
    /// </summary>
    public void ClearSelection()
    {
        MainDataGrid.UnselectAll();
    }

    /// <summary>
    /// انتخاب همه
    /// </summary>
    public void SelectAll()
    {
        MainDataGrid.SelectAll();
    }

    #endregion

    #region Export

    /// <summary>
    /// خروجی CSV
    /// </summary>
    public string ExportToCsv()
    {
        if (_currentSchema == null || ItemsSource == null) return string.Empty;

        var sb = new StringBuilder();

        // هدر
        var headers = _currentSchema.Columns
            .Where(c => c.Visible)
            .Select(c => c.HeaderFa ?? c.Id);
        sb.AppendLine(string.Join(",", headers));

        // داده‌ها
        IEnumerable<object> items;
        if (ItemsSource is IEnumerable<object> enumerable)
        {
            items = enumerable;
        }
        else if (ItemsSource is IEnumerable nonGeneric)
        {
            items = nonGeneric.Cast<object>();
        }
        else
        {
            items = new[] { ItemsSource };
        }

        foreach (var item in items)
        {
            var values = _currentSchema.Columns
                .Where(c => c.Visible)
                .Select(c =>
                {
                    var value = GetPropertyValue(item, c.BindingPath ?? c.Id);
                    var strValue = FormatValue(value, c);
                    // Escape CSV
                    if (strValue.Contains(",") || strValue.Contains("\"") || strValue.Contains("\n"))
                    {
                        strValue = $"\"{strValue.Replace("\"", "\"\"")}\"";
                    }
                    return strValue;
                });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private string FormatValue(object? value, DataGridColumnSchema column)
    {
        if (value == null) return "";

        return column.Type switch
        {
            ColumnType.Date => value is DateTime dt ? dt.ToString("yyyy/MM/dd") : value.ToString() ?? "",
            ColumnType.DateTime => value is DateTime dt2 ? dt2.ToString("yyyy/MM/dd HH:mm") : value.ToString() ?? "",
            ColumnType.Currency => value is decimal dec ? dec.ToString("N2") : value.ToString() ?? "",
            ColumnType.Percent => value is decimal pct ? pct.ToString("P2") : value.ToString() ?? "",
            ColumnType.Boolean => value is bool b ? (b ? "بله" : "خیر") : value.ToString() ?? "",
            _ => value.ToString() ?? ""
        };
    }

    /// <summary>
    /// ذخیره به فایل CSV
    /// </summary>
    public async Task<bool> SaveToCsvAsync(string filePath)
    {
        try
        {
            var csv = ExportToCsv();
            await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ذخیره CSV: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Column Visibility

    /// <summary>
    /// نمایش/مخفی کردن ستون
    /// </summary>
    public void SetColumnVisibility(string columnId, bool visible)
    {
        if (_currentSchema == null) return;

        var column = _currentSchema.Columns.FirstOrDefault(c => c.Id == columnId);
        if (column != null)
        {
            column.Visible = visible;
            RenderGrid();
        }
    }

    /// <summary>
    /// دریافت لیست ستون‌ها
    /// </summary>
    public List<ColumnInfo> GetColumnsInfo()
    {
        if (_currentSchema == null) return new List<ColumnInfo>();

        return _currentSchema.Columns.Select(c => new ColumnInfo
        {
            Id = c.Id,
            HeaderFa = c.HeaderFa ?? c.Id,
            Visible = c.Visible,
            Width = c.Width
        }).ToList();
    }

    #endregion

    #region Context Menu

    private void CreateContextMenu()
    {
        var contextMenu = new ContextMenu
        {
            FlowDirection = FlowDirection.RightToLeft
        };

        // کپی
        var copyItem = new MenuItem { Header = "📋 کپی" };
        copyItem.Click += (s, e) => CopySelectedToClipboard();
        contextMenu.Items.Add(copyItem);

        // مشاهده جزئیات
        var viewItem = new MenuItem { Header = "👁️ مشاهده جزئیات" };
        viewItem.Click += (s, e) => RaiseViewDetailsEvent();
        contextMenu.Items.Add(viewItem);

        contextMenu.Items.Add(new Separator());

        // ویرایش
        var editItem = new MenuItem { Header = "✏️ ویرایش" };
        editItem.Click += (s, e) => RaiseEditEvent();
        contextMenu.Items.Add(editItem);

        // حذف
        var deleteItem = new MenuItem { Header = "🗑️ حذف", Foreground = Brushes.Red };
        deleteItem.Click += (s, e) => RaiseDeleteEvent();
        contextMenu.Items.Add(deleteItem);

        contextMenu.Items.Add(new Separator());

        // خروجی
        var exportItem = new MenuItem { Header = "📥 خروجی CSV" };
        exportItem.Click += async (s, e) => await ExportToCsvWithDialogAsync();
        contextMenu.Items.Add(exportItem);

        MainDataGrid.ContextMenu = contextMenu;
    }

    private void CopySelectedToClipboard()
    {
        var selected = MainDataGrid.SelectedItem;
        if (selected == null) return;

        var sb = new StringBuilder();
        var properties = selected.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(selected);
            sb.AppendLine($"{prop.Name}: {value}");
        }

        Clipboard.SetText(sb.ToString());
    }

    private async Task ExportToCsvWithDialogAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            DefaultExt = ".csv",
            FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            var result = await SaveToCsvAsync(dialog.FileName);
            if (result)
            {
                MessageBox.Show("فایل با موفقیت ذخیره شد.", "موفقیت",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("خطا در ذخیره فایل.", "خطا",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnGridDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var selected = MainDataGrid.SelectedItem;
        if (selected != null)
        {
            OnRowDoubleClick?.Invoke(this, selected);
        }
    }

    private void OnGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedItem = MainDataGrid.SelectedItem;
        OnSelectionChanged?.Invoke(this, e);
    }

    private void RaiseViewDetailsEvent()
    {
        OnViewDetails?.Invoke(this, MainDataGrid.SelectedItem);
    }

    private void RaiseEditEvent()
    {
        OnEdit?.Invoke(this, MainDataGrid.SelectedItem);
    }

    private void RaiseDeleteEvent()
    {
        OnDelete?.Invoke(this, MainDataGrid.SelectedItem);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var searchText = SearchBox.Text;
        // اعمال جستجو روی همه ستون‌ها یا ستون خاص
        // فعلاً ساده پیاده‌سازی شده
        RefreshData();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshData();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ExportToCsvWithDialogAsync();
    }

    private void ColumnSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // باز کردن پنجره تنظیمات ستون‌ها
        // در آینده پیاده‌سازی می‌شود
    }

    private void FirstPageButton_Click(object sender, RoutedEventArgs e)
    {
        GoToFirstPage();
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        PreviousPage();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        NextPage();
    }

    private void LastPageButton_Click(object sender, RoutedEventArgs e)
    {
        GoToLastPage();
    }

    private void PageSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeCombo.SelectedItem is ComboBoxItem item)
        {
            if (int.TryParse(item.Content?.ToString(), out var size))
            {
                PageSize = size;
            }
        }
    }

    #endregion

    #region UI State

    private void ShowLoading()
    {
        _isLoading = true;
        LoadingOverlay.Visibility = Visibility.Visible;
        NoDataOverlay.Visibility = Visibility.Collapsed;
    }

    private void HideLoading()
    {
        _isLoading = false;
        LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private void ShowNoData()
    {
        NoDataOverlay.Visibility = Visibility.Visible;
        LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private void HideOverlays()
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
        NoDataOverlay.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Helper Classes

    public class ColumnInfo
    {
        public string Id { get; set; } = string.Empty;
        public string HeaderFa { get; set; } = string.Empty;
        public bool Visible { get; set; } = true;
        public int Width { get; set; } = 100;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _activeFilters.Clear();
        _currentSchema = null;

        if (MainDataGrid != null)
        {
            MainDataGrid.Sorting -= OnGridSorting;
            MainDataGrid.MouseDoubleClick -= OnGridDoubleClick;
            MainDataGrid.SelectionChanged -= OnGridSelectionChanged;
        }
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicDataGrid.xaml.cs
// =============================================================================