// ═══════════════════════════════════════════════════════════════════════
// فایل: DynamicField.xaml.cs
// مسیر: src/AriaJournal.Core/UI/Controls/DynamicField.xaml.cs
// توضیح: کنترل فیلد داینامیک - Code-behind
// ═══════════════════════════════════════════════════════════════════════

using System.Windows;
using System.Windows.Controls;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// کنترل فیلد داینامیک
/// </summary>
public partial class DynamicField : UserControl
{
    public DynamicField()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    /// <summary>
    /// Schema فیلد
    /// </summary>
    public static readonly DependencyProperty FieldSchemaProperty =
        DependencyProperty.Register(
            nameof(FieldSchema),
            typeof(FieldSchema),
            typeof(DynamicField),
            new PropertyMetadata(null, OnFieldSchemaChanged));

    public FieldSchema? FieldSchema
    {
        get => (FieldSchema?)GetValue(FieldSchemaProperty);
        set => SetValue(FieldSchemaProperty, value);
    }

    /// <summary>
    /// مقدار فیلد
    /// </summary>
    public static readonly DependencyProperty FieldValueProperty =
        DependencyProperty.Register(
            nameof(FieldValue),
            typeof(object),
            typeof(DynamicField),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? FieldValue
    {
        get => GetValue(FieldValueProperty);
        set => SetValue(FieldValueProperty, value);
    }

    /// <summary>
    /// متن Label
    /// </summary>
    public static readonly DependencyProperty LabelTextProperty =
        DependencyProperty.Register(
            nameof(LabelText),
            typeof(string),
            typeof(DynamicField),
            new PropertyMetadata(string.Empty));

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    /// <summary>
    /// آیا Label نمایش داده شود؟
    /// </summary>
    public static readonly DependencyProperty ShowLabelProperty =
        DependencyProperty.Register(
            nameof(ShowLabel),
            typeof(bool),
            typeof(DynamicField),
            new PropertyMetadata(true));

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    /// <summary>
    /// آیا فیلد الزامی است؟
    /// </summary>
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(
            nameof(IsRequired),
            typeof(bool),
            typeof(DynamicField),
            new PropertyMetadata(false));

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>
    /// پیام خطا
    /// </summary>
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.Register(
            nameof(ErrorMessage),
            typeof(string),
            typeof(DynamicField),
            new PropertyMetadata(string.Empty));

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    /// <summary>
    /// آیا خطا دارد؟
    /// </summary>
    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(
            nameof(HasError),
            typeof(bool),
            typeof(DynamicField),
            new PropertyMetadata(false));

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    /// <summary>
    /// محتوای فیلد
    /// </summary>
    public static readonly DependencyProperty FieldContentProperty =
        DependencyProperty.Register(
            nameof(FieldContent),
            typeof(FrameworkElement),
            typeof(DynamicField),
            new PropertyMetadata(null));

    public FrameworkElement? FieldContent
    {
        get => (FrameworkElement?)GetValue(FieldContentProperty);
        set => SetValue(FieldContentProperty, value);
    }

    #endregion

    #region Event Handlers

    private static void OnFieldSchemaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DynamicField field && e.NewValue is FieldSchema schema)
        {
            field.BuildField(schema);
        }
    }

    #endregion

    #region Private Methods

    private void BuildField(FieldSchema schema)
    {
        LabelText = schema.LabelFa;
        IsRequired = schema.Required;

        FrameworkElement control = schema.Type?.ToLower() switch
        {
            "text" => CreateTextBox(schema),
            "number" or "decimal" => CreateNumberBox(schema),
            "select" or "dropdown" => CreateComboBox(schema),
            "checkbox" or "bool" => CreateCheckBox(schema),
            "date" => CreateDatePicker(schema),
            "datetime" => CreateDateTimePicker(schema),
            "textarea" or "multiline" => CreateTextArea(schema),
            "rating" => CreateRating(schema),
            _ => CreateTextBox(schema)
        };

        FieldContent = control;
    }

    private TextBox CreateTextBox(FieldSchema schema)
    {
        var textBox = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Padding = new Thickness(10),
        };

        if (!string.IsNullOrEmpty(schema.Placeholder))
        {
            // TODO: اضافه کردن placeholder
        }

        textBox.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(nameof(FieldValue))
        {
            Source = this,
            Mode = System.Windows.Data.BindingMode.TwoWay,
            UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
        });

        return textBox;
    }

    private TextBox CreateNumberBox(FieldSchema schema)
    {
        var textBox = CreateTextBox(schema);
        textBox.PreviewTextInput += (s, e) =>
        {
            e.Handled = !decimal.TryParse(((TextBox)s).Text + e.Text, out _);
        };
        return textBox;
    }

    private ComboBox CreateComboBox(FieldSchema schema)
    {
        var comboBox = new ComboBox
        {
            Style = (Style)FindResource("ModernComboBox"),
            Padding = new Thickness(10),
        };

        if (schema.Options != null)
        {
            foreach (var option in schema.Options)
            {
                comboBox.Items.Add(new ComboBoxItem
                {
                    Content = option.LabelFa,
                    Tag = option.Value
                });
            }
        }

        comboBox.SetBinding(ComboBox.SelectedValueProperty, new System.Windows.Data.Binding(nameof(FieldValue))
        {
            Source = this,
            Mode = System.Windows.Data.BindingMode.TwoWay
        });

        return comboBox;
    }

    private CheckBox CreateCheckBox(FieldSchema schema)
    {
        var checkBox = new CheckBox
        {
            Content = schema.LabelFa,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        ShowLabel = false;

        checkBox.SetBinding(CheckBox.IsCheckedProperty, new System.Windows.Data.Binding(nameof(FieldValue))
        {
            Source = this,
            Mode = System.Windows.Data.BindingMode.TwoWay
        });

        return checkBox;
    }

    private DatePicker CreateDatePicker(FieldSchema schema)
    {
        var datePicker = new DatePicker
        {
            Padding = new Thickness(10),
        };

        datePicker.SetBinding(DatePicker.SelectedDateProperty, new System.Windows.Data.Binding(nameof(FieldValue))
        {
            Source = this,
            Mode = System.Windows.Data.BindingMode.TwoWay
        });

        return datePicker;
    }

    private StackPanel CreateDateTimePicker(FieldSchema schema)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        
        var datePicker = CreateDatePicker(schema);
        datePicker.Width = 150;
        datePicker.Margin = new Thickness(0, 0, 10, 0);

        var timeBox = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Width = 80,
            Padding = new Thickness(10),
        };

        panel.Children.Add(datePicker);
        panel.Children.Add(timeBox);

        return panel;
    }

    private TextBox CreateTextArea(FieldSchema schema)
    {
        var textBox = CreateTextBox(schema);
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.AcceptsReturn = true;
        textBox.MinHeight = 80;
        textBox.MaxHeight = 200;
        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        return textBox;
    }

    private StackPanel CreateRating(FieldSchema schema)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        for (int i = 1; i <= 5; i++)
        {
            var radio = new RadioButton
            {
                Content = i.ToString(),
                GroupName = $"Rating_{schema.Id}",
                Margin = new Thickness(0, 0, 10, 0),
                Tag = i
            };

            radio.Checked += (s, e) =>
            {
                if (s is RadioButton rb && rb.Tag is int value)
                {
                    FieldValue = value;
                }
            };

            panel.Children.Add(radio);
        }

        return panel;
    }

    #endregion

    #region Validation

    /// <summary>
    /// اعتبارسنجی فیلد
    /// </summary>
    public bool Validate()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        if (IsRequired && (FieldValue == null || string.IsNullOrWhiteSpace(FieldValue.ToString())))
        {
            HasError = true;
            ErrorMessage = $"{LabelText} الزامی است";
            return false;
        }

        // TODO: اعتبارسنجی‌های بیشتر بر اساس FieldSchema.Validation

        return true;
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: DynamicField.xaml.cs
// ═══════════════════════════════════════════════════════════════════════