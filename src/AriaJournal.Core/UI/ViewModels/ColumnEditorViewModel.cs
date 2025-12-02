// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/ColumnEditorViewModel.cs
// توضیح: ViewModel ویرایشگر ستون‌ها - نسخه اصلاح‌شده
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.ViewModels;

#region Supporting Models

/// <summary>
/// مدل ستون قابل ویرایش
/// </summary>
public partial class EditableColumnModel : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _field = string.Empty;
    [ObservableProperty] private string _headerFa = string.Empty;
    [ObservableProperty] private string? _headerEn;
    [ObservableProperty] private string _type = "text";
    [ObservableProperty] private int _width = 100;
    [ObservableProperty] private int? _minWidth;
    [ObservableProperty] private int? _maxWidth;
    [ObservableProperty] private bool _visible = true;
    [ObservableProperty] private bool _sortable = true;
    [ObservableProperty] private bool _filterable = true;
    [ObservableProperty] private bool _resizable = true;
    [ObservableProperty] private string? _format;
    [ObservableProperty] private string? _template;
    [ObservableProperty] private string _alignment = "right";
    [ObservableProperty] private int _order;
    [ObservableProperty] private string? _customHeader;
    [ObservableProperty] private string? _cellStyle;
    [ObservableProperty] private string? _headerStyle;
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    /// آیکون نوع
    /// </summary>
    public string Icon => Type switch
    {
        "text" => "📝",
        "number" or "decimal" or "integer" => "🔢",
        "currency" => "💰",
        "date" or "datetime" => "📅",
        "time" => "⏰",
        "boolean" => "✅",
        "enum" => "📋",
        "image" => "🖼️",
        _ => "📄"
    };

    /// <summary>
    /// نمایش نوع
    /// </summary>
    public string TypeDisplay => Type switch
    {
        "text" => "متن",
        "number" => "عدد",
        "decimal" => "اعشاری",
        "integer" => "صحیح",
        "currency" => "ارز",
        "date" => "تاریخ",
        "datetime" => "تاریخ‌زمان",
        "time" => "زمان",
        "boolean" => "منطقی",
        "enum" => "لیست",
        "image" => "تصویر",
        _ => Type
    };

    /// <summary>
    /// کپی از ستون
    /// </summary>
    public EditableColumnModel Clone()
    {
        return new EditableColumnModel
        {
            Id = Id,
            Field = Field,
            HeaderFa = HeaderFa,
            HeaderEn = HeaderEn,
            Type = Type,
            Width = Width,
            MinWidth = MinWidth,
            MaxWidth = MaxWidth,
            Visible = Visible,
            Sortable = Sortable,
            Filterable = Filterable,
            Resizable = Resizable,
            Format = Format,
            Template = Template,
            Alignment = Alignment,
            Order = Order,
            CustomHeader = CustomHeader,
            CellStyle = CellStyle,
            HeaderStyle = HeaderStyle
        };
    }
}

/// <summary>
/// مدل جدول ساده
/// </summary>
public class SimpleTableModel
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// ViewModel ویرایشگر ستون‌ها
/// </summary>
public partial class ColumnEditorViewModel : BaseViewModel
{
    private readonly IMetadataService _metadataService;
    private readonly ISchemaEngine _schemaEngine;
    private readonly AuthService _authService;
    private readonly IEventBusEngine _eventBus;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<SimpleTableModel> _availableTables = new();

    [ObservableProperty]
    private SimpleTableModel? _selectedTable;

    [ObservableProperty]
    private ObservableCollection<EditableColumnModel> _columns = new();

    [ObservableProperty]
    private EditableColumnModel? _selectedColumn;

    [ObservableProperty]
    private EditableColumnModel? _editingColumn;

    [ObservableProperty]
    private bool _isEditPanelVisible;

    [ObservableProperty]
    private string _editPanelTitle = "ویرایش ستون";

    [ObservableProperty]
    private bool _hasChanges;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyVisibleColumns;

    [ObservableProperty]
    private bool _isAllSelected = true;

    #endregion

    #region Computed Properties

    /// <summary>
    /// تعداد ستون‌های نمایان
    /// </summary>
    public int VisibleCount => Columns.Count(c => c.Visible);

    #endregion

    public ColumnEditorViewModel(
        IMetadataService metadataService,
        ISchemaEngine schemaEngine,
        AuthService authService,
        IEventBusEngine eventBus)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _schemaEngine = schemaEngine ?? throw new ArgumentNullException(nameof(schemaEngine));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        Title = "مدیریت ستون‌ها";
    }

    #region Property Changed Handlers

    partial void OnSelectedTableChanged(SimpleTableModel? value)
    {
        if (value != null)
        {
            _ = LoadColumnsAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterColumns();
    }

    partial void OnShowOnlyVisibleColumnsChanged(bool value)
    {
        FilterColumns();
    }

    partial void OnIsAllSelectedChanged(bool value)
    {
        foreach (var column in Columns)
        {
            column.Visible = value;
        }
        HasChanges = true;
        OnPropertyChanged(nameof(VisibleCount));
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadTablesAsync();
        if (SelectedTable != null)
        {
            await LoadColumnsAsync();
        }
    }

    [RelayCommand]
    private void ShowAll()
    {
        foreach (var column in Columns)
        {
            column.Visible = true;
        }
        IsAllSelected = true;
        HasChanges = true;
        OnPropertyChanged(nameof(VisibleCount));
    }

    [RelayCommand]
    private void HideAll()
    {
        foreach (var column in Columns)
        {
            column.Visible = false;
        }
        IsAllSelected = false;
        HasChanges = true;
        OnPropertyChanged(nameof(VisibleCount));
    }

    [RelayCommand]
    private void EditColumn(EditableColumnModel? column)
    {
        if (column == null) return;

        EditingColumn = column.Clone();
        EditPanelTitle = $"⚙️ تنظیمات ستون: {column.HeaderFa}";
        IsEditPanelVisible = true;
    }

    [RelayCommand]
    private void MoveUp(EditableColumnModel? column)
    {
        if (column == null) return;

        var index = Columns.IndexOf(column);
        if (index > 0)
        {
            Columns.Move(index, index - 1);
            UpdateColumnOrders();
            HasChanges = true;
        }
    }

    [RelayCommand]
    private void MoveDown(EditableColumnModel? column)
    {
        if (column == null) return;

        var index = Columns.IndexOf(column);
        if (index < Columns.Count - 1)
        {
            Columns.Move(index, index + 1);
            UpdateColumnOrders();
            HasChanges = true;
        }
    }

    [RelayCommand]
    private void ConfirmEdit()
    {
        if (EditingColumn == null) return;

        // بروزرسانی ستون موجود
        var existingColumn = Columns.FirstOrDefault(c => c.Id == EditingColumn.Id);
        if (existingColumn != null)
        {
            var index = Columns.IndexOf(existingColumn);
            Columns[index] = EditingColumn;
        }

        HasChanges = true;
        IsEditPanelVisible = false;
        EditingColumn = null;
        OnPropertyChanged(nameof(VisibleCount));
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditPanelVisible = false;
        EditingColumn = null;
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        var result = MessageBox.Show(
            "آیا از بازنشانی تنظیمات ستون‌ها به حالت پیش‌فرض مطمئن هستید؟",
            "تأیید بازنشانی",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes && SelectedTable != null)
        {
            await ExecuteAsync(async () =>
            {
                var userId = _authService.CurrentUser?.Id ?? 0;
                var resetResult = await _metadataService.ResetColumnCustomizationsAsync(userId, SelectedTable.Id);

                if (resetResult.IsSuccess)
                {
                    await LoadColumnsAsync();
                    HasChanges = false;
                    ShowSuccess("تنظیمات ستون‌ها به حالت پیش‌فرض بازنشانی شد");
                }
                else
                {
                    ShowError(resetResult.Error.Message);
                }
            }, "خطا در بازنشانی");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedTable == null) return;

        await ExecuteAsync(async () =>
        {
            var userId = _authService.CurrentUser?.Id ?? 0;

            // تبدیل به UserColumnCustomization
            var customizations = Columns.Select(c => new UserColumnCustomization
            {
                UserId = userId,
                TableId = SelectedTable.Id,
                ColumnId = c.Id,
                Visible = c.Visible,
                Order = c.Order,
                Width = c.Width,
                CustomHeader = c.CustomHeader
            }).ToList();

            var saveResult = await _metadataService.SaveColumnCustomizationsAsync(
                userId, SelectedTable.Id, customizations);

            if (saveResult.IsSuccess)
            {
                HasChanges = false;
                ShowSuccess("تغییرات با موفقیت ذخیره شد");

                // ارسال رویداد تغییر
                _eventBus.Publish(new SchemaChangedEvent("Table", "ColumnsUpdated"));
            }
            else
            {
                ShowError(saveResult.Error.Message);
            }
        }, "خطا در ذخیره تغییرات");
    }

    [RelayCommand]
    private void Cancel()
    {
        if (HasChanges)
        {
            var result = MessageBox.Show(
                "تغییرات ذخیره نشده وجود دارد. آیا می‌خواهید بدون ذخیره خارج شوید؟",
                "تأیید",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
                return;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// بارگذاری لیست جداول
    /// </summary>
    private async Task LoadTablesAsync()
    {
        await ExecuteAsync(async () =>
        {
            AvailableTables.Clear();

            // جداول پیش‌فرض
            AvailableTables.Add(new SimpleTableModel
            {
                Id = "tradeList",
                TitleFa = "لیست معاملات",
                DataSource = "Trades"
            });

            AvailableTables.Add(new SimpleTableModel
            {
                Id = "accountList",
                TitleFa = "لیست حساب‌ها",
                DataSource = "Accounts"
            });

            // دریافت جداول از Schema
            var modules = _schemaEngine.GetRegisteredModules();
            foreach (var module in modules)
            {
                var schema = _schemaEngine.GetSchema(module);
                if (schema?.Tables != null)
                {
                    foreach (var table in schema.Tables)
                    {
                        if (!AvailableTables.Any(t => t.Id == table.Id))
                        {
                            AvailableTables.Add(new SimpleTableModel
                            {
                                Id = table.Id,
                                TitleFa = table.TitleFa,
                                DataSource = table.DataSource
                            });
                        }
                    }
                }
            }

            // انتخاب جدول اول
            if (AvailableTables.Any() && SelectedTable == null)
            {
                SelectedTable = AvailableTables.First();
            }

            await Task.CompletedTask;
        }, "خطا در بارگذاری جداول");
    }

    /// <summary>
    /// بارگذاری ستون‌های جدول انتخاب‌شده
    /// </summary>
    private async Task LoadColumnsAsync()
    {
        if (SelectedTable == null) return;

        await ExecuteAsync(async () =>
        {
            Columns.Clear();

            var userId = _authService.CurrentUser?.Id ?? 0;

            // دریافت جدول سفارشی‌شده
            var tableResult = await _metadataService.GetCustomizedTableAsync(userId, SelectedTable.Id);

            if (tableResult.IsSuccess)
            {
                var table = tableResult.Value;

                foreach (var column in table.Columns.OrderBy(c => c.Order))
                {
                    Columns.Add(new EditableColumnModel
                    {
                        Id = column.Id,
                        Field = column.Field,
                        HeaderFa = column.HeaderFa,
                        HeaderEn = column.HeaderEn,
                        Type = column.Type,
                        Width = column.Width,
                        MinWidth = column.MinWidth,
                        MaxWidth = column.MaxWidth,
                        Visible = column.Visible,
                        Sortable = column.Sortable,
                        Filterable = column.Filterable,
                        Resizable = column.Resizable,
                        Format = column.Format,
                        Template = column.Template,
                        Alignment = column.Alignment,
                        Order = column.Order,
                        CellStyle = column.CellStyle,
                        HeaderStyle = column.HeaderStyle
                    });
                }
            }
            else
            {
                // بارگذاری ستون‌های پیش‌فرض
                LoadDefaultColumns();
            }

            HasChanges = false;
            UpdateIsAllSelected();
            OnPropertyChanged(nameof(VisibleCount));
        }, "خطا در بارگذاری ستون‌ها");
    }

    /// <summary>
    /// بارگذاری ستون‌های پیش‌فرض
    /// </summary>
    private void LoadDefaultColumns()
    {
        if (SelectedTable?.DataSource == "Trades")
        {
            var defaultColumns = new[]
            {
                new EditableColumnModel { Id = "id", Field = "Id", HeaderFa = "ردیف", Type = "number", Width = 60, Order = 0 },
                new EditableColumnModel { Id = "symbol", Field = "Symbol", HeaderFa = "نماد", Type = "text", Width = 100, Order = 1 },
                new EditableColumnModel { Id = "direction", Field = "Direction", HeaderFa = "جهت", Type = "enum", Width = 80, Order = 2 },
                new EditableColumnModel { Id = "volume", Field = "Volume", HeaderFa = "حجم", Type = "decimal", Width = 80, Order = 3 },
                new EditableColumnModel { Id = "entryPrice", Field = "EntryPrice", HeaderFa = "قیمت ورود", Type = "decimal", Width = 100, Order = 4 },
                new EditableColumnModel { Id = "exitPrice", Field = "ExitPrice", HeaderFa = "قیمت خروج", Type = "decimal", Width = 100, Order = 5 },
                new EditableColumnModel { Id = "stopLoss", Field = "StopLoss", HeaderFa = "حد ضرر", Type = "decimal", Width = 100, Order = 6 },
                new EditableColumnModel { Id = "takeProfit", Field = "TakeProfit", HeaderFa = "حد سود", Type = "decimal", Width = 100, Order = 7 },
                new EditableColumnModel { Id = "profitLoss", Field = "ProfitLoss", HeaderFa = "سود/زیان", Type = "currency", Width = 100, Order = 8 },
                new EditableColumnModel { Id = "entryTime", Field = "EntryTime", HeaderFa = "زمان ورود", Type = "datetime", Width = 150, Order = 9 },
                new EditableColumnModel { Id = "exitTime", Field = "ExitTime", HeaderFa = "زمان خروج", Type = "datetime", Width = 150, Order = 10 }
            };

            foreach (var col in defaultColumns)
            {
                col.Visible = true;
                col.Sortable = true;
                col.Filterable = true;
                col.Resizable = true;
                col.Alignment = "right";
                Columns.Add(col);
            }
        }
        else if (SelectedTable?.DataSource == "Accounts")
        {
            var defaultColumns = new[]
            {
                new EditableColumnModel { Id = "id", Field = "Id", HeaderFa = "ردیف", Type = "number", Width = 60, Order = 0 },
                new EditableColumnModel { Id = "name", Field = "Name", HeaderFa = "نام", Type = "text", Width = 150, Order = 1 },
                new EditableColumnModel { Id = "type", Field = "Type", HeaderFa = "نوع", Type = "enum", Width = 100, Order = 2 },
                new EditableColumnModel { Id = "broker", Field = "BrokerName", HeaderFa = "بروکر", Type = "text", Width = 120, Order = 3 },
                new EditableColumnModel { Id = "balance", Field = "CurrentBalance", HeaderFa = "موجودی", Type = "currency", Width = 120, Order = 4 },
                new EditableColumnModel { Id = "currency", Field = "Currency", HeaderFa = "ارز", Type = "text", Width = 60, Order = 5 }
            };

            foreach (var col in defaultColumns)
            {
                col.Visible = true;
                col.Sortable = true;
                col.Filterable = true;
                col.Resizable = true;
                col.Alignment = "right";
                Columns.Add(col);
            }
        }
    }

    /// <summary>
    /// فیلتر کردن ستون‌ها
    /// </summary>
    private void FilterColumns()
    {
        // این متد می‌تواند با CollectionViewSource پیاده‌سازی شود
    }

    /// <summary>
    /// بروزرسانی ترتیب ستون‌ها
    /// </summary>
    private void UpdateColumnOrders()
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            Columns[i].Order = i;
        }
    }

    /// <summary>
    /// بروزرسانی وضعیت انتخاب همه
    /// </summary>
    private void UpdateIsAllSelected()
    {
        var allSelected = Columns.All(c => c.Visible);
        if (IsAllSelected != allSelected)
        {
            IsAllSelected = allSelected;
        }
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadTablesAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/ColumnEditorViewModel.cs
// =============================================================================