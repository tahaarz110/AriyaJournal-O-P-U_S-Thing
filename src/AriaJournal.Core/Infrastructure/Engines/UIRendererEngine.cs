// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/UIRendererEngine.cs
// توضیح: موتور رندر UI از Schema - اصلاح‌شده برای سازگاری با FieldSchema
// =============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور رندر UI از Schema
/// </summary>
public class UIRendererEngine : IUIRendererEngine
{
    private readonly ISchemaEngine _schemaEngine;
    private readonly IRuleEngine _ruleEngine;
    private readonly Dictionary<string, FrameworkElement> _fieldElements;
    private readonly Dictionary<string, object?> _fieldValues;

    private const string FieldTagPrefix = "Field_";
    private const string ErrorTagPrefix = "Error_";

    public UIRendererEngine(ISchemaEngine schemaEngine, IRuleEngine ruleEngine)
    {
        _schemaEngine = schemaEngine ?? throw new ArgumentNullException(nameof(schemaEngine));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _fieldElements = new Dictionary<string, FrameworkElement>();
        _fieldValues = new Dictionary<string, object?>();
    }

    public FrameworkElement RenderForm(string formId)
    {
        var schema = _schemaEngine.GetForm(formId);
        if (schema == null)
        {
            return CreateErrorPanel($"فرم '{formId}' یافت نشد");
        }

        return RenderForm(schema);
    }

    public FrameworkElement RenderForm(FormSchema schema)
    {
        _fieldElements.Clear();
        _fieldValues.Clear();

        var mainPanel = new Grid
        {
            FlowDirection = schema.Direction == "rtl" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            Tag = schema.Id
        };

        // تعریف ردیف‌ها
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // عنوان
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // محتوا
        mainPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // دکمه‌ها

        // عنوان فرم
        var titleBlock = new TextBlock
        {
            Text = schema.TitleFa,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 15),
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetRow(titleBlock, 0);
        mainPanel.Children.Add(titleBlock);

        // محتوا (Sections)
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var contentPanel = new StackPanel
        {
            Margin = new Thickness(0, 10, 0, 10)
        };

        foreach (var section in schema.Sections)
        {
            var sectionElement = RenderSection(section);
            contentPanel.Children.Add(sectionElement);
        }

        scrollViewer.Content = contentPanel;
        Grid.SetRow(scrollViewer, 1);
        mainPanel.Children.Add(scrollViewer);

        // دکمه‌ها
        if (schema.Actions.Any())
        {
            var actionsPanel = RenderActions(schema.Actions);
            Grid.SetRow(actionsPanel, 2);
            mainPanel.Children.Add(actionsPanel);
        }

        // اعمال قوانین OnLoad
        _ruleEngine.ApplyRules(schema.Id, mainPanel, "OnLoad");

        return mainPanel;
    }

    private FrameworkElement RenderSection(SectionSchema section)
    {
        var expander = new Expander
        {
            Header = section.TitleFa,
            IsExpanded = !section.Collapsed,
            Margin = new Thickness(0, 0, 0, 10),
            FlowDirection = FlowDirection.RightToLeft
        };

        var grid = new Grid
        {
            Margin = new Thickness(10)
        };

        // تعریف ستون‌ها
        for (int i = 0; i < section.Columns; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        int row = 0;
        int col = 0;

        foreach (var field in section.Fields)
        {
            // افزودن ردیف جدید در صورت نیاز
            if (col == 0 || grid.RowDefinitions.Count <= row)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var fieldElement = RenderField(field);
            
            Grid.SetRow(fieldElement, row);
            Grid.SetColumn(fieldElement, col);

            // استفاده از ColSpan یا ColumnSpan
            var colSpan = field.ColSpan ?? field.ColumnSpan ?? 1;
            if (colSpan > 1)
            {
                Grid.SetColumnSpan(fieldElement, colSpan);
                col += colSpan;
            }
            else
            {
                col++;
            }

            if (col >= section.Columns)
            {
                col = 0;
                row++;
            }

            grid.Children.Add(fieldElement);
            _fieldElements[field.Id] = fieldElement;
        }

        expander.Content = grid;
        return expander;
    }

    public FrameworkElement RenderField(FieldSchema field)
    {
        var container = new StackPanel
        {
            Margin = new Thickness(5),
            Tag = $"{FieldTagPrefix}{field.Id}"
        };

        // برچسب
        if (!string.IsNullOrEmpty(field.LabelFa))
        {
            var label = new TextBlock
            {
                Text = field.Required ? $"{field.LabelFa} *" : field.LabelFa,
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = field.Required ? FontWeights.SemiBold : FontWeights.Normal
            };
            container.Children.Add(label);
        }

        // کنترل اصلی بر اساس نوع
        var control = CreateControlByType(field);
        control.Tag = field.Id;
        control.IsEnabled = !field.Disabled && !field.ReadOnly;
        
        if (field.Hidden)
        {
            container.Visibility = Visibility.Collapsed;
        }

        container.Children.Add(control);

        // متن راهنما
        if (!string.IsNullOrEmpty(field.HelpText))
        {
            var helpText = new TextBlock
            {
                Text = field.HelpText,
                FontSize = 11,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 3, 0, 0)
            };
            container.Children.Add(helpText);
        }

        // محل نمایش خطا
        var errorBlock = new TextBlock
        {
            Tag = $"{ErrorTagPrefix}{field.Id}",
            Foreground = Brushes.Red,
            FontSize = 11,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 3, 0, 0)
        };
        container.Children.Add(errorBlock);

        return container;
    }

    private FrameworkElement CreateControlByType(FieldSchema field)
    {
        return field.Type.ToLower() switch
        {
            "text" => CreateTextBox(field),
            "textarea" => CreateTextArea(field),
            "integer" => CreateIntegerBox(field),
            "decimal" => CreateDecimalBox(field),
            "select" => CreateComboBox(field),
            "multiselect" => CreateMultiSelectBox(field),
            "boolean" => CreateCheckBox(field),
            "date" => CreateDatePicker(field),
            "datetime" => CreateDateTimePicker(field),
            "time" => CreateTimePicker(field),
            "rating" => CreateRatingControl(field),
            "color" => CreateColorPicker(field),
            "percentage" => CreatePercentageBox(field),
            _ => CreateTextBox(field)
        };
    }

    private TextBox CreateTextBox(FieldSchema field)
    {
        var textBox = new TextBox
        {
            MinWidth = 200,
            Padding = new Thickness(8, 6, 8, 6)
        };

        if (!string.IsNullOrEmpty(field.Placeholder))
        {
            textBox.Tag = field.Id;
            textBox.Text = field.Placeholder;
            textBox.Foreground = Brushes.Gray;
            
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == field.Placeholder)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = field.Placeholder;
                    textBox.Foreground = Brushes.Gray;
                }
            };
        }

        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            textBox.Text = field.DefaultValue;
            textBox.Foreground = Brushes.Black;
        }

        if (field.Validation?.MaxLength.HasValue == true)
        {
            textBox.MaxLength = field.Validation.MaxLength.Value;
        }

        return textBox;
    }

    private TextBox CreateTextArea(FieldSchema field)
    {
        var textBox = CreateTextBox(field);
        textBox.AcceptsReturn = true;
        textBox.TextWrapping = TextWrapping.Wrap;
        textBox.MinHeight = 80;
        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        return textBox;
    }

    private TextBox CreateIntegerBox(FieldSchema field)
    {
        var textBox = CreateTextBox(field);
        textBox.PreviewTextInput += (s, e) =>
        {
            e.Handled = !int.TryParse(e.Text, out _) && e.Text != "-";
        };
        return textBox;
    }

    private TextBox CreateDecimalBox(FieldSchema field)
    {
        var textBox = CreateTextBox(field);
        textBox.PreviewTextInput += (s, e) =>
        {
            var newText = textBox.Text + e.Text;
            e.Handled = !decimal.TryParse(newText, out _) && e.Text != "." && e.Text != "-";
        };
        return textBox;
    }

    private ComboBox CreateComboBox(FieldSchema field)
    {
        var comboBox = new ComboBox
        {
            MinWidth = 200,
            Padding = new Thickness(8, 6, 8, 6)
        };

        if (field.Options != null)
        {
            foreach (var option in field.Options)
            {
                comboBox.Items.Add(new ComboBoxItem
                {
                    Content = option.LabelFa,
                    Tag = option.Value
                });
            }
        }

        if (!string.IsNullOrEmpty(field.DefaultValue))
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Tag?.ToString() == field.DefaultValue)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        return comboBox;
    }

    private ListBox CreateMultiSelectBox(FieldSchema field)
    {
        var listBox = new ListBox
        {
            MinWidth = 200,
            MinHeight = 100,
            SelectionMode = SelectionMode.Multiple
        };

        if (field.Options != null)
        {
            foreach (var option in field.Options)
            {
                var checkBox = new CheckBox
                {
                    Content = option.LabelFa,
                    Tag = option.Value,
                    Margin = new Thickness(5)
                };
                listBox.Items.Add(checkBox);
            }
        }

        return listBox;
    }

    private CheckBox CreateCheckBox(FieldSchema field)
    {
        var checkBox = new CheckBox
        {
            Content = field.LabelFa,
            IsChecked = field.DefaultValue?.ToLower() == "true"
        };
        return checkBox;
    }

    private DatePicker CreateDatePicker(FieldSchema field)
    {
        var datePicker = new DatePicker
        {
            MinWidth = 200
        };

        if (!string.IsNullOrEmpty(field.DefaultValue) && DateTime.TryParse(field.DefaultValue, out var date))
        {
            datePicker.SelectedDate = date;
        }

        return datePicker;
    }

    private StackPanel CreateDateTimePicker(FieldSchema field)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var datePicker = new DatePicker
        {
            MinWidth = 150,
            Margin = new Thickness(0, 0, 10, 0),
            Tag = $"{field.Id}_date"
        };

        var timePicker = new TextBox
        {
            MinWidth = 80,
            Text = "00:00",
            Tag = $"{field.Id}_time"
        };

        panel.Children.Add(datePicker);
        panel.Children.Add(new TextBlock { Text = "ساعت:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 5, 0) });
        panel.Children.Add(timePicker);

        return panel;
    }

    private TextBox CreateTimePicker(FieldSchema field)
    {
        var textBox = new TextBox
        {
            MinWidth = 80,
            Text = field.DefaultValue ?? "00:00"
        };
        return textBox;
    }

    private StackPanel CreateRatingControl(FieldSchema field)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        for (int i = 1; i <= 5; i++)
        {
            var star = new RadioButton
            {
                Content = "★",
                FontSize = 20,
                Tag = i,
                GroupName = $"Rating_{field.Id}",
                Margin = new Thickness(2)
            };
            panel.Children.Add(star);
        }

        return panel;
    }

    private ComboBox CreateColorPicker(FieldSchema field)
    {
        var comboBox = new ComboBox
        {
            MinWidth = 150
        };

        var colors = new[] { "Red", "Green", "Blue", "Yellow", "Orange", "Purple", "Gray", "Black" };
        foreach (var color in colors)
        {
            var item = new ComboBoxItem
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        new Border
                        {
                            Width = 20,
                            Height = 20,
                            Background = (SolidColorBrush)new BrushConverter().ConvertFromString(color)!,
                            Margin = new Thickness(0, 0, 5, 0)
                        },
                        new TextBlock { Text = color }
                    }
                },
                Tag = color
            };
            comboBox.Items.Add(item);
        }

        return comboBox;
    }

    private StackPanel CreatePercentageBox(FieldSchema field)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var textBox = new TextBox
        {
            MinWidth = 80,
            Tag = field.Id
        };
        textBox.PreviewTextInput += (s, e) =>
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        };

        panel.Children.Add(textBox);
        panel.Children.Add(new TextBlock
        {
            Text = "%",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0)
        });

        return panel;
    }

    private StackPanel RenderActions(List<ActionSchema> actions)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 15, 0, 0)
        };

        foreach (var action in actions)
        {
            var button = new Button
            {
                Content = action.LabelFa,
                Tag = action.Id,
                Padding = new Thickness(20, 8, 20, 8),
                Margin = new Thickness(0, 0, 10, 0),
                MinWidth = 100
            };

            // استایل بر اساس نوع
            if (action.Style == "primary")
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
                button.Foreground = Brushes.White;
            }
            else if (action.Style == "danger")
            {
                button.Background = new SolidColorBrush(Color.FromRgb(220, 53, 69));
                button.Foreground = Brushes.White;
            }

            panel.Children.Add(button);
        }

        return panel;
    }

    public void BindData(FrameworkElement form, object data)
    {
        if (data == null) return;

        var properties = data.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
            var fieldId = prop.Name;
            var value = prop.GetValue(data);
            SetFieldValue(form, fieldId, value);
        }
    }

    public Dictionary<string, object?> ExtractData(FrameworkElement form)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kvp in _fieldElements)
        {
            var fieldId = kvp.Key;
            var value = GetFieldValue(form, fieldId);
            result[fieldId] = value;
        }

        return result;
    }

    public Result<bool> ValidateForm(FrameworkElement form)
    {
        ClearErrors(form);
        var errors = new List<string>();

        var formId = form.Tag?.ToString() ?? "";
        var fields = _schemaEngine.GetFields(formId);

        foreach (var field in fields)
        {
            var value = GetFieldValue(form, field.Id);
            
            // بررسی اجباری بودن
            if (field.Required && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                ShowFieldError(form, field.Id, $"{field.LabelFa} الزامی است");
                errors.Add($"{field.LabelFa} الزامی است");
                continue;
            }

            // بررسی Validation
            if (field.Validation != null && value != null)
            {
                var validationError = ValidateFieldValue(field, value);
                if (!string.IsNullOrEmpty(validationError))
                {
                    ShowFieldError(form, field.Id, validationError);
                    errors.Add(validationError);
                }
            }
        }

        if (errors.Any())
        {
            return Result.Failure<bool>(Error.Validation(string.Join("\n", errors)));
        }

        return Result.Success(true);
    }

    private string? ValidateFieldValue(FieldSchema field, object value)
    {
        var validation = field.Validation;
        if (validation == null) return null;

        var strValue = value.ToString() ?? "";

        // حداقل طول
        if (validation.MinLength.HasValue && strValue.Length < validation.MinLength.Value)
        {
            return validation.MessageFa ?? $"{field.LabelFa} باید حداقل {validation.MinLength} کاراکتر باشد";
        }

        // حداکثر طول
        if (validation.MaxLength.HasValue && strValue.Length > validation.MaxLength.Value)
        {
            return validation.MessageFa ?? $"{field.LabelFa} باید حداکثر {validation.MaxLength} کاراکتر باشد";
        }

        // حداقل مقدار عددی
        if (validation.Min.HasValue && decimal.TryParse(strValue, out var numValue))
        {
            if (numValue < validation.Min.Value)
            {
                return validation.MessageFa ?? $"{field.LabelFa} باید حداقل {validation.Min} باشد";
            }
        }

        // حداکثر مقدار عددی
        if (validation.Max.HasValue && decimal.TryParse(strValue, out numValue))
        {
            if (numValue > validation.Max.Value)
            {
                return validation.MessageFa ?? $"{field.LabelFa} باید حداکثر {validation.Max} باشد";
            }
        }

        // الگوی Regex
        if (!string.IsNullOrEmpty(validation.Pattern))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(strValue, validation.Pattern))
            {
                return validation.MessageFa ?? $"{field.LabelFa} فرمت صحیح ندارد";
            }
        }

        return null;
    }

    public object? GetFieldValue(FrameworkElement form, string fieldId)
    {
        var element = FindFieldElement(form, fieldId);
        if (element == null) return null;

        return element switch
        {
            TextBox tb => string.IsNullOrEmpty(tb.Text) ? null : tb.Text,
            ComboBox cb => (cb.SelectedItem as ComboBoxItem)?.Tag,
            CheckBox chk => chk.IsChecked,
            DatePicker dp => dp.SelectedDate,
            ListBox lb => GetMultiSelectValues(lb),
            StackPanel sp => GetStackPanelValue(sp, fieldId),
            _ => null
        };
    }

    private List<string> GetMultiSelectValues(ListBox listBox)
    {
        var values = new List<string>();
        foreach (var item in listBox.Items)
        {
            if (item is CheckBox chk && chk.IsChecked == true)
            {
                values.Add(chk.Tag?.ToString() ?? "");
            }
        }
        return values;
    }

    private object? GetStackPanelValue(StackPanel panel, string fieldId)
    {
        // برای Rating
        foreach (var child in panel.Children)
        {
            if (child is RadioButton rb && rb.IsChecked == true)
            {
                return rb.Tag;
            }
        }

        // برای DateTime
        DatePicker? datePicker = null;
        TextBox? timeBox = null;

        foreach (var child in panel.Children)
        {
            if (child is DatePicker dp) datePicker = dp;
            if (child is TextBox tb && tb.Tag?.ToString()?.EndsWith("_time") == true) timeBox = tb;
        }

        if (datePicker?.SelectedDate != null)
        {
            var date = datePicker.SelectedDate.Value;
            if (timeBox != null && TimeSpan.TryParse(timeBox.Text, out var time))
            {
                return date.Add(time);
            }
            return date;
        }

        return null;
    }

    public void SetFieldValue(FrameworkElement form, string fieldId, object? value)
    {
        var element = FindFieldElement(form, fieldId);
        if (element == null) return;

        switch (element)
        {
            case TextBox tb:
                tb.Text = value?.ToString() ?? "";
                tb.Foreground = Brushes.Black;
                break;
            case ComboBox cb:
                foreach (ComboBoxItem item in cb.Items)
                {
                    if (item.Tag?.ToString() == value?.ToString())
                    {
                        cb.SelectedItem = item;
                        break;
                    }
                }
                break;
            case CheckBox chk:
                chk.IsChecked = value is bool b && b;
                break;
            case DatePicker dp:
                dp.SelectedDate = value as DateTime?;
                break;
        }
    }

    public void SetFieldVisibility(FrameworkElement form, string fieldId, bool visible)
    {
        var container = FindFieldContainer(form, fieldId);
        if (container != null)
        {
            container.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public void SetFieldEnabled(FrameworkElement form, string fieldId, bool enabled)
    {
        var element = FindFieldElement(form, fieldId);
        if (element != null)
        {
            element.IsEnabled = enabled;
        }
    }

    public void ShowFieldError(FrameworkElement form, string fieldId, string message)
    {
        var errorBlock = FindErrorBlock(form, fieldId);
        if (errorBlock != null)
        {
            errorBlock.Text = message;
            errorBlock.Visibility = Visibility.Visible;
        }
    }

    public void ClearErrors(FrameworkElement form)
    {
        ClearErrorsRecursive(form);
    }

    private void ClearErrorsRecursive(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is TextBlock tb && tb.Tag?.ToString()?.StartsWith(ErrorTagPrefix) == true)
            {
                tb.Text = "";
                tb.Visibility = Visibility.Collapsed;
            }

            ClearErrorsRecursive(child);
        }
    }

    private FrameworkElement? FindFieldElement(FrameworkElement parent, string fieldId)
    {
        return FindElementByTag(parent, fieldId);
    }

    private FrameworkElement? FindFieldContainer(FrameworkElement parent, string fieldId)
    {
        return FindElementByTag(parent, $"{FieldTagPrefix}{fieldId}");
    }

    private TextBlock? FindErrorBlock(FrameworkElement parent, string fieldId)
    {
        return FindElementByTag(parent, $"{ErrorTagPrefix}{fieldId}") as TextBlock;
    }

    private FrameworkElement? FindElementByTag(DependencyObject parent, string tag)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is FrameworkElement fe && fe.Tag?.ToString() == tag)
            {
                return fe;
            }

            var result = FindElementByTag(child, tag);
            if (result != null) return result;
        }

        return null;
    }

    private FrameworkElement CreateErrorPanel(string message)
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(255, 200, 200)),
            Padding = new Thickness(20),
            Child = new TextBlock
            {
                Text = message,
                Foreground = Brushes.DarkRed,
                FontSize = 14
            }
        };
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/UIRendererEngine.cs
// =============================================================================