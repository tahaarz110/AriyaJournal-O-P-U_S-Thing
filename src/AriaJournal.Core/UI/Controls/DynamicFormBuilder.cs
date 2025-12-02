// =============================================================================
// فایل: src/AriaJournal.Core/UI/Controls/DynamicFormBuilder.cs
// توضیح: سازنده فرم‌های داینامیک از Schema - نسخه اصلاح‌شده کامل
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AriaJournal.Core.Domain.Schemas;

namespace AriaJournal.Core.UI.Controls;

/// <summary>
/// سازنده فرم‌های داینامیک از Schema
/// </summary>
public class DynamicFormBuilder
{
    private readonly Dictionary<string, FrameworkElement> _fieldControls = new();
    private readonly Dictionary<string, object> _fieldValues = new();

    #region Resource Helper - دسترسی امن به Resource ها

    /// <summary>
    /// دریافت امن Resource از Application
    /// </summary>
    private static object? GetResource(string key)
    {
        try
        {
            // استفاده از System.Windows.Application به جای AriaJournal.Core.Application
            var app = System.Windows.Application.Current;
            if (app?.Resources != null && app.Resources.Contains(key))
            {
                return app.Resources[key];
            }
        }
        catch
        {
            // در صورت خطا، null برمی‌گردانیم
        }
        return null;
    }

    /// <summary>
    /// دریافت Brush از Resource یا مقدار پیش‌فرض
    /// </summary>
    private static Brush GetBrush(string key, Brush defaultBrush)
    {
        var resource = GetResource(key);
        return resource as Brush ?? defaultBrush;
    }

    /// <summary>
    /// دریافت Style از Resource
    /// </summary>
    private static Style? GetStyle(string key)
    {
        return GetResource(key) as Style;
    }

    #endregion

    /// <summary>
    /// ساخت فرم از Schema
    /// </summary>
    public FrameworkElement BuildForm(FormSchema schema)
    {
        if (schema == null)
            return CreateEmptyMessage("Schema نامعتبر است");

        _fieldControls.Clear();
        _fieldValues.Clear();

        var mainPanel = new StackPanel
        {
            Margin = new Thickness(0)
        };

        // عنوان فرم
        if (!string.IsNullOrWhiteSpace(schema.TitleFa))
        {
            mainPanel.Children.Add(new TextBlock
            {
                Text = schema.TitleFa,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = GetBrush("ForegroundBrush", Brushes.Black)
            });
        }

        // بخش‌ها
        if (schema.Sections != null)
        {
            foreach (var section in schema.Sections)
            {
                var sectionControl = BuildSection(section);
                mainPanel.Children.Add(sectionControl);
            }
        }

        // دکمه‌ها
        if (schema.Actions != null && schema.Actions.Any())
        {
            var actionsPanel = BuildActionsPanel(schema.Actions);
            mainPanel.Children.Add(actionsPanel);
        }

        return new ScrollViewer
        {
            Content = mainPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
    }

    /// <summary>
    /// ساخت یک بخش
    /// </summary>
    private FrameworkElement BuildSection(SectionSchema section)
    {
        var expander = new Expander
        {
            IsExpanded = !section.Collapsed,
            Margin = new Thickness(0, 0, 0, 15)
        };

        // هدر بخش
        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        
        if (!string.IsNullOrWhiteSpace(section.Icon))
        {
            headerPanel.Children.Add(new TextBlock
            {
                Text = section.Icon,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        headerPanel.Children.Add(new TextBlock
        {
            Text = section.TitleFa,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
            Foreground = GetBrush("ForegroundBrush", Brushes.Black)
        });

        expander.Header = headerPanel;

        // محتوای بخش
        var contentBorder = new Border
        {
            Background = GetBrush("CardBackgroundBrush", Brushes.White),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(15),
            Margin = new Thickness(0, 10, 0, 0)
        };

        var fieldsPanel = new Grid();
        
        // تنظیم ستون‌ها
        var columns = Math.Max(1, section.Columns);
        for (int i = 0; i < columns; i++)
        {
            fieldsPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        // افزودن فیلدها
        if (section.Fields != null)
        {
            var visibleFields = section.Fields.Where(f => f.Visible).ToList();
            var row = 0;
            var col = 0;

            foreach (var field in visibleFields)
            {
                // اضافه کردن ردیف جدید در صورت نیاز
                if (col == 0)
                {
                    fieldsPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                var fieldControl = BuildField(field);
                Grid.SetRow(fieldControl, row);
                Grid.SetColumn(fieldControl, col);

                // اگر ColSpan یا ColumnSpan دارد
                var colSpan = field.ColSpan ?? field.ColumnSpan ?? 1;
                if (colSpan > 1)
                {
                    Grid.SetColumnSpan(fieldControl, Math.Min(colSpan, columns - col));
                }

                fieldsPanel.Children.Add(fieldControl);

                col++;
                if (col >= columns)
                {
                    col = 0;
                    row++;
                }
            }
        }

        contentBorder.Child = fieldsPanel;
        expander.Content = contentBorder;

        return expander;
    }

    /// <summary>
    /// ساخت یک فیلد
    /// </summary>
    private FrameworkElement BuildField(FieldSchema field)
    {
        var container = new StackPanel
        {
            Margin = new Thickness(0, 0, 10, 15)
        };

        // بررسی پنهان بودن
        if (field.Hidden)
        {
            container.Visibility = Visibility.Collapsed;
        }

        // برچسب
        var labelPanel = new StackPanel { Orientation = Orientation.Horizontal };
        
        labelPanel.Children.Add(new TextBlock
        {
            Text = field.LabelFa,
            Foreground = GetBrush("ForegroundBrush", Brushes.Black),
            Margin = new Thickness(0, 0, 0, 5)
        });

        if (field.Required)
        {
            labelPanel.Children.Add(new TextBlock
            {
                Text = " *",
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold
            });
        }

        container.Children.Add(labelPanel);

        // کنترل ورودی
        FrameworkElement inputControl = field.Type.ToLower() switch
        {
            "text" => CreateTextBox(field),
            "number" or "decimal" or "integer" => CreateNumericBox(field),
            "select" => CreateComboBox(field),
            "multiselect" => CreateMultiSelectBox(field),
            "date" => CreateDatePicker(field),
            "datetime" => CreateDateTimePicker(field),
            "time" => CreateTimePicker(field),
            "boolean" => CreateCheckBox(field),
            "textarea" => CreateTextArea(field),
            "rating" => CreateRatingControl(field),
            "color" => CreateColorPicker(field),
            "file" or "image" => CreateFilePicker(field),
            _ => CreateTextBox(field)
        };

        // بررسی غیرفعال بودن
        if (field.Disabled && inputControl is Control control)
        {
            control.IsEnabled = false;
        }

        _fieldControls[field.Id] = inputControl;
        container.Children.Add(inputControl);

        // متن راهنما
        if (!string.IsNullOrWhiteSpace(field.HelpText))
        {
            container.Children.Add(new TextBlock
            {
                Text = field.HelpText,
                FontSize = 11,
                Foreground = GetBrush("SecondaryForegroundBrush", Brushes.Gray),
                Margin = new Thickness(0, 3, 0, 0)
            });
        }

        return container;
    }

    #region Field Controls

    private TextBox CreateTextBox(FieldSchema field)
    {
        var textBox = new TextBox
        {
            IsReadOnly = field.ReadOnly
        };

        // اعمال استایل
        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            textBox.Style = style;
        }

        if (!string.IsNullOrWhiteSpace(field.Placeholder))
        {
            textBox.Tag = field.Placeholder;
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
        {
            textBox.Text = field.DefaultValue;
        }

        if (field.Width.HasValue)
        {
            textBox.Width = field.Width.Value;
        }

        if (field.Validation?.MaxLength.HasValue == true)
        {
            textBox.MaxLength = field.Validation.MaxLength.Value;
        }

        return textBox;
    }

    private TextBox CreateNumericBox(FieldSchema field)
    {
        var textBox = new TextBox
        {
            IsReadOnly = field.ReadOnly,
            TextAlignment = TextAlignment.Left
        };

        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            textBox.Style = style;
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
        {
            textBox.Text = field.DefaultValue;
        }

        // اعتبارسنجی عددی
        textBox.PreviewTextInput += (s, e) =>
        {
            var text = textBox.Text + e.Text;
            e.Handled = !IsValidNumber(text, field.Type);
        };

        return textBox;
    }

    private ComboBox CreateComboBox(FieldSchema field)
    {
        var comboBox = new ComboBox
        {
            IsEnabled = !field.ReadOnly
        };

        var style = GetStyle("ComboBoxStyle");
        if (style != null)
        {
            comboBox.Style = style;
        }

        if (field.Options != null)
        {
            foreach (var option in field.Options)
            {
                comboBox.Items.Add(new ComboBoxItem
                {
                    Content = option.LabelFa,
                    Tag = option.Value,
                    IsEnabled = !option.Disabled
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
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
            SelectionMode = SelectionMode.Multiple,
            MaxHeight = 150,
            IsEnabled = !field.ReadOnly
        };

        if (field.Options != null)
        {
            foreach (var option in field.Options)
            {
                var checkBox = new CheckBox
                {
                    Content = option.LabelFa,
                    Tag = option.Value,
                    IsEnabled = !option.Disabled,
                    Margin = new Thickness(5)
                };
                listBox.Items.Add(checkBox);
            }
        }

        return listBox;
    }

    private DatePicker CreateDatePicker(FieldSchema field)
    {
        var datePicker = new DatePicker
        {
            IsEnabled = !field.ReadOnly
        };

        if (!string.IsNullOrWhiteSpace(field.DefaultValue) &&
            DateTime.TryParse(field.DefaultValue, out var date))
        {
            datePicker.SelectedDate = date;
        }

        return datePicker;
    }

    private StackPanel CreateDateTimePicker(FieldSchema field)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var datePicker = new DatePicker
        {
            IsEnabled = !field.ReadOnly,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var timePicker = new TextBox
        {
            Width = 80,
            IsReadOnly = field.ReadOnly
        };

        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            timePicker.Style = style;
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue) &&
            DateTime.TryParse(field.DefaultValue, out var dateTime))
        {
            datePicker.SelectedDate = dateTime.Date;
            timePicker.Text = dateTime.ToString("HH:mm");
        }

        panel.Children.Add(datePicker);
        panel.Children.Add(new TextBlock
        {
            Text = "ساعت:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0),
            Foreground = GetBrush("ForegroundBrush", Brushes.Black)
        });
        panel.Children.Add(timePicker);

        return panel;
    }

    private TextBox CreateTimePicker(FieldSchema field)
    {
        var textBox = new TextBox
        {
            Width = 80,
            IsReadOnly = field.ReadOnly
        };

        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            textBox.Style = style;
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
        {
            textBox.Text = field.DefaultValue;
        }

        return textBox;
    }

    private CheckBox CreateCheckBox(FieldSchema field)
    {
        var checkBox = new CheckBox
        {
            Content = field.LabelFa,
            IsEnabled = !field.ReadOnly,
            Foreground = GetBrush("ForegroundBrush", Brushes.Black)
        };

        if (!string.IsNullOrWhiteSpace(field.DefaultValue) &&
            bool.TryParse(field.DefaultValue, out var isChecked))
        {
            checkBox.IsChecked = isChecked;
        }

        return checkBox;
    }

    private TextBox CreateTextArea(FieldSchema field)
    {
        var textBox = new TextBox
        {
            IsReadOnly = field.ReadOnly,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80,
            MaxHeight = 200,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            textBox.Style = style;
        }

        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
        {
            textBox.Text = field.DefaultValue;
        }

        if (field.Validation?.MaxLength.HasValue == true)
        {
            textBox.MaxLength = field.Validation.MaxLength.Value;
        }

        return textBox;
    }

    private StackPanel CreateRatingControl(FieldSchema field)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var defaultRating = 0;
        if (!string.IsNullOrWhiteSpace(field.DefaultValue) &&
            int.TryParse(field.DefaultValue, out var rating))
        {
            defaultRating = rating;
        }

        for (int i = 1; i <= 5; i++)
        {
            var star = new Button
            {
                Content = i <= defaultRating ? "★" : "☆",
                Tag = i,
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FontSize = 20,
                Foreground = i <= defaultRating ? Brushes.Gold : GetBrush("SecondaryForegroundBrush", Brushes.Gray),
                Cursor = System.Windows.Input.Cursors.Hand,
                IsEnabled = !field.ReadOnly
            };

            var index = i;
            star.Click += (s, e) =>
            {
                foreach (Button btn in panel.Children)
                {
                    var btnIndex = (int)btn.Tag;
                    btn.Content = btnIndex <= index ? "★" : "☆";
                    btn.Foreground = btnIndex <= index ? Brushes.Gold : GetBrush("SecondaryForegroundBrush", Brushes.Gray);
                }
                _fieldValues[field.Id] = index;
            };

            panel.Children.Add(star);
        }

        return panel;
    }

    private Button CreateColorPicker(FieldSchema field)
    {
        var defaultColor = Colors.Gray;
        if (!string.IsNullOrWhiteSpace(field.DefaultValue))
        {
            try
            {
                defaultColor = (Color)ColorConverter.ConvertFromString(field.DefaultValue);
            }
            catch { }
        }

        var button = new Button
        {
            Width = 100,
            Height = 30,
            Background = new SolidColorBrush(defaultColor),
            Content = field.DefaultValue ?? "#808080",
            IsEnabled = !field.ReadOnly
        };

        button.Click += (s, e) =>
        {
            // در اینجا می‌توان ColorDialog باز کرد
        };

        return button;
    }

    private StackPanel CreateFilePicker(FieldSchema field)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var textBox = new TextBox
        {
            IsReadOnly = true,
            Width = 200
        };

        var style = GetStyle("TextBoxStyle");
        if (style != null)
        {
            textBox.Style = style;
        }

        var browseButton = new Button
        {
            Content = "انتخاب...",
            Margin = new Thickness(10, 0, 0, 0),
            IsEnabled = !field.ReadOnly
        };

        var buttonStyle = GetStyle("SecondaryButtonStyle");
        if (buttonStyle != null)
        {
            browseButton.Style = buttonStyle;
        }

        browseButton.Click += (s, e) =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (field.Type == "image")
            {
                dialog.Filter = "تصاویر|*.jpg;*.jpeg;*.png;*.gif;*.bmp|همه فایل‌ها|*.*";
            }

            if (dialog.ShowDialog() == true)
            {
                textBox.Text = dialog.FileName;
                _fieldValues[field.Id] = dialog.FileName;
            }
        };

        panel.Children.Add(textBox);
        panel.Children.Add(browseButton);

        return panel;
    }

    #endregion

    #region Actions Panel

    private FrameworkElement BuildActionsPanel(List<ActionSchema> actions)
    {
        var border = new Border
        {
            Margin = new Thickness(0, 20, 0, 0),
            Padding = new Thickness(0, 15, 0, 0),
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = GetBrush("BorderBrush", Brushes.LightGray)
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        foreach (var action in actions.OrderBy(a => a.Order))
        {
            var button = new Button
            {
                Content = BuildActionContent(action),
                Tag = action.Id,
                Margin = new Thickness(0, 0, 10, 0),
                MinWidth = 100
            };

            // استایل بر اساس نوع
            var styleName = action.Style?.ToLower() switch
            {
                "primary" => "PrimaryButtonStyle",
                "danger" => "DangerButtonStyle",
                "success" => "SuccessButtonStyle",
                _ => "SecondaryButtonStyle"
            };

            var buttonStyle = GetStyle(styleName);
            if (buttonStyle != null)
            {
                button.Style = buttonStyle;
            }

            panel.Children.Add(button);
        }

        border.Child = panel;
        return border;
    }

    private object BuildActionContent(ActionSchema action)
    {
        if (string.IsNullOrWhiteSpace(action.Icon))
        {
            return action.LabelFa;
        }

        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        panel.Children.Add(new TextBlock
        {
            Text = action.Icon,
            Margin = new Thickness(0, 0, 5, 0)
        });
        panel.Children.Add(new TextBlock { Text = action.LabelFa });

        return panel;
    }

    #endregion

    #region Data Methods

    /// <summary>
    /// دریافت مقادیر فرم
    /// </summary>
    public Dictionary<string, object?> GetFormValues()
    {
        var values = new Dictionary<string, object?>();

        foreach (var kvp in _fieldControls)
        {
            var value = GetControlValue(kvp.Value);
            values[kvp.Key] = value;
        }

        // اضافه کردن مقادیر خاص (مثل rating)
        foreach (var kvp in _fieldValues)
        {
            values[kvp.Key] = kvp.Value;
        }

        return values;
    }

    /// <summary>
    /// تنظیم مقادیر فرم
    /// </summary>
    public void SetFormValues(Dictionary<string, object?> values)
    {
        foreach (var kvp in values)
        {
            if (_fieldControls.TryGetValue(kvp.Key, out var control))
            {
                SetControlValue(control, kvp.Value);
            }
        }
    }

    /// <summary>
    /// اعتبارسنجی فرم
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateForm(FormSchema schema)
    {
        var errors = new List<string>();
        var values = GetFormValues();

        if (schema.Sections == null) return (true, errors);

        foreach (var section in schema.Sections)
        {
            if (section.Fields == null) continue;

            foreach (var field in section.Fields.Where(f => f.Required))
            {
                if (!values.TryGetValue(field.Id, out var value) || IsEmpty(value))
                {
                    errors.Add($"فیلد «{field.LabelFa}» الزامی است");
                }
            }

            // اعتبارسنجی‌های دیگر
            foreach (var field in section.Fields.Where(f => f.Validation != null))
            {
                if (!values.TryGetValue(field.Id, out var value) || value == null)
                    continue;

                var validation = field.Validation!;
                var strValue = value.ToString() ?? "";

                if (validation.MinLength.HasValue && strValue.Length < validation.MinLength.Value)
                {
                    errors.Add($"فیلد «{field.LabelFa}» باید حداقل {validation.MinLength.Value} کاراکتر باشد");
                }

                if (validation.MaxLength.HasValue && strValue.Length > validation.MaxLength.Value)
                {
                    errors.Add($"فیلد «{field.LabelFa}» باید حداکثر {validation.MaxLength.Value} کاراکتر باشد");
                }

                if (decimal.TryParse(strValue, out var numValue))
                {
                    if (validation.Min.HasValue && numValue < validation.Min.Value)
                    {
                        errors.Add($"فیلد «{field.LabelFa}» باید حداقل {validation.Min.Value} باشد");
                    }

                    if (validation.Max.HasValue && numValue > validation.Max.Value)
                    {
                        errors.Add($"فیلد «{field.LabelFa}» باید حداکثر {validation.Max.Value} باشد");
                    }
                }
            }
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// پاک کردن فرم
    /// </summary>
    public void ClearForm()
    {
        foreach (var control in _fieldControls.Values)
        {
            ClearControl(control);
        }
        _fieldValues.Clear();
    }

    #endregion

    #region Helper Methods

    private object? GetControlValue(FrameworkElement control)
    {
        return control switch
        {
            TextBox textBox => textBox.Text,
            ComboBox comboBox => (comboBox.SelectedItem as ComboBoxItem)?.Tag,
            CheckBox checkBox => checkBox.IsChecked,
            DatePicker datePicker => datePicker.SelectedDate,
            ListBox listBox => GetMultiSelectValues(listBox),
            StackPanel panel => GetPanelValue(panel),
            _ => null
        };
    }

    private void SetControlValue(FrameworkElement control, object? value)
    {
        if (value == null) return;

        switch (control)
        {
            case TextBox textBox:
                textBox.Text = value.ToString();
                break;
            case ComboBox comboBox:
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if (item.Tag?.ToString() == value.ToString())
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
                break;
            case CheckBox checkBox:
                if (bool.TryParse(value.ToString(), out var isChecked))
                    checkBox.IsChecked = isChecked;
                break;
            case DatePicker datePicker:
                if (DateTime.TryParse(value.ToString(), out var date))
                    datePicker.SelectedDate = date;
                break;
        }
    }

    private void ClearControl(FrameworkElement control)
    {
        switch (control)
        {
            case TextBox textBox:
                textBox.Clear();
                break;
            case ComboBox comboBox:
                comboBox.SelectedIndex = -1;
                break;
            case CheckBox checkBox:
                checkBox.IsChecked = false;
                break;
            case DatePicker datePicker:
                datePicker.SelectedDate = null;
                break;
            case ListBox listBox:
                foreach (var item in listBox.Items)
                {
                    if (item is CheckBox cb) cb.IsChecked = false;
                }
                break;
        }
    }

    private List<string> GetMultiSelectValues(ListBox listBox)
    {
        var values = new List<string>();
        foreach (var item in listBox.Items)
        {
            if (item is CheckBox { IsChecked: true } cb && cb.Tag != null)
            {
                values.Add(cb.Tag.ToString()!);
            }
        }
        return values;
    }

    private object? GetPanelValue(StackPanel panel)
    {
        // برای DateTimePicker
        var datePicker = panel.Children.OfType<DatePicker>().FirstOrDefault();
        var timeBox = panel.Children.OfType<TextBox>().FirstOrDefault();

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

    private bool IsEmpty(object? value)
    {
        if (value == null) return true;
        if (value is string str) return string.IsNullOrWhiteSpace(str);
        if (value is IEnumerable<object> list) return !list.Any();
        return false;
    }

    private bool IsValidNumber(string text, string type)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;

        return type.ToLower() switch
        {
            "integer" => int.TryParse(text, out _),
            "decimal" or "number" => decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out _),
            _ => true
        };
    }

    private FrameworkElement CreateEmptyMessage(string message)
    {
        return new TextBlock
        {
            Text = message,
            Foreground = Brushes.Gray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20)
        };
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/Controls/DynamicFormBuilder.cs
// =============================================================================