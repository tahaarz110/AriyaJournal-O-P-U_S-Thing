// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/ThemeEngine.cs
// شماره فایل: 19
// توضیح: موتور مدیریت تم‌ها (Meta-driven)
// =============================================================================

using System.IO;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Media;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Interfaces.Engines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// اینترفیس موتور تم
/// </summary>
public interface IThemeEngine
{
    /// <summary>
    /// مقداردهی اولیه
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// دریافت لیست تم‌ها
    /// </summary>
    List<ThemeInfo> GetAvailableThemes();

    /// <summary>
    /// اعمال تم
    /// </summary>
    Result<bool> ApplyTheme(string themeId);

    /// <summary>
    /// دریافت تم فعلی
    /// </summary>
    string GetCurrentTheme();

    /// <summary>
    /// ثبت تم جدید
    /// </summary>
    Result<bool> RegisterTheme(ThemeDefinition theme);

    /// <summary>
    /// ذخیره تم سفارشی
    /// </summary>
    Task<Result<bool>> SaveCustomThemeAsync(ThemeDefinition theme);

    /// <summary>
    /// دریافت رنگ
    /// </summary>
    Color GetColor(string colorKey);

    /// <summary>
    /// دریافت Brush
    /// </summary>
    Brush GetBrush(string brushKey);

    /// <summary>
    /// تنظیم فونت
    /// </summary>
    void SetFont(string fontFamily);

    /// <summary>
    /// دریافت فونت فعلی
    /// </summary>
    string GetCurrentFont();

    /// <summary>
    /// رویداد تغییر تم
    /// </summary>
    event EventHandler<string>? OnThemeChanged;
}

/// <summary>
/// اطلاعات تم
/// </summary>
public class ThemeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameFa { get; set; } = string.Empty;
    public bool IsDark { get; set; }
    public bool IsBuiltIn { get; set; }
    public string? PreviewColor { get; set; }
}

/// <summary>
/// تعریف کامل تم
/// </summary>
public class ThemeDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameFa { get; set; } = string.Empty;
    public bool IsDark { get; set; }
    public Dictionary<string, string> Colors { get; set; } = new();
    public Dictionary<string, string> Fonts { get; set; } = new();
    public Dictionary<string, double> Sizes { get; set; } = new();
}

/// <summary>
/// پیاده‌سازی موتور تم
/// </summary>
public class ThemeEngine : IThemeEngine
{
    private readonly ConcurrentDictionary<string, ThemeDefinition> _themes;
    private readonly IEventBusEngine _eventBus;
    private string _currentThemeId = "Dark";
    private string _currentFont = "IRANSans";
    private readonly string _themesPath;

    public event EventHandler<string>? OnThemeChanged;

    public ThemeEngine(IEventBusEngine eventBus)
    {
        _eventBus = eventBus;
        _themes = new ConcurrentDictionary<string, ThemeDefinition>();

        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _themesPath = Path.Combine(appPath, "themes");
    }

    public async Task InitializeAsync()
    {
        // ثبت تم‌های پیش‌فرض
        RegisterBuiltInThemes();

        // بارگذاری تم‌های سفارشی
        await LoadCustomThemesAsync();

        // اعمال تم پیش‌فرض
        ApplyTheme(_currentThemeId);
    }

    #region Built-in Themes

    private void RegisterBuiltInThemes()
    {
        // تم تیره
        var darkTheme = new ThemeDefinition
        {
            Id = "Dark",
            Name = "Dark",
            NameFa = "تیره",
            IsDark = true,
            Colors = new Dictionary<string, string>
            {
                ["Background"] = "#1E1E1E",
                ["CardBackground"] = "#252526",
                ["SidebarBackground"] = "#2D2D30",
                ["Foreground"] = "#FFFFFF",
                ["SecondaryForeground"] = "#A0A0A0",
                ["Primary"] = "#007ACC",
                ["PrimaryLight"] = "#3399DD",
                ["PrimaryDark"] = "#005A9E",
                ["Accent"] = "#FF6B35",
                ["Success"] = "#4CAF50",
                ["Warning"] = "#FFC107",
                ["Danger"] = "#F44336",
                ["Info"] = "#2196F3",
                ["Border"] = "#3C3C3C",
                ["BorderLight"] = "#4A4A4A",
                ["Hover"] = "#3A3A3A",
                ["Selected"] = "#094771",
                ["InputBackground"] = "#3C3C3C",
                ["InputForeground"] = "#FFFFFF",
                ["MenuHover"] = "#3A3A3A",
                ["MenuActive"] = "#094771",
                ["WidgetHeader"] = "#2D2D30",
                ["WidgetFooter"] = "#252526"
            }
        };

        // تم روشن
        var lightTheme = new ThemeDefinition
        {
            Id = "Light",
            Name = "Light",
            NameFa = "روشن",
            IsDark = false,
            Colors = new Dictionary<string, string>
            {
                ["Background"] = "#F5F5F5",
                ["CardBackground"] = "#FFFFFF",
                ["SidebarBackground"] = "#EBEBEB",
                ["Foreground"] = "#212121",
                ["SecondaryForeground"] = "#757575",
                ["Primary"] = "#1976D2",
                ["PrimaryLight"] = "#42A5F5",
                ["PrimaryDark"] = "#1565C0",
                ["Accent"] = "#FF5722",
                ["Success"] = "#4CAF50",
                ["Warning"] = "#FF9800",
                ["Danger"] = "#F44336",
                ["Info"] = "#2196F3",
                ["Border"] = "#E0E0E0",
                ["BorderLight"] = "#EEEEEE",
                ["Hover"] = "#EEEEEE",
                ["Selected"] = "#E3F2FD",
                ["InputBackground"] = "#FFFFFF",
                ["InputForeground"] = "#212121",
                ["MenuHover"] = "#E0E0E0",
                ["MenuActive"] = "#BBDEFB",
                ["WidgetHeader"] = "#FAFAFA",
                ["WidgetFooter"] = "#F5F5F5"
            }
        };

        // تم آبی
        var blueTheme = new ThemeDefinition
        {
            Id = "Blue",
            Name = "Blue",
            NameFa = "آبی",
            IsDark = true,
            Colors = new Dictionary<string, string>
            {
                ["Background"] = "#0D1B2A",
                ["CardBackground"] = "#1B263B",
                ["SidebarBackground"] = "#1B263B",
                ["Foreground"] = "#E0E1DD",
                ["SecondaryForeground"] = "#778DA9",
                ["Primary"] = "#415A77",
                ["PrimaryLight"] = "#778DA9",
                ["PrimaryDark"] = "#1B263B",
                ["Accent"] = "#E0E1DD",
                ["Success"] = "#4CAF50",
                ["Warning"] = "#FFC107",
                ["Danger"] = "#F44336",
                ["Info"] = "#2196F3",
                ["Border"] = "#415A77",
                ["BorderLight"] = "#778DA9",
                ["Hover"] = "#2D3E50",
                ["Selected"] = "#415A77",
                ["InputBackground"] = "#1B263B",
                ["InputForeground"] = "#E0E1DD",
                ["MenuHover"] = "#2D3E50",
                ["MenuActive"] = "#415A77",
                ["WidgetHeader"] = "#1B263B",
                ["WidgetFooter"] = "#0D1B2A"
            }
        };

        // تم سبز
        var greenTheme = new ThemeDefinition
        {
            Id = "Green",
            Name = "Green",
            NameFa = "سبز",
            IsDark = true,
            Colors = new Dictionary<string, string>
            {
                ["Background"] = "#1A1A2E",
                ["CardBackground"] = "#16213E",
                ["SidebarBackground"] = "#16213E",
                ["Foreground"] = "#EAEAEA",
                ["SecondaryForeground"] = "#94A3B8",
                ["Primary"] = "#0F3460",
                ["PrimaryLight"] = "#1A5276",
                ["PrimaryDark"] = "#0A2647",
                ["Accent"] = "#E94560",
                ["Success"] = "#4CAF50",
                ["Warning"] = "#FFC107",
                ["Danger"] = "#E94560",
                ["Info"] = "#2196F3",
                ["Border"] = "#0F3460",
                ["BorderLight"] = "#1A5276",
                ["Hover"] = "#1F4068",
                ["Selected"] = "#0F3460",
                ["InputBackground"] = "#16213E",
                ["InputForeground"] = "#EAEAEA",
                ["MenuHover"] = "#1F4068",
                ["MenuActive"] = "#0F3460",
                ["WidgetHeader"] = "#16213E",
                ["WidgetFooter"] = "#1A1A2E"
            }
        };

        _themes.TryAdd(darkTheme.Id, darkTheme);
        _themes.TryAdd(lightTheme.Id, lightTheme);
        _themes.TryAdd(blueTheme.Id, blueTheme);
        _themes.TryAdd(greenTheme.Id, greenTheme);
    }

    #endregion

    #region Theme Management

    public List<ThemeInfo> GetAvailableThemes()
    {
        return _themes.Values.Select(t => new ThemeInfo
        {
            Id = t.Id,
            Name = t.Name,
            NameFa = t.NameFa,
            IsDark = t.IsDark,
            IsBuiltIn = t.Id is "Dark" or "Light" or "Blue" or "Green",
            PreviewColor = t.Colors.GetValueOrDefault("Primary")
        }).ToList();
    }

    public Result<bool> ApplyTheme(string themeId)
    {
        if (!_themes.TryGetValue(themeId, out var theme))
        {
            return Result.Failure<bool>(Error.NotFound);
        }

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var resources = Application.Current.Resources;

                // اعمال رنگ‌ها
                foreach (var color in theme.Colors)
                {
                    var colorValue = (Color)ColorConverter.ConvertFromString(color.Value);
                    var brush = new SolidColorBrush(colorValue);
                    brush.Freeze();

                    resources[$"{color.Key}Color"] = colorValue;
                    resources[$"{color.Key}Brush"] = brush;
                }

                // اعمال فونت
                if (theme.Fonts.TryGetValue("Default", out var fontFamily))
                {
                    resources["DefaultFontFamily"] = new FontFamily(fontFamily);
                    _currentFont = fontFamily;
                }
            });

            _currentThemeId = themeId;
            OnThemeChanged?.Invoke(this, themeId);
            _eventBus.Publish(new ThemeChangedEvent(themeId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Validation($"خطا در اعمال تم: {ex.Message}"));
        }
    }

    public string GetCurrentTheme() => _currentThemeId;

    public Result<bool> RegisterTheme(ThemeDefinition theme)
    {
        if (string.IsNullOrWhiteSpace(theme.Id))
        {
            return Result.Failure<bool>(Error.Validation("شناسه تم الزامی است"));
        }

        _themes.AddOrUpdate(theme.Id, theme, (_, _) => theme);
        return Result.Success(true);
    }

    public async Task<Result<bool>> SaveCustomThemeAsync(ThemeDefinition theme)
    {
        try
        {
            if (!Directory.Exists(_themesPath))
            {
                Directory.CreateDirectory(_themesPath);
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(theme);
            var filePath = Path.Combine(_themesPath, $"{theme.Id}.yaml");
            await File.WriteAllTextAsync(filePath, yaml);

            RegisterTheme(theme);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Validation($"خطا در ذخیره تم: {ex.Message}"));
        }
    }

    private async Task LoadCustomThemesAsync()
    {
        try
        {
            if (!Directory.Exists(_themesPath))
                return;

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var files = Directory.GetFiles(_themesPath, "*.yaml");
            foreach (var file in files)
            {
                try
                {
                    var yaml = await File.ReadAllTextAsync(file);
                    var theme = deserializer.Deserialize<ThemeDefinition>(yaml);
                    if (theme != null && !string.IsNullOrWhiteSpace(theme.Id))
                    {
                        _themes.TryAdd(theme.Id, theme);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری تم {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری تم‌ها: {ex.Message}");
        }
    }

    #endregion

    #region Color & Brush

    public Color GetColor(string colorKey)
    {
        if (_themes.TryGetValue(_currentThemeId, out var theme))
        {
            if (theme.Colors.TryGetValue(colorKey, out var colorStr))
            {
                return (Color)ColorConverter.ConvertFromString(colorStr);
            }
        }

        return Colors.Gray;
    }

    public Brush GetBrush(string brushKey)
    {
        var color = GetColor(brushKey.Replace("Brush", ""));
        return new SolidColorBrush(color);
    }

    #endregion

    #region Font

    public void SetFont(string fontFamily)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Resources["DefaultFontFamily"] = new FontFamily(fontFamily);
            });
            _currentFont = fontFamily;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در تنظیم فونت: {ex.Message}");
        }
    }

    public string GetCurrentFont() => _currentFont;

    #endregion
}

/// <summary>
/// رویداد تغییر تم
/// </summary>
public record ThemeChangedEvent(string ThemeId);

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/ThemeEngine.cs
// =============================================================================