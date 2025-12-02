// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/INavigationEngine.cs
// توضیح: اینترفیس موتور ناوبری - اصلاح‌شده
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// اینترفیس موتور ناوبری
/// </summary>
public interface INavigationEngine
{
    /// <summary>
    /// نام View فعلی
    /// </summary>
    string CurrentView { get; }

    /// <summary>
    /// آیا امکان بازگشت وجود دارد
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// پارامتر صفحه فعلی
    /// </summary>
    object? CurrentParameter { get; }

    /// <summary>
    /// تاریخچه ناوبری
    /// </summary>
    IReadOnlyList<string> NavigationHistory { get; }

    /// <summary>
    /// رویداد ناوبری
    /// </summary>
    event EventHandler<NavigationEventArgs>? Navigated;

    /// <summary>
    /// تنظیم Frame اصلی برای نمایش صفحات
    /// </summary>
    void SetMainFrame(ContentControl frame);

    /// <summary>
    /// ناوبری به یک View
    /// </summary>
    Task NavigateToAsync(string viewName, object? parameter = null);

    /// <summary>
    /// بازگشت به صفحه قبلی
    /// </summary>
    Task NavigateBackAsync();

    /// <summary>
    /// ثبت یک View
    /// </summary>
    void RegisterView(string name, Type viewType, Type? viewModelType = null);

    /// <summary>
    /// حذف ثبت یک View
    /// </summary>
    void UnregisterView(string name);

    /// <summary>
    /// بررسی ثبت بودن View
    /// </summary>
    bool IsViewRegistered(string name);

    /// <summary>
    /// دریافت لیست View های ثبت‌شده
    /// </summary>
    IEnumerable<string> GetRegisteredViews();

    /// <summary>
    /// پاک کردن تاریخچه ناوبری
    /// </summary>
    void ClearHistory();
}

/// <summary>
/// آرگومان‌های رویداد ناوبری
/// </summary>
public class NavigationEventArgs : EventArgs
{
    public string FromView { get; set; } = string.Empty;
    public string ToView { get; set; } = string.Empty;
    public object? Parameter { get; set; }
}

// =============================================================================
// پایان فایل
// =============================================================================