// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/TradeListViewModel.cs
// توضیح: ViewModel لیست معاملات - اصلاح‌شده
// =============================================================================

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Interfaces.Engines;

using AriaJournal.Core.Domain.Events;
namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel لیست معاملات
/// </summary>
public partial class TradeListViewModel : BaseViewModel
{
    private readonly TradeService _tradeService;
    private readonly INavigationEngine _navigationEngine;
    private readonly IStateEngine _stateEngine;
    private readonly IEventBusEngine _eventBus;

    private ObservableCollection<TradeDto> _trades;
    private TradeDto? _selectedTrade;
    private string _filterSymbol = string.Empty;
    private DateTime? _filterFromDate;
    private DateTime? _filterToDate;
    private int _filterStatusIndex;
    private int _currentPage = 1;
    private int _pageSize = 50;
    private int _totalCount;
    private decimal _totalProfitLoss;
    private int _winCount;
    private int _lossCount;

    public TradeListViewModel(
        TradeService tradeService,
        INavigationEngine navigationEngine,
        IStateEngine stateEngine,
        IEventBusEngine eventBus)
    {
        _tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        _navigationEngine = navigationEngine ?? throw new ArgumentNullException(nameof(navigationEngine));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _trades = new ObservableCollection<TradeDto>();
        Title = "معاملات";

        SubscribeToEvents();
    }

    #region Properties

    public ObservableCollection<TradeDto> Trades
    {
        get => _trades;
        set => SetProperty(ref _trades, value);
    }

    public TradeDto? SelectedTrade
    {
        get => _selectedTrade;
        set => SetProperty(ref _selectedTrade, value);
    }

    public string FilterSymbol
    {
        get => _filterSymbol;
        set => SetProperty(ref _filterSymbol, value);
    }

    public DateTime? FilterFromDate
    {
        get => _filterFromDate;
        set => SetProperty(ref _filterFromDate, value);
    }

    public DateTime? FilterToDate
    {
        get => _filterToDate;
        set => SetProperty(ref _filterToDate, value);
    }

    public int FilterStatusIndex
    {
        get => _filterStatusIndex;
        set => SetProperty(ref _filterStatusIndex, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(PageInfo));
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                CurrentPage = 1;
                _ = LoadTradesAsync();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(HasTrades));
                OnPropertyChanged(nameof(IsEmpty));
            }
        }
    }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    public string PageInfo => $"صفحه {CurrentPage} از {Math.Max(1, TotalPages)} (مجموع: {TotalCount} معامله)";

    public bool HasTrades => TotalCount > 0;
    public bool IsEmpty => !IsBusy && TotalCount == 0;

    public decimal TotalProfitLoss
    {
        get => _totalProfitLoss;
        set
        {
            if (SetProperty(ref _totalProfitLoss, value))
            {
                OnPropertyChanged(nameof(TotalProfitLossDisplay));
            }
        }
    }

    public string TotalProfitLossDisplay
    {
        get
        {
            var sign = TotalProfitLoss >= 0 ? "+" : "";
            return $"{sign}{TotalProfitLoss:N2} $";
        }
    }

    public int WinCount
    {
        get => _winCount;
        set
        {
            if (SetProperty(ref _winCount, value))
            {
                OnPropertyChanged(nameof(WinRateDisplay));
            }
        }
    }

    public int LossCount
    {
        get => _lossCount;
        set
        {
            if (SetProperty(ref _lossCount, value))
            {
                OnPropertyChanged(nameof(WinRateDisplay));
            }
        }
    }

    public string WinRateDisplay
    {
        get
        {
            var total = WinCount + LossCount;
            if (total == 0) return "0%";
            var rate = (decimal)WinCount / total * 100;
            return $"{rate:N1}%";
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task NewTradeAsync()
    {
        await _navigationEngine.NavigateToAsync("TradeEntry");
    }

    [RelayCommand]
    private async Task EditTradeAsync(TradeDto? trade)
    {
        if (trade == null) return;
        
        // ارسال ID معامله به صفحه ویرایش
        await _navigationEngine.NavigateToAsync("TradeEntry", trade.Id);
    }

    [RelayCommand]
    private async Task DeleteTradeAsync(TradeDto? trade)
    {
        if (trade == null) return;

        var result = MessageBox.Show(
            $"آیا از حذف معامله #{trade.Id} ({trade.Symbol}) مطمئن هستید؟",
            "تأیید حذف",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await ExecuteAsync(async () =>
            {
                var deleteResult = await _tradeService.DeleteAsync(trade.Id);
                if (deleteResult.IsSuccess)
                {
                    ShowSuccess("معامله با موفقیت حذف شد");
                    await LoadTradesAsync();
                }
                else
                {
                    ShowError(deleteResult.Error.Message);
                }
            }, "خطا در حذف معامله");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadTradesAsync();
    }

    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        CurrentPage = 1;
        await LoadTradesAsync();
    }

    [RelayCommand]
    private async Task FirstPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage = 1;
            await LoadTradesAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadTradesAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadTradesAsync();
        }
    }

    [RelayCommand]
    private async Task LastPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage = TotalPages;
            await LoadTradesAsync();
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadTradesAsync()
    {
        var accountId = _stateEngine.Get<int?>(StateKeys.CurrentAccountId);
        if (!accountId.HasValue)
        {
            Trades.Clear();
            TotalCount = 0;
            return;
        }

        await ExecuteAsync(async () =>
        {
            var filter = BuildFilter(accountId.Value);
            var result = await _tradeService.GetTradesAsync(filter);

            if (result.IsSuccess)
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Trades.Clear();
                    foreach (var trade in result.Value.Items)
                    {
                        Trades.Add(trade);
                    }

                    TotalCount = result.Value.TotalCount;
                    TotalProfitLoss = result.Value.TotalProfitLoss;
                    WinCount = result.Value.WinCount;
                    LossCount = result.Value.LossCount;
                });
            }
            else
            {
                ShowError(result.Error.Message);
            }
        }, "خطا در بارگذاری معاملات");
    }

    private TradeFilterDto BuildFilter(int accountId)
    {
        var filter = new TradeFilterDto
        {
            AccountId = accountId,
            PageNumber = CurrentPage,
            PageSize = PageSize,
            SortBy = "EntryTime",
            SortDescending = true
        };

        if (!string.IsNullOrWhiteSpace(FilterSymbol))
            filter.Symbol = FilterSymbol.Trim();

        if (FilterFromDate.HasValue)
            filter.FromDate = FilterFromDate.Value;

        if (FilterToDate.HasValue)
            filter.ToDate = FilterToDate.Value.AddDays(1).AddSeconds(-1);

        // فیلتر وضعیت
        filter.IsClosed = FilterStatusIndex switch
        {
            1 => false, // باز
            2 => true,  // بسته
            _ => null   // همه
        };

        filter.IsProfit = FilterStatusIndex switch
        {
            3 => true,  // سودده
            4 => false, // ضررده
            _ => null
        };

        return filter;
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<TradeCreatedEvent>(async e =>
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadTradesAsync();
            });
        });

        _eventBus.Subscribe<TradeUpdatedEvent>(async e =>
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadTradesAsync();
            });
        });

        _eventBus.Subscribe<TradeDeletedEvent>(async e =>
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadTradesAsync();
            });
        });

        _eventBus.Subscribe<AccountSelectedEvent>(async e =>
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                CurrentPage = 1;
                await LoadTradesAsync();
            });
        });
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        await LoadTradesAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/TradeListViewModel.cs
// =============================================================================