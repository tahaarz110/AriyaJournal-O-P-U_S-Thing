// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/FilterBuilderViewModel.cs
// توضیح: ViewModel ساخت فیلتر
// =============================================================================

using System.Collections.ObjectModel;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel ساخت فیلتر
/// </summary>
public partial class FilterBuilderViewModel : BaseViewModel
{
    private readonly IQueryEngine _queryEngine;
    private readonly INavigationEngine _navigationEngine;
    private readonly ISchemaEngine _schemaEngine;

    #region Properties

    [ObservableProperty]
    private ObservableCollection<SavedFilterModel> _savedFilters = new();

    [ObservableProperty]
    private SavedFilterModel? _selectedFilter;

    [ObservableProperty]
    private string _filterName = string.Empty;

    [ObservableProperty]
    private string _selectedIcon = "🔍";

    [ObservableProperty]
    private ObservableCollection<string> _filterIcons = new()
    {
        "🔍", "📊", "💰", "📈", "📉", "🎯", "⭐", "🏆", "⚡", "🔥"
    };

    [ObservableProperty]
    private ObservableCollection<FilterFieldModel> _availableFields = new();

    [ObservableProperty]
    private FilterFieldModel? _selectedField;

    [ObservableProperty]
    private ObservableCollection<OperatorModel> _availableOperators = new();

    [ObservableProperty]
    private OperatorModel? _selectedOperator;

    [ObservableProperty]
    private string _conditionValue = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ConditionModel> _conditions = new();

    [ObservableProperty]
    private ObservableCollection<string> _logicOperators = new() { "و", "یا" };

    #endregion

    #region Constructor

    public FilterBuilderViewModel(
        IQueryEngine queryEngine,
        INavigationEngine navigationEngine,
        ISchemaEngine schemaEngine)
    {
        _queryEngine = queryEngine;
        _navigationEngine = navigationEngine;
        _schemaEngine = schemaEngine;

        LoadAvailableFields();
        LoadAvailableOperators();
        _ = LoadSavedFiltersAsync();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationEngine.NavigateBackAsync();
    }

    [RelayCommand]
    private void AddCondition()
    {
        if (SelectedField == null || SelectedOperator == null)
        {
            ErrorMessage = "لطفاً فیلد و عملگر را انتخاب کنید";
            return;
        }

        var condition = new ConditionModel
        {
            FieldId = SelectedField.Id,
            FieldLabel = SelectedField.LabelFa,
            Operator = SelectedOperator.Value,
            OperatorLabel = SelectedOperator.DisplayName,
            Value = ConditionValue,
            Logic = Conditions.Any() ? "و" : "",
            IsFirst = !Conditions.Any()
        };

        Conditions.Add(condition);

        // پاک کردن فرم
        ConditionValue = string.Empty;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void RemoveCondition(ConditionModel condition)
    {
        Conditions.Remove(condition);
        
        // بروزرسانی IsFirst
        if (Conditions.Any())
        {
            Conditions[0].IsFirst = true;
            Conditions[0].Logic = "";
        }
    }

    [RelayCommand]
    private async Task SaveFilterAsync()
    {
        if (string.IsNullOrWhiteSpace(FilterName))
        {
            ErrorMessage = "لطفاً نام فیلتر را وارد کنید";
            return;
        }

        if (!Conditions.Any())
        {
            ErrorMessage = "حداقل یک شرط باید وجود داشته باشد";
            return;
        }

        var template = new QueryTemplate
        {
            Name = FilterName,
            Filters = Conditions.Select(c => new FilterDefinition
            {
                Field = c.FieldId,
                Operator = ParseOperator(c.Operator),
                Value = c.Value,
                Logic = c.Logic == "یا" ? FilterLogic.Or : FilterLogic.And
            }).ToList()
        };

        var result = await _queryEngine.SaveQueryTemplateAsync(template);

        if (result.IsSuccess)
        {
            SuccessMessage = "فیلتر با موفقیت ذخیره شد";
            await LoadSavedFiltersAsync();
            ClearForm();
        }
        else
        {
            ErrorMessage = result.Error.Message;
        }
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        // اعمال فیلتر و برگشت به لیست معاملات
        // در آینده پیاده‌سازی می‌شود
    }

    [RelayCommand]
    private void Clear()
    {
        ClearForm();
    }

    [RelayCommand]
    private async Task DeleteFilterAsync(SavedFilterModel filter)
    {
        if (filter.IsBuiltIn) return;

        var result = await _queryEngine.DeleteQueryTemplateAsync(filter.Id);
        if (result.IsSuccess)
        {
            await LoadSavedFiltersAsync();
        }
    }

    #endregion

    #region Private Methods

    private void LoadAvailableFields()
    {
        AvailableFields.Clear();

        // فیلدهای پیش‌فرض معامله
        AvailableFields.Add(new FilterFieldModel { Id = "Symbol", LabelFa = "نماد", Type = "text" });
        AvailableFields.Add(new FilterFieldModel { Id = "Direction", LabelFa = "جهت", Type = "select" });
        AvailableFields.Add(new FilterFieldModel { Id = "Volume", LabelFa = "حجم", Type = "number" });
        AvailableFields.Add(new FilterFieldModel { Id = "EntryPrice", LabelFa = "قیمت ورود", Type = "number" });
        AvailableFields.Add(new FilterFieldModel { Id = "ExitPrice", LabelFa = "قیمت خروج", Type = "number" });
        AvailableFields.Add(new FilterFieldModel { Id = "ProfitLoss", LabelFa = "سود/زیان", Type = "number" });
        AvailableFields.Add(new FilterFieldModel { Id = "EntryTime", LabelFa = "زمان ورود", Type = "date" });
        AvailableFields.Add(new FilterFieldModel { Id = "ExitTime", LabelFa = "زمان خروج", Type = "date" });
        AvailableFields.Add(new FilterFieldModel { Id = "IsClosed", LabelFa = "بسته شده", Type = "boolean" });
        AvailableFields.Add(new FilterFieldModel { Id = "FollowedPlan", LabelFa = "طبق پلن", Type = "boolean" });
        AvailableFields.Add(new FilterFieldModel { Id = "IsImpulsive", LabelFa = "هیجانی", Type = "boolean" });
    }

    private void LoadAvailableOperators()
    {
        AvailableOperators.Clear();

        AvailableOperators.Add(new OperatorModel { Value = "Equals", DisplayName = "برابر با" });
        AvailableOperators.Add(new OperatorModel { Value = "NotEquals", DisplayName = "نابرابر با" });
        AvailableOperators.Add(new OperatorModel { Value = "Contains", DisplayName = "شامل" });
        AvailableOperators.Add(new OperatorModel { Value = "StartsWith", DisplayName = "شروع با" });
        AvailableOperators.Add(new OperatorModel { Value = "GreaterThan", DisplayName = "بزرگتر از" });
        AvailableOperators.Add(new OperatorModel { Value = "GreaterThanOrEqual", DisplayName = "بزرگتر یا مساوی" });
        AvailableOperators.Add(new OperatorModel { Value = "LessThan", DisplayName = "کوچکتر از" });
        AvailableOperators.Add(new OperatorModel { Value = "LessThanOrEqual", DisplayName = "کوچکتر یا مساوی" });
        AvailableOperators.Add(new OperatorModel { Value = "IsNull", DisplayName = "خالی" });
        AvailableOperators.Add(new OperatorModel { Value = "IsNotNull", DisplayName = "غیر خالی" });
    }

    private async Task LoadSavedFiltersAsync()
    {
        SavedFilters.Clear();

        // فیلترهای پیش‌فرض
        SavedFilters.Add(new SavedFilterModel
        {
            Id = "winners",
            Name = "معاملات برنده",
            Icon = "🏆",
            Description = "معاملات با سود مثبت",
            IsBuiltIn = true
        });

        SavedFilters.Add(new SavedFilterModel
        {
            Id = "losers",
            Name = "معاملات بازنده",
            Icon = "📉",
            Description = "معاملات با زیان",
            IsBuiltIn = true
        });

        SavedFilters.Add(new SavedFilterModel
        {
            Id = "open",
            Name = "معاملات باز",
            Icon = "🔓",
            Description = "معاملاتی که هنوز بسته نشده‌اند",
            IsBuiltIn = true
        });

        // فیلترهای کاربر
        var templates = await _queryEngine.GetQueryTemplatesAsync();
        foreach (var template in templates)
        {
            SavedFilters.Add(new SavedFilterModel
            {
                Id = template.Id,
                Name = template.Name,
                Icon = "🔍",
                Description = template.Description ?? "",
                IsBuiltIn = false
            });
        }
    }

    private FilterOperator ParseOperator(string operatorStr)
    {
        return operatorStr switch
        {
            "Equals" => FilterOperator.Equals,
            "NotEquals" => FilterOperator.NotEquals,
            "Contains" => FilterOperator.Contains,
            "StartsWith" => FilterOperator.StartsWith,
            "GreaterThan" => FilterOperator.GreaterThan,
            "GreaterThanOrEqual" => FilterOperator.GreaterThanOrEqual,
            "LessThan" => FilterOperator.LessThan,
            "LessThanOrEqual" => FilterOperator.LessThanOrEqual,
            "IsNull" => FilterOperator.IsNull,
            "IsNotNull" => FilterOperator.IsNotNull,
            _ => FilterOperator.Equals
        };
    }

    private void ClearForm()
    {
        FilterName = string.Empty;
        SelectedIcon = "🔍";
        Conditions.Clear();
        ConditionValue = string.Empty;
        SelectedField = null;
        SelectedOperator = null;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    #endregion
}

#region Models

public class SavedFilterModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "🔍";
    public string Description { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}

public class FilterFieldModel
{
    public string Id { get; set; } = string.Empty;
    public string LabelFa { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
}

public class OperatorModel
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class ConditionModel
{
    public string FieldId { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string OperatorLabel { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Logic { get; set; } = string.Empty;
    public bool IsFirst { get; set; }
}

#endregion

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/FilterBuilderViewModel.cs
// =============================================================================