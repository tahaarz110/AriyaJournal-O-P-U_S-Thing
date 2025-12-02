// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/NavigationEngine.cs
// توضیح: موتور ناوبری - سازگار با INavigationEngine
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.UI.ViewModels;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور ناوبری
/// </summary>
public class NavigationEngine : INavigationEngine
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ViewRegistration> _viewRegistry;
    private readonly Stack<NavigationEntry> _navigationStack;
    private ContentControl? _mainFrame;
    private string _currentView = string.Empty;
    private object? _currentParameter;

    public NavigationEngine(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _viewRegistry = new Dictionary<string, ViewRegistration>(StringComparer.OrdinalIgnoreCase);
        _navigationStack = new Stack<NavigationEntry>();
    }

    #region INavigationEngine Implementation

    public string CurrentView => _currentView;

    public bool CanGoBack => _navigationStack.Count > 1;

    public object? CurrentParameter => _currentParameter;

    public IReadOnlyList<string> NavigationHistory
    {
        get
        {
            var list = _navigationStack.Select(e => e.ViewName).ToList();
            list.Reverse();
            return list;
        }
    }

    public event EventHandler<NavigationEventArgs>? Navigated;

    public void SetMainFrame(ContentControl frame)
    {
        _mainFrame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    public async Task NavigateToAsync(string viewName, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(viewName))
            throw new ArgumentNullException(nameof(viewName));

        if (_mainFrame == null)
            throw new InvalidOperationException("NavigationHost تنظیم نشده است. ابتدا SetMainFrame را فراخوانی کنید.");

        if (!_viewRegistry.TryGetValue(viewName, out var registration))
            throw new InvalidOperationException($"View با نام '{viewName}' یافت نشد. ابتدا آن را ثبت کنید.");

        try
        {
            var previousView = _currentView;

            // ایجاد View
            FrameworkElement? view = null;
            
            try
            {
                view = _serviceProvider.GetService(registration.ViewType) as FrameworkElement;
            }
            catch
            {
                // اگر از DI نشد، مستقیم بساز
            }

            if (view == null)
            {
                view = Activator.CreateInstance(registration.ViewType) as FrameworkElement;
            }

            if (view == null)
                throw new InvalidOperationException($"خطا در ایجاد View: {viewName}");

            // ایجاد و تنظیم ViewModel
            object? viewModel = null;
            if (registration.ViewModelType != null)
            {
                viewModel = _serviceProvider.GetService(registration.ViewModelType);
                if (viewModel != null)
                {
                    view.DataContext = viewModel;

                    // ارسال پارامتر به ViewModel
                    await SetViewModelParameterAsync(viewModel, parameter);

                    // Initialize ViewModel
                    if (viewModel is BaseViewModel baseVm)
                    {
                        await baseVm.InitializeAsync();
                    }
                }
            }

            // نمایش View در UI Thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainFrame.Content = view;
            });

            // ذخیره در Stack
            if (_currentView != viewName)
            {
                _navigationStack.Push(new NavigationEntry
                {
                    ViewName = viewName,
                    Parameter = parameter
                });
            }

            _currentView = viewName;
            _currentParameter = parameter;

            // رویداد ناوبری
            Navigated?.Invoke(this, new NavigationEventArgs
            {
                FromView = previousView,
                ToView = viewName,
                Parameter = parameter
            });

            System.Diagnostics.Debug.WriteLine($"ناوبری به: {viewName}" + (parameter != null ? $" با پارامتر: {parameter}" : ""));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ناوبری به {viewName}: {ex.Message}");
            throw;
        }
    }

    public async Task NavigateBackAsync()
    {
        if (!CanGoBack)
            return;

        // حذف صفحه فعلی
        _navigationStack.Pop();

        if (_navigationStack.Count > 0)
        {
            var previousEntry = _navigationStack.Pop(); // حذف برای جلوگیری از دوباره اضافه شدن
            await NavigateToAsync(previousEntry.ViewName, previousEntry.Parameter);
        }
    }

    public void RegisterView(string name, Type viewType, Type? viewModelType = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (viewType == null)
            throw new ArgumentNullException(nameof(viewType));

        _viewRegistry[name] = new ViewRegistration
        {
            ViewType = viewType,
            ViewModelType = viewModelType
        };

        System.Diagnostics.Debug.WriteLine($"View ثبت شد: {name} -> {viewType.Name}");
    }

    public void UnregisterView(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        _viewRegistry.Remove(name);
    }

    public bool IsViewRegistered(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return _viewRegistry.ContainsKey(name);
    }

    public IEnumerable<string> GetRegisteredViews()
    {
        return _viewRegistry.Keys.ToList();
    }

    public void ClearHistory()
    {
        _navigationStack.Clear();
    }

    #endregion

    #region Private Methods

    private async Task SetViewModelParameterAsync(object viewModel, object? parameter)
    {
        if (parameter == null)
            return;

        // روش ۱: متد SetParameterAsync
        var setParameterMethod = viewModel.GetType().GetMethod("SetParameterAsync");
        if (setParameterMethod != null)
        {
            var task = setParameterMethod.Invoke(viewModel, new[] { parameter }) as Task;
            if (task != null)
                await task;
            return;
        }

        // روش ۲: Property به نام Parameter
        var parameterProperty = viewModel.GetType().GetProperty("Parameter");
        if (parameterProperty != null && parameterProperty.CanWrite)
        {
            parameterProperty.SetValue(viewModel, parameter);
            return;
        }

        // روش ۳: ذخیره در StateEngine
        var stateEngine = _serviceProvider.GetService<IStateEngine>();
        stateEngine?.Set("NavigationParameter", parameter);
    }

    #endregion

    #region Inner Classes

    private class ViewRegistration
    {
        public Type ViewType { get; set; } = null!;
        public Type? ViewModelType { get; set; }
    }

    private class NavigationEntry
    {
        public string ViewName { get; set; } = string.Empty;
        public object? Parameter { get; set; }
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/NavigationEngine.cs
// =============================================================================