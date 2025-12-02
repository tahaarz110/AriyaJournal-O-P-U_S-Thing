// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/BaseViewModel.cs
// شماره فایل: 73
// توضیح: کلاس پایه ViewModel
// =============================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// کلاس پایه برای همه ViewModelها
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _hasError;
    private bool _hasSuccess;

    /// <summary>
    /// آیا در حال پردازش است
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }
    }

    /// <summary>
    /// آیا در حال پردازش نیست
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// عنوان صفحه
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// پیام خطا
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                HasError = !string.IsNullOrEmpty(value);
            }
        }
    }

    /// <summary>
    /// پیام موفقیت
    /// </summary>
    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            if (SetProperty(ref _successMessage, value))
            {
                HasSuccess = !string.IsNullOrEmpty(value);
            }
        }
    }

    /// <summary>
    /// آیا خطا دارد
    /// </summary>
    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    /// <summary>
    /// آیا پیام موفقیت دارد
    /// </summary>
    public bool HasSuccess
    {
        get => _hasSuccess;
        set => SetProperty(ref _hasSuccess, value);
    }

    /// <summary>
    /// پاک کردن پیام‌ها
    /// </summary>
    public void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    /// <summary>
    /// نمایش خطا
    /// </summary>
    protected void ShowError(string message)
    {
        SuccessMessage = string.Empty;
        ErrorMessage = message;
    }

    /// <summary>
    /// نمایش موفقیت
    /// </summary>
    protected void ShowSuccess(string message)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = message;
    }

    /// <summary>
    /// اجرای async با مدیریت IsBusy
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> action, string? errorPrefix = null)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearMessages();
            await action();
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrEmpty(errorPrefix)
                ? ex.Message
                : $"{errorPrefix}: {ex.Message}";
            ShowError(message);
            System.Diagnostics.Debug.WriteLine($"خطا: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// اجرای async با مقدار برگشتی
    /// </summary>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> action, string? errorPrefix = null)
    {
        if (IsBusy) return default;

        try
        {
            IsBusy = true;
            ClearMessages();
            return await action();
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrEmpty(errorPrefix)
                ? ex.Message
                : $"{errorPrefix}: {ex.Message}";
            ShowError(message);
            System.Diagnostics.Debug.WriteLine($"خطا: {ex}");
            return default;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// متد مجازی برای بارگذاری اولیه
    /// </summary>
    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// متد مجازی برای پاکسازی
    /// </summary>
    public virtual void Cleanup()
    {
    }
}

/// <summary>
/// کلاس پایه با پارامتر ناوبری
/// </summary>
public abstract class BaseViewModel<TParameter> : BaseViewModel
{
    /// <summary>
    /// پارامتر ناوبری
    /// </summary>
    public TParameter? Parameter { get; private set; }

    /// <summary>
    /// تنظیم پارامتر و بارگذاری
    /// </summary>
    public async Task InitializeAsync(TParameter? parameter)
    {
        Parameter = parameter;
        await OnParameterSetAsync(parameter);
        await InitializeAsync();
    }

    /// <summary>
    /// متد مجازی برای پردازش پارامتر
    /// </summary>
    protected virtual Task OnParameterSetAsync(TParameter? parameter)
    {
        return Task.CompletedTask;
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/BaseViewModel.cs
// =============================================================================