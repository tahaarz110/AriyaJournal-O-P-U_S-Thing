// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicForm.xaml.cs
// شماره فایل: 106
// توضیح: کد پشت کنترل فرم داینامیک
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل فرم داینامیک - فرم را از Schema می‌سازد
/// </summary>
public partial class DynamicForm : UserControl
{
    private readonly IUIRendererEngine? _uiRenderer;
    private readonly IRuleEngine? _ruleEngine;
    private FrameworkElement? _renderedForm;

    public static readonly DependencyProperty FormIdProperty =
        DependencyProperty.Register(
            nameof(FormId),
            typeof(string),
            typeof(DynamicForm),
            new PropertyMetadata(null, OnFormIdChanged));

    public static readonly DependencyProperty FormSchemaProperty =
        DependencyProperty.Register(
            nameof(FormSchema),
            typeof(FormSchema),
            typeof(DynamicForm),
            new PropertyMetadata(null, OnFormSchemaChanged));

    public static readonly DependencyProperty FormDataProperty =
        DependencyProperty.Register(
            nameof(FormData),
            typeof(object),
            typeof(DynamicForm),
            new PropertyMetadata(null, OnFormDataChanged));

    public DynamicForm()
    {
        InitializeComponent();

        // دریافت سرویس‌ها
        try
        {
            _uiRenderer = App.GetService<IUIRendererEngine>();
            _ruleEngine = App.GetService<IRuleEngine>();
        }
        catch
        {
            // در Design Mode سرویس‌ها در دسترس نیستند
        }
    }

    /// <summary>
    /// شناسه فرم برای بارگذاری از Schema
    /// </summary>
    public string FormId
    {
        get => (string)GetValue(FormIdProperty);
        set => SetValue(FormIdProperty, value);
    }

    /// <summary>
    /// Schema فرم (مستقیم)
    /// </summary>
    public FormSchema FormSchema
    {
        get => (FormSchema)GetValue(FormSchemaProperty);
        set => SetValue(FormSchemaProperty, value);
    }

    /// <summary>
    /// داده فرم برای Binding
    /// </summary>
    public object FormData
    {
        get => GetValue(FormDataProperty);
        set => SetValue(FormDataProperty, value);
    }

    /// <summary>
    /// رندر فرم
    /// </summary>
    public void RenderForm()
    {
        if (_uiRenderer == null) return;

        FormContainer.Children.Clear();

        if (!string.IsNullOrEmpty(FormId))
        {
            _renderedForm = _uiRenderer.RenderForm(FormId);
        }
        else if (FormSchema != null)
        {
            _renderedForm = _uiRenderer.RenderForm(FormSchema);
        }

        if (_renderedForm != null)
        {
            FormContainer.Children.Add(_renderedForm);

            // اعمال داده اگر وجود دارد
            if (FormData != null)
            {
                _uiRenderer.BindData(_renderedForm, FormData);
            }
        }
    }

    /// <summary>
    /// دریافت داده‌های فرم
    /// </summary>
    public Dictionary<string, object?> GetFormData()
    {
        if (_uiRenderer == null || _renderedForm == null)
            return new Dictionary<string, object?>();

        return _uiRenderer.ExtractData(_renderedForm);
    }

    /// <summary>
    /// اعتبارسنجی فرم
    /// </summary>
    public bool Validate(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (_uiRenderer == null || _renderedForm == null)
            return true;

        var result = _uiRenderer.ValidateForm(_renderedForm);
        if (result.IsFailure)
        {
            errorMessage = result.Error.Message;
            return false;
        }

        return true;
    }

    /// <summary>
    /// پاک کردن خطاها
    /// </summary>
    public void ClearErrors()
    {
        if (_uiRenderer != null && _renderedForm != null)
        {
            _uiRenderer.ClearErrors(_renderedForm);
        }
    }

    /// <summary>
    /// اجرای قوانین
    /// </summary>
    public void ApplyRules(string trigger)
    {
        if (_ruleEngine == null || _renderedForm == null || string.IsNullOrEmpty(FormId))
            return;

        _ruleEngine.ApplyRules(FormId, _renderedForm, trigger);
    }

    private static void OnFormIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicForm form)
        {
            form.RenderForm();
        }
    }

    private static void OnFormSchemaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicForm form)
        {
            form.RenderForm();
        }
    }

    private static void OnFormDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicForm form && form._renderedForm != null && form._uiRenderer != null)
        {
            form._uiRenderer.BindData(form._renderedForm, e.NewValue);
        }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicForm.xaml.cs
// =============================================================================