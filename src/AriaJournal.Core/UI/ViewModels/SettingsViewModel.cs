// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/SettingsViewModel.cs
// توضیح: ViewModel تنظیمات - اصلاح‌شده کامل برای پشتیبانی از ۴ تم
// بخش ۱ از ۲
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel تنظیمات
/// </summary>
public partial class SettingsViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly IBackupEngine _backupEngine;

    private int _themeIndex;
    private int _languageIndex;
    private string _selectedDateFormat = "yyyy/MM/dd";
    private int _priceDecimalsIndex = 3;
    private string _itemsPerPage = "50";
    private bool _autoBackup = true;
    private string _backupInterval = "7";
    private string _backupPath = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isInitialized = false;

    public SettingsViewModel(
        AuthService authService,
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        IBackupEngine backupEngine)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _backupEngine = backupEngine ?? throw new ArgumentNullException(nameof(backupEngine));

        Title = "تنظیمات";

        DateFormats = new ObservableCollection<string>
        {
            "yyyy/MM/dd",
            "dd/MM/yyyy",
            "MM/dd/yyyy",
            "yyyy-MM-dd"
        };

        BackupPath = _backupEngine.BackupPath;

        // مقداردهی اولیه تم از App (بدون اعمال تغییر)
        _themeIndex = App.GetThemeIndex();
    }

    #region Properties

    public ObservableCollection<string> DateFormats { get; }

    /// <summary>
    /// ایندکس تم انتخاب‌شده
    /// 0 = Dark, 1 = Light, 2 = Blue, 3 = Green
    /// </summary>
    public int ThemeIndex
    {
        get => _themeIndex;
        set
        {
            if (SetProperty(ref _themeIndex, value))
            {
                // فقط اگر Initialize شده بود، تم را اعمال کن
                if (_isInitialized)
                {
                    ApplyThemeImmediately();
                }
            }
        }
    }

    public int LanguageIndex
    {
        get => _languageIndex;
        set => SetProperty(ref _languageIndex, value);
    }

    public string SelectedDateFormat
    {
        get => _selectedDateFormat;
        set => SetProperty(ref _selectedDateFormat, value);
    }

    public int PriceDecimalsIndex
    {
        get => _priceDecimalsIndex;
        set => SetProperty(ref _priceDecimalsIndex, value);
    }

    public string ItemsPerPage
    {
        get => _itemsPerPage;
        set => SetProperty(ref _itemsPerPage, value);
    }

    public bool AutoBackup
    {
        get => _autoBackup;
        set => SetProperty(ref _autoBackup, value);
    }

    public string BackupInterval
    {
        get => _backupInterval;
        set => SetProperty(ref _backupInterval, value);
    }

    public string BackupPath
    {
        get => _backupPath;
        set => SetProperty(ref _backupPath, value);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SaveAsync()
    {
        await ExecuteAsync(async () =>
        {
            var userId = _authService.CurrentUser?.Id;
            if (!userId.HasValue)
                return;

            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (settings == null)
            {
                settings = new Settings
                {
                    UserId = userId.Value
                };
                await settingsRepo.AddAsync(settings);
            }

            // ذخیره نام تم
            settings.Theme = App.GetThemeNameByIndex(ThemeIndex);
            settings.Language = LanguageIndex == 0 ? "fa" : "en";
            settings.DateFormat = SelectedDateFormat;
            settings.PriceDecimals = PriceDecimalsIndex + 2;
            settings.ItemsPerPage = int.TryParse(ItemsPerPage, out var items) ? items : 50;
            settings.AutoBackup = AutoBackup;
            settings.AutoBackupInterval = int.TryParse(BackupInterval, out var interval) ? interval : 7;
            settings.BackupPath = BackupPath;
            settings.UpdatedAt = DateTime.Now;

            settingsRepo.Update(settings);
            await _unitOfWork.SaveChangesAsync();

            _backupEngine.SetBackupPath(BackupPath);

            ShowSuccess("تنظیمات با موفقیت ذخیره شد");

        }, "خطا در ذخیره تنظیمات");
    }

    [RelayCommand]
    private void SelectBackupPath()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "مسیر ذخیره فایل‌های پشتیبان را انتخاب کنید",
                InitialDirectory = BackupPath
            };

            if (dialog.ShowDialog() == true)
            {
                BackupPath = dialog.FolderName;
            }
        }
        catch
        {
            // اگر OpenFolderDialog در دسترس نبود
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "مسیر ذخیره پشتیبان را انتخاب کنید",
                FileName = "SelectFolder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                BackupPath = System.IO.Path.GetDirectoryName(dialog.FileName) ?? BackupPath;
            }
        }
    }

    // =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/SettingsViewModel.cs
// بخش ۲ از ۲
// =============================================================================

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ShowError("رمز عبور فعلی را وارد کنید");
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ShowError("رمز عبور جدید را وارد کنید");
            return;
        }

        if (NewPassword.Length < 6)
        {
            ShowError("رمز عبور جدید باید حداقل ۶ کاراکتر باشد");
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ShowError("رمز عبور جدید و تکرار آن یکسان نیستند");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var userId = _authService.CurrentUser?.Id ?? 0;

            var dto = new ChangePasswordDto
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmNewPassword = ConfirmPassword
            };

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (result.IsSuccess)
            {
                ShowSuccess("رمز عبور با موفقیت تغییر کرد");
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                ShowError(result.Error.Message);
            }

        }, "خطا در تغییر رمز عبور");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// اعمال تم فوری هنگام تغییر انتخاب
    /// </summary>
    private void ApplyThemeImmediately()
    {
        try
        {
            var themeName = App.GetThemeNameByIndex(ThemeIndex);
            App.ChangeTheme(themeName);

            // اطلاع‌رسانی به سایر بخش‌ها
            _eventBus.Publish(new ThemeChangedEvent(themeName));

            System.Diagnostics.Debug.WriteLine($"تم تغییر کرد به: {themeName}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در اعمال تم: {ex.Message}");
            ShowError("خطا در تغییر تم");
        }
    }

    /// <summary>
    /// بارگذاری تنظیمات از دیتابیس
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        var userId = _authService.CurrentUser?.Id;
        if (!userId.HasValue)
            return;

        await ExecuteAsync(async () =>
        {
            var settingsRepo = _unitOfWork.Repository<Settings>();
            var settings = await settingsRepo.FirstOrDefaultAsync(s => s.UserId == userId.Value);

            if (settings != null)
            {
                // تبدیل نام تم به ایندکس - بدون اعمال تم
                _themeIndex = settings.Theme?.ToLower() switch
                {
                    "light" => 1,
                    "blue" => 2,
                    "green" => 3,
                    _ => 0 // Dark
                };
                OnPropertyChanged(nameof(ThemeIndex));

                LanguageIndex = settings.Language == "en" ? 1 : 0;
                SelectedDateFormat = settings.DateFormat ?? "yyyy/MM/dd";
                PriceDecimalsIndex = Math.Max(0, settings.PriceDecimals - 2);
                ItemsPerPage = settings.ItemsPerPage.ToString();
                AutoBackup = settings.AutoBackup;
                BackupInterval = settings.AutoBackupInterval.ToString();
                BackupPath = settings.BackupPath ?? _backupEngine.BackupPath;

                // اعمال تم ذخیره‌شده
                App.ChangeTheme(settings.Theme ?? "Dark");
            }
        });
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadSettingsAsync();
        _isInitialized = true; // حالا می‌تواند تم را تغییر دهد
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/SettingsViewModel.cs
// =============================================================================
