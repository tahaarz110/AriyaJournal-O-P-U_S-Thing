// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/FieldEditorViewModel.cs
// توضیح: ViewModel ویرایشگر فیلدها - سیستم GUI-driven
// بخش ۱ از ۲
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
/// مدل فیلد قابل ویرایش
/// </summary>
public partial class EditableFieldModel : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _fieldName = string.Empty;
    [ObservableProperty] private string _labelFa = string.Empty;
    [ObservableProperty] private string _fieldType = "text";
    [ObservableProperty] private bool _required;
    [ObservableProperty] private bool _visible = true;
    [ObservableProperty] private string? _defaultValue;
    [ObservableProperty] private string? _placeholder;
    [ObservableProperty] private string? _helpText;
    [ObservableProperty] private string? _optionsText;
    [ObservableProperty] private int _order;
    [ObservableProperty] private int? _width;
    [ObservableProperty] private bool _isCustom;
    [ObservableProperty] private bool _isNew;
    [ObservableProperty] private bool _canEdit = true;
    [ObservableProperty] private bool _readOnly;
    [ObservableProperty] private string? _visibleCondition;
    [ObservableProperty] private string? _calculateExpression;

    /// <summary>
    /// نمایش نوع فیلد
    /// </summary>
    public string TypeDisplay => FieldType switch
    {
        "text" => "متن",
        "number" => "عدد",
        "decimal" => "عدد اعشاری",
        "integer" => "عدد صحیح",
        "select" => "لیست انتخابی",
        "multiselect" => "چند انتخابی",
        "date" => "تاریخ",
        "datetime" => "تاریخ و زمان",
        "time" => "زمان",
        "boolean" => "بله/خیر",
        "textarea" => "متن چند خطی",
        "rating" => "امتیاز",
        "color" => "رنگ",
        "file" => "فایل",
        "image" => "تصویر",
        _ => FieldType
    };

    /// <summary>
    /// آیا گزینه‌ها نمایش داده شود
    /// </summary>
    public bool ShowOptions => FieldType == "select" || FieldType == "multiselect";

    /// <summary>
    /// آیا تنظیمات عددی نمایش داده شود
    /// </summary>
    public bool ShowNumericSettings => FieldType == "number" || FieldType == "decimal" || FieldType == "integer";

    /// <summary>
    /// کپی از فیلد
    /// </summary>
    public EditableFieldModel Clone()
    {
        return new EditableFieldModel
        {
            Id = Id,
            FieldName = FieldName,
            LabelFa = LabelFa,
            FieldType = FieldType,
            Required = Required,
            Visible = Visible,
            DefaultValue = DefaultValue,
            Placeholder = Placeholder,
            HelpText = HelpText,
            OptionsText = OptionsText,
            Order = Order,
            Width = Width,
            IsCustom = IsCustom,
            IsNew = IsNew,
            CanEdit = CanEdit,
            ReadOnly = ReadOnly,
            VisibleCondition = VisibleCondition,
            CalculateExpression = CalculateExpression
        };
    }
}

/// <summary>
/// مدل نوع فیلد
/// </summary>
public class FieldTypeModel
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// مدل فرم ساده
/// </summary>
public class SimpleFormModel
{
    public string Id { get; set; } = string.Empty;
    public string TitleFa { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// ViewModel ویرایشگر فیلدها - سیستم GUI-driven
/// </summary>
public partial class FieldEditorViewModel : BaseViewModel
{
    private readonly IMetadataService _metadataService;
    private readonly ISchemaEngine _schemaEngine;
    private readonly AuthService _authService;
    private readonly IEventBusEngine _eventBus;

    #region Observable Properties

    [ObservableProperty] 
    private ObservableCollection<SimpleFormModel> _availableForms = new();

    [ObservableProperty] 
    private SimpleFormModel? _selectedForm;

    [ObservableProperty] 
    private ObservableCollection<EditableFieldModel> _fields = new();

    [ObservableProperty] 
    private EditableFieldModel? _selectedField;

    [ObservableProperty] 
    private EditableFieldModel? _editingField;

    [ObservableProperty] 
    private bool _isEditPanelVisible;

    [ObservableProperty] 
    private string _editPanelTitle = "ویرایش فیلد";

    [ObservableProperty] 
    private bool _hasChanges;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showOnlyCustomFields;

    [ObservableProperty]
    private bool _showOnlyVisibleFields;

    #endregion

    #region Collections

    /// <summary>
    /// انواع فیلدهای موجود
    /// </summary>
    public ObservableCollection<FieldTypeModel> FieldTypes { get; } = new()
    {
        new() { Value = "text", DisplayName = "متن", Icon = "📝", Description = "فیلد متنی ساده" },
        new() { Value = "number", DisplayName = "عدد", Icon = "🔢", Description = "عدد صحیح یا اعشاری" },
        new() { Value = "decimal", DisplayName = "عدد اعشاری", Icon = "💲", Description = "عدد با دقت بالا" },
        new() { Value = "integer", DisplayName = "عدد صحیح", Icon = "🔢", Description = "فقط عدد صحیح" },
        new() { Value = "select", DisplayName = "لیست انتخابی", Icon = "📋", Description = "انتخاب از لیست" },
        new() { Value = "multiselect", DisplayName = "چند انتخابی", Icon = "☑️", Description = "انتخاب چندتایی" },
        new() { Value = "date", DisplayName = "تاریخ", Icon = "📅", Description = "انتخاب تاریخ" },
        new() { Value = "datetime", DisplayName = "تاریخ و زمان", Icon = "🕐", Description = "تاریخ با زمان" },
        new() { Value = "time", DisplayName = "زمان", Icon = "⏰", Description = "فقط زمان" },
        new() { Value = "boolean", DisplayName = "بله/خیر", Icon = "✅", Description = "مقدار منطقی" },
        new() { Value = "textarea", DisplayName = "متن چند خطی", Icon = "📄", Description = "متن طولانی" },
        new() { Value = "rating", DisplayName = "امتیاز", Icon = "⭐", Description = "امتیاز ۱ تا ۵" },
        new() { Value = "color", DisplayName = "رنگ", Icon = "🎨", Description = "انتخاب رنگ" },
        new() { Value = "file", DisplayName = "فایل", Icon = "📁", Description = "آپلود فایل" },
        new() { Value = "image", DisplayName = "تصویر", Icon = "🖼️", Description = "آپلود تصویر" }
    };

    #endregion

    public FieldEditorViewModel(
        IMetadataService metadataService,
        ISchemaEngine schemaEngine,
        AuthService authService,
        IEventBusEngine eventBus)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _schemaEngine = schemaEngine ?? throw new ArgumentNullException(nameof(schemaEngine));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        Title = "مدیریت فیلدها";
    }

    #region Property Changed Handlers

    partial void OnSelectedFormChanged(SimpleFormModel? value)
    {
        if (value != null)
        {
            _ = LoadFieldsAsync();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterFields();
    }

    partial void OnShowOnlyCustomFieldsChanged(bool value)
    {
        FilterFields();
    }

    partial void OnShowOnlyVisibleFieldsChanged(bool value)
    {
        FilterFields();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadFormsAsync();
        if (SelectedForm != null)
        {
            await LoadFieldsAsync();
        }
    }

    [RelayCommand]
    private void AddField()
    {
        EditingField = new EditableFieldModel
        {
            Id = $"custom_{Guid.NewGuid():N}",
            FieldName = string.Empty,
            LabelFa = "فیلد جدید",
            FieldType = "text",
            Visible = true,
            IsCustom = true,
            IsNew = true,
            CanEdit = true,
            Order = Fields.Count
        };

        EditPanelTitle = "➕ افزودن فیلد جدید";
        IsEditPanelVisible = true;
    }

    [RelayCommand]
    private void EditField(EditableFieldModel? field)
    {
        if (field == null) return;

        EditingField = field.Clone();
        EditPanelTitle = field.IsCustom ? $"✏️ ویرایش فیلد: {field.LabelFa}" : $"⚙️ تنظیمات فیلد: {field.LabelFa}";
        IsEditPanelVisible = true;
    }

    [RelayCommand]
    private async Task DeleteFieldAsync(EditableFieldModel? field)
    {
        if (field == null) return;

        if (!field.IsCustom)
        {
            ShowError("فیلدهای سیستمی قابل حذف نیستند. می‌توانید آن‌ها را مخفی کنید.");
            return;
        }

        var result = MessageBox.Show(
            $"آیا از حذف فیلد «{field.LabelFa}» مطمئن هستید؟",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await ExecuteAsync(async () =>
            {
                var userId = _authService.CurrentUser?.Id ?? 0;
                var deleteResult = await _metadataService.DeleteUserDefinedFieldAsync(field.Order);

                if (deleteResult.IsSuccess)
                {
                    Fields.Remove(field);
                    HasChanges = true;
                    ShowSuccess("فیلد با موفقیت حذف شد");
                }
                else
                {
                    ShowError(deleteResult.Error.Message);
                }
            }, "خطا در حذف فیلد");
        }
    }

    [RelayCommand]
    private void MoveUp(EditableFieldModel? field)
    {
        if (field == null) return;

        var index = Fields.IndexOf(field);
        if (index > 0)
        {
            Fields.Move(index, index - 1);
            UpdateFieldOrders();
            HasChanges = true;
        }
    }

    [RelayCommand]
    private void MoveDown(EditableFieldModel? field)
    {
        if (field == null) return;

        var index = Fields.IndexOf(field);
        if (index < Fields.Count - 1)
        {
            Fields.Move(index, index + 1);
            UpdateFieldOrders();
            HasChanges = true;
        }
    }

    [RelayCommand]
    private void ConfirmEdit()
    {
        if (EditingField == null) return;

        // اعتبارسنجی
        if (string.IsNullOrWhiteSpace(EditingField.LabelFa))
        {
            ShowError("نام نمایشی فیلد الزامی است");
            return;
        }

        if (EditingField.IsCustom && EditingField.IsNew && string.IsNullOrWhiteSpace(EditingField.FieldName))
        {
            ShowError("شناسه فیلد الزامی است");
            return;
        }

        if (EditingField.IsNew)
        {
            // افزودن فیلد جدید
            Fields.Add(EditingField);
        }
        else
        {
            // بروزرسانی فیلد موجود
            var existingField = Fields.FirstOrDefault(f => f.Id == EditingField.Id);
            if (existingField != null)
            {
                var index = Fields.IndexOf(existingField);
                Fields[index] = EditingField;
            }
        }

        HasChanges = true;
        IsEditPanelVisible = false;
        EditingField = null;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditPanelVisible = false;
        EditingField = null;
    }

    [RelayCommand]
    private void ShowDefaultFields()
    {
        // بازنشانی فیلترها
        ShowOnlyCustomFields = false;
        ShowOnlyVisibleFields = false;
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        var result = MessageBox.Show(
            "آیا از بازنشانی تنظیمات فیلدها به حالت پیش‌فرض مطمئن هستید؟\nتمام سفارشی‌سازی‌های شما حذف خواهد شد.",
            "تأیید بازنشانی",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes && SelectedForm != null)
        {
            await ExecuteAsync(async () =>
            {
                var userId = _authService.CurrentUser?.Id ?? 0;
                var resetResult = await _metadataService.ResetFieldCustomizationsAsync(userId, SelectedForm.Id);

                if (resetResult.IsSuccess)
                {
                    await LoadFieldsAsync();
                    HasChanges = false;
                    ShowSuccess("تنظیمات فیلدها به حالت پیش‌فرض بازنشانی شد");
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
        if (SelectedForm == null) return;

        await ExecuteAsync(async () =>
        {
            var userId = _authService.CurrentUser?.Id ?? 0;

            // تبدیل به UserFieldCustomization
            var customizations = Fields.Select(f => new UserFieldCustomization
            {
                UserId = userId,
                FormId = SelectedForm.Id,
                FieldId = f.Id,
                Visible = f.Visible,
                Order = f.Order,
                CustomLabel = f.LabelFa,
                Required = f.Required,
                DefaultValue = f.DefaultValue,
                Width = f.Width
            }).ToList();

            var saveResult = await _metadataService.SaveFieldCustomizationsAsync(userId, SelectedForm.Id, customizations);

            if (saveResult.IsSuccess)
            {
                HasChanges = false;
                ShowSuccess("تغییرات با موفقیت ذخیره شد");

                // ارسال رویداد تغییر
                _eventBus.Publish(new SchemaChangedEvent(SelectedForm.Module, "FieldsUpdated"));
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

        // بستن پنجره یا برگشت
    }

    #endregion

    // =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/FieldEditorViewModel.cs
// بخش ۲ از ۲
// =============================================================================

    #region Private Methods

    /// <summary>
    /// بارگذاری لیست فرم‌ها
    /// </summary>
    private async Task LoadFormsAsync()
    {
        await ExecuteAsync(async () =>
        {
            AvailableForms.Clear();

            // دریافت فرم‌ها از SchemaEngine
            var modules = _schemaEngine.GetRegisteredModules();

            foreach (var module in modules)
            {
                var schema = _schemaEngine.GetSchema(module);
                if (schema?.Forms != null)
                {
                    foreach (var form in schema.Forms)
                    {
                        AvailableForms.Add(new SimpleFormModel
                        {
                            Id = form.Id,
                            TitleFa = form.TitleFa,
                            Module = module
                        });
                    }
                }
            }

            // انتخاب فرم اول
            if (AvailableForms.Any() && SelectedForm == null)
            {
                SelectedForm = AvailableForms.First();
            }

            await Task.CompletedTask;
        }, "خطا در بارگذاری فرم‌ها");
    }

    /// <summary>
    /// بارگذاری فیلدهای فرم انتخاب‌شده
    /// </summary>
    private async Task LoadFieldsAsync()
    {
        if (SelectedForm == null) return;

        await ExecuteAsync(async () =>
        {
            Fields.Clear();

            var userId = _authService.CurrentUser?.Id ?? 0;

            // دریافت فرم سفارشی‌شده
            var formResult = await _metadataService.GetCustomizedFormAsync(userId, SelectedForm.Id);

            if (formResult.IsSuccess)
            {
                var form = formResult.Value;
                var order = 0;

                foreach (var section in form.Sections)
                {
                    foreach (var field in section.Fields)
                    {
                        var editableField = new EditableFieldModel
                        {
                            Id = field.Id,
                            FieldName = field.Id,
                            LabelFa = field.LabelFa,
                            FieldType = field.Type,
                            Required = field.Required,
                            Visible = field.Visible,
                            ReadOnly = field.ReadOnly,
                            DefaultValue = field.DefaultValue,
                            Placeholder = field.Placeholder,
                            HelpText = field.HelpText,
                            Width = field.Width,
                            Order = order++,
                            IsCustom = field.Id.StartsWith("custom_"),
                            CanEdit = true,
                            VisibleCondition = field.VisibleCondition,
                            CalculateExpression = field.CalculateExpression
                        };

                        // تبدیل Options به متن
                        if (field.Options != null && field.Options.Any())
                        {
                            editableField.OptionsText = string.Join("\n", 
                                field.Options.Select(o => o.LabelFa));
                        }

                        Fields.Add(editableField);
                    }
                }
            }
            else
            {
                // بارگذاری فرم اصلی
                var form = _schemaEngine.GetForm(SelectedForm.Id);
                if (form != null)
                {
                    var order = 0;
                    foreach (var section in form.Sections)
                    {
                        foreach (var field in section.Fields)
                        {
                            Fields.Add(new EditableFieldModel
                            {
                                Id = field.Id,
                                FieldName = field.Id,
                                LabelFa = field.LabelFa,
                                FieldType = field.Type,
                                Required = field.Required,
                                Visible = field.Visible,
                                DefaultValue = field.DefaultValue,
                                Order = order++,
                                IsCustom = false,
                                CanEdit = true
                            });
                        }
                    }
                }
            }

            HasChanges = false;
        }, "خطا در بارگذاری فیلدها");
    }

    /// <summary>
    /// فیلتر کردن فیلدها
    /// </summary>
    private void FilterFields()
    {
        // این متد می‌تواند با CollectionViewSource پیاده‌سازی شود
        // فعلاً ساده نگه می‌داریم
    }

    /// <summary>
    /// بروزرسانی ترتیب فیلدها
    /// </summary>
    private void UpdateFieldOrders()
    {
        for (int i = 0; i < Fields.Count; i++)
        {
            Fields[i].Order = i;
        }
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadFormsAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/FieldEditorViewModel.cs
// =============================================================================