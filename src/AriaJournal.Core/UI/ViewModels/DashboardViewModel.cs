// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/DashboardViewModel.cs
// توضیح: ViewModel داشبورد
// =============================================================================

using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Engines;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel داشبورد
/// </summary>
public partial class DashboardViewModel : BaseViewModel
{
    private readonly IDataEngine _dataEngine;
    private readonly IAggregationEngine _aggregationEngine;
    private readonly IStateEngine _stateEngine;
    private readonly IEventBusEngine _eventBus;

    #region Properties

    [ObservableProperty]
    private ObservableCollection<KpiCardModel> _kpiCards = new();

    [ObservableProperty]
    private ObservableCollection<WidgetModel> _widgets = new();

    [ObservableProperty]
    private ObservableCollection<string> _timeRanges = new()
    {
        "امروز",
        "این هفته",
        "این ماه",
        "سه ماه اخیر",
        "شش ماه اخیر",
        "امسال",
        "همه"
    };

    [ObservableProperty]
    private string _selectedTimeRange = "این ماه";

    [ObservableProperty]
    private string _lastUpdateText = string.Empty;

    #endregion

    #region Constructor

    public DashboardViewModel(
        IDataEngine dataEngine,
        IAggregationEngine aggregationEngine,
        IStateEngine stateEngine,
        IEventBusEngine eventBus)
    {
        _dataEngine = dataEngine;
        _aggregationEngine = aggregationEngine;
        _stateEngine = stateEngine;
        _eventBus = eventBus;

        // Subscribe to events
        _eventBus.Subscribe<TradeCreatedEvent>(OnTradeCreated);
        _eventBus.Subscribe<TradeUpdatedEvent>(OnTradeUpdated);
        _eventBus.Subscribe<AccountChangedEvent>(OnAccountChanged);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync();
    }

    [RelayCommand]
    private void Customize()
    {
        // باز کردن پنجره سفارشی‌سازی داشبورد
        // در آینده پیاده‌سازی می‌شود
    }

    #endregion

    #region Data Loading

    public async Task LoadDashboardDataAsync()
    {
        IsBusy = true;

        try
        {
            var accountId = _stateEngine.Get<int>("CurrentAccountId");
            if (accountId == 0) return;

            // دریافت معاملات
            var trades = await GetTradesForTimeRangeAsync(accountId);
            
            // محاسبه آمار
            var stats = await _aggregationEngine.CalculateStatisticsAsync(trades);

            // بروزرسانی KPI Cards
            UpdateKpiCards(stats, trades);

            // بروزرسانی زمان آخرین بروزرسانی
            LastUpdateText = $"آخرین بروزرسانی: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"خطا در بارگذاری داشبورد: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<List<Trade>> GetTradesForTimeRangeAsync(int accountId)
    {
        var repository = _dataEngine.Repository<Trade>();
        var allTrades = await repository.GetAllAsync();
        
        var accountTrades = allTrades.Where(t => t.AccountId == accountId);
        
        // فیلتر بر اساس بازه زمانی
        var now = DateTime.Now;
        var filteredTrades = SelectedTimeRange switch
        {
            "امروز" => accountTrades.Where(t => t.EntryTime?.Date == now.Date),
            "این هفته" => accountTrades.Where(t => t.EntryTime >= now.AddDays(-7)),
            "این ماه" => accountTrades.Where(t => t.EntryTime >= now.AddMonths(-1)),
            "سه ماه اخیر" => accountTrades.Where(t => t.EntryTime >= now.AddMonths(-3)),
            "شش ماه اخیر" => accountTrades.Where(t => t.EntryTime >= now.AddMonths(-6)),
            "امسال" => accountTrades.Where(t => t.EntryTime?.Year == now.Year),
            _ => accountTrades
        };

        return filteredTrades.ToList();
    }

    private void UpdateKpiCards(TradeStatistics stats, List<Trade> trades)
    {
        KpiCards.Clear();

        // تعداد معاملات
        KpiCards.Add(new KpiCardModel
        {
            Icon = "📊",
            Title = "تعداد معاملات",
            Value = stats.TotalTrades.ToString("N0"),
            Subtitle = $"{stats.WinningTrades} برد | {stats.LosingTrades} باخت",
            ValueColor = Brushes.White
        });

        // نرخ برد
        KpiCards.Add(new KpiCardModel
        {
            Icon = "🎯",
            Title = "نرخ برد",
            Value = $"{stats.WinRate:N1}%",
            ValueColor = stats.WinRate >= 50 ? Brushes.LimeGreen : Brushes.OrangeRed
        });

        // سود/زیان کل
        KpiCards.Add(new KpiCardModel
        {
            Icon = "💰",
            Title = "سود/زیان کل",
            Value = $"${stats.NetProfitLoss:N2}",
            ValueColor = stats.NetProfitLoss >= 0 ? Brushes.LimeGreen : Brushes.OrangeRed
        });

        // Profit Factor
        KpiCards.Add(new KpiCardModel
        {
            Icon = "📈",
            Title = "فاکتور سود",
            Value = stats.ProfitFactor.ToString("N2"),
            ValueColor = stats.ProfitFactor >= 1.5m ? Brushes.LimeGreen : 
                         stats.ProfitFactor >= 1 ? Brushes.Orange : Brushes.OrangeRed
        });

        // میانگین R:R
        KpiCards.Add(new KpiCardModel
        {
            Icon = "⚖️",
            Title = "میانگین R:R",
            Value = $"1:{stats.AverageRR:N2}",
            ValueColor = stats.AverageRR >= 2 ? Brushes.LimeGreen : 
                         stats.AverageRR >= 1 ? Brushes.Orange : Brushes.OrangeRed
        });

        // معاملات باز
        var openTrades = trades.Count(t => !t.IsClosed);
        KpiCards.Add(new KpiCardModel
        {
            Icon = "🔓",
            Title = "معاملات باز",
            Value = openTrades.ToString(),
            ValueColor = Brushes.CornflowerBlue
        });
    }

    #endregion

    #region Event Handlers

    private void OnTradeCreated(TradeCreatedEvent e)
    {
        _ = RefreshAsync();
    }

    private void OnTradeUpdated(TradeUpdatedEvent e)
    {
        _ = RefreshAsync();
    }

    private void OnAccountChanged(AccountChangedEvent e)
    {
        _ = RefreshAsync();
    }

    #endregion

    #region Partial Methods

    partial void OnSelectedTimeRangeChanged(string value)
    {
        _ = RefreshAsync();
    }

    #endregion
}

/// <summary>
/// مدل کارت KPI
/// </summary>
public class KpiCardModel
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public Brush ValueColor { get; set; } = Brushes.White;
    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
}

/// <summary>
/// مدل ویجت
/// </summary>
public class WidgetModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 200;
    public string Type { get; set; } = "value";
}

// رویدادهای مورد نیاز
public record TradeCreatedEvent(Trade Trade);
public record TradeUpdatedEvent(Trade Trade);
public record AccountChangedEvent(int AccountId);

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/DashboardViewModel.cs
// =============================================================================