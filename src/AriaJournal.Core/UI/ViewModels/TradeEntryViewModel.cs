// =============================================================================
// فایل: src/AriaJournal.Core/UI/ViewModels/TradeEntryViewModel.cs
// توضیح: ViewModel ثبت و ویرایش معامله - نسخه اصلاح‌شده
// =============================================================================

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Services;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces.Engines;

using AriaJournal.Core.Domain.Events;

namespace AriaJournal.Core.UI.ViewModels;

/// <summary>
/// ViewModel ثبت و ویرایش معامله
/// </summary>
public partial class TradeEntryViewModel : BaseViewModel
{
    private readonly TradeService _tradeService;
    private readonly INavigationEngine _navigationEngine;
    private readonly IStateEngine _stateEngine;
    private readonly IEventBusEngine _eventBus;

    private int? _tradeId;
    private string _symbol = string.Empty;
    private int _directionIndex;
    private string _volume = "0.01";
    private string _entryPrice = string.Empty;
    private string _exitPrice = string.Empty;
    private string _stopLoss = string.Empty;
    private string _takeProfit = string.Empty;
    private DateTime? _entryDate = DateTime.Today;
    private string _entryTimeText = DateTime.Now.ToString("HH:mm");
    private DateTime? _exitDate;
    private string _exitTimeText = string.Empty;
    private string _commission = "0";
    private string _swap = "0";
    private string _entryReason = string.Empty;
    private string _preTradeNotes = string.Empty;
    private string _postTradeNotes = string.Empty;
    private string _mistakes = string.Empty;
    private string _lessons = string.Empty;
    private bool _followedPlan;
    private bool _isImpulsive;
    private bool _isClosed;
    private int _executionRating;
    private decimal _calculatedProfitLoss;
    private decimal _calculatedPips;
    private decimal? _calculatedRR;
    private bool _isLoading;

    public TradeEntryViewModel(
        TradeService tradeService,
        INavigationEngine navigationEngine,
        IStateEngine stateEngine,
        IEventBusEngine eventBus)
    {
        _tradeService = tradeService ?? throw new ArgumentNullException(nameof(tradeService));
        _navigationEngine = navigationEngine ?? throw new ArgumentNullException(nameof(navigationEngine));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        Title = "ثبت معامله";
    }

    #region Properties

    public string PageTitle => IsNewTrade ? "➕ ثبت معامله جدید" : $"✏️ ویرایش معامله #{_tradeId}";
    public string SaveButtonText => IsNewTrade ? "ذخیره" : "بروزرسانی";
    public bool IsNewTrade => !_tradeId.HasValue;

    public string Symbol
    {
        get => _symbol;
        set => SetProperty(ref _symbol, value?.ToUpperInvariant() ?? string.Empty);
    }

    public int DirectionIndex
    {
        get => _directionIndex;
        set
        {
            if (SetProperty(ref _directionIndex, value))
            {
                OnPropertyChanged(nameof(Direction));
                OnPropertyChanged(nameof(DirectionText));
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public TradeDirection Direction => DirectionIndex == 0 ? TradeDirection.Buy : TradeDirection.Sell;
    public string DirectionText => Direction == TradeDirection.Buy ? "خرید" : "فروش";

    public string Volume
    {
        get => _volume;
        set
        {
            if (SetProperty(ref _volume, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string EntryPrice
    {
        get => _entryPrice;
        set
        {
            if (SetProperty(ref _entryPrice, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string ExitPrice
    {
        get => _exitPrice;
        set
        {
            if (SetProperty(ref _exitPrice, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string StopLoss
    {
        get => _stopLoss;
        set
        {
            if (SetProperty(ref _stopLoss, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string TakeProfit
    {
        get => _takeProfit;
        set
        {
            if (SetProperty(ref _takeProfit, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public DateTime? EntryDate
    {
        get => _entryDate;
        set => SetProperty(ref _entryDate, value);
    }

    public string EntryTimeText
    {
        get => _entryTimeText;
        set => SetProperty(ref _entryTimeText, value);
    }

    public DateTime? ExitDate
    {
        get => _exitDate;
        set => SetProperty(ref _exitDate, value);
    }

    public string ExitTimeText
    {
        get => _exitTimeText;
        set => SetProperty(ref _exitTimeText, value);
    }

    public string Commission
    {
        get => _commission;
        set
        {
            if (SetProperty(ref _commission, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string Swap
    {
        get => _swap;
        set
        {
            if (SetProperty(ref _swap, value))
            {
                if (!_isLoading) CalculateAll();
            }
        }
    }

    public string EntryReason
    {
        get => _entryReason;
        set => SetProperty(ref _entryReason, value);
    }

    public string PreTradeNotes
    {
        get => _preTradeNotes;
        set => SetProperty(ref _preTradeNotes, value);
    }

    public string PostTradeNotes
    {
        get => _postTradeNotes;
        set => SetProperty(ref _postTradeNotes, value);
    }

    public string Mistakes
    {
        get => _mistakes;
        set => SetProperty(ref _mistakes, value);
    }

    public string Lessons
    {
        get => _lessons;
        set => SetProperty(ref _lessons, value);
    }

    public bool FollowedPlan
    {
        get => _followedPlan;
        set => SetProperty(ref _followedPlan, value);
    }

    public bool IsImpulsive
    {
        get => _isImpulsive;
        set => SetProperty(ref _isImpulsive, value);
    }

    public bool IsClosed
    {
        get => _isClosed;
        set => SetProperty(ref _isClosed, value);
    }

    // Rating Properties
    public bool Rating1 { get => _executionRating == 1; set { if (value) SetRating(1); } }
    public bool Rating2 { get => _executionRating == 2; set { if (value) SetRating(2); } }
    public bool Rating3 { get => _executionRating == 3; set { if (value) SetRating(3); } }
    public bool Rating4 { get => _executionRating == 4; set { if (value) SetRating(4); } }
    public bool Rating5 { get => _executionRating == 5; set { if (value) SetRating(5); } }

    private void SetRating(int rating)
    {
        _executionRating = rating;
        OnPropertyChanged(nameof(Rating1));
        OnPropertyChanged(nameof(Rating2));
        OnPropertyChanged(nameof(Rating3));
        OnPropertyChanged(nameof(Rating4));
        OnPropertyChanged(nameof(Rating5));
    }

    // محاسبات نمایشی
    public string ProfitLossDisplay
    {
        get
        {
            if (_calculatedProfitLoss == 0 && string.IsNullOrEmpty(ExitPrice))
                return "-";
            var sign = _calculatedProfitLoss >= 0 ? "+" : "";
            return $"{sign}{_calculatedProfitLoss:N2} $";
        }
    }

    public string PipsDisplay
    {
        get
        {
            if (_calculatedPips == 0 && string.IsNullOrEmpty(ExitPrice))
                return "-";
            var sign = _calculatedPips >= 0 ? "+" : "";
            return $"{sign}{_calculatedPips:N1} پیپ";
        }
    }

    public string RiskRewardDisplay
    {
        get
        {
            if (!_calculatedRR.HasValue)
                return "-";
            return $"{_calculatedRR.Value:N2}R";
        }
    }

    public string ProfitLossColor => _calculatedProfitLoss >= 0 ? "#22C55E" : "#EF4444";
    public string PipsColor => _calculatedPips >= 0 ? "#22C55E" : "#EF4444";

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        await ExecuteAsync(async () =>
        {
            var accountId = _stateEngine.Get<int?>(StateKeys.CurrentAccountId);
            if (!accountId.HasValue)
            {
                ShowError("لطفاً ابتدا یک حساب انتخاب کنید");
                return;
            }

            if (IsNewTrade)
            {
                await CreateTradeAsync(accountId.Value);
            }
            else
            {
                await UpdateTradeAsync(accountId.Value);
            }
        }, "خطا در ذخیره معامله");
    }

    [RelayCommand]
    private async Task SaveAndNewAsync()
    {
        if (!ValidateForm()) return;

        await ExecuteAsync(async () =>
        {
            var accountId = _stateEngine.Get<int?>(StateKeys.CurrentAccountId);
            if (!accountId.HasValue)
            {
                ShowError("لطفاً ابتدا یک حساب انتخاب کنید");
                return;
            }

            await CreateTradeAsync(accountId.Value);
            ClearForm();
            ShowSuccess("معامله ذخیره شد. فرم برای ثبت معامله جدید آماده است.");
        }, "خطا در ذخیره معامله");
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationEngine.NavigateBackAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await _navigationEngine.NavigateBackAsync();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// تنظیم پارامتر (ID معامله برای ویرایش)
    /// </summary>
    public async Task SetParameterAsync(object? parameter)
    {
        _isLoading = true;

        try
        {
            if (parameter is int tradeId && tradeId > 0)
            {
                _tradeId = tradeId;
                await LoadTradeAsync(tradeId);
            }
            else
            {
                _tradeId = null;
                ClearForm();
            }

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
            OnPropertyChanged(nameof(IsNewTrade));
        }
        finally
        {
            _isLoading = false;
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadTradeAsync(int tradeId)
    {
        IsBusy = true;

        try
        {
            var result = await _tradeService.GetByIdAsync(tradeId);
            if (result.IsSuccess)
            {
                var trade = result.Value;

                Symbol = trade.Symbol;
                DirectionIndex = trade.Direction == TradeDirection.Buy ? 0 : 1;
                Volume = trade.Volume.ToString("G29");
                EntryPrice = trade.EntryPrice.ToString("G29");
                ExitPrice = trade.ExitPrice?.ToString("G29") ?? string.Empty;
                StopLoss = trade.StopLoss?.ToString("G29") ?? string.Empty;
                TakeProfit = trade.TakeProfit?.ToString("G29") ?? string.Empty;
                EntryDate = trade.EntryTime.Date;
                EntryTimeText = trade.EntryTime.ToString("HH:mm");
                ExitDate = trade.ExitTime?.Date;
                ExitTimeText = trade.ExitTime?.ToString("HH:mm") ?? string.Empty;
                Commission = trade.Commission.ToString("G29");
                Swap = trade.Swap.ToString("G29");
                EntryReason = trade.EntryReason ?? string.Empty;
                PreTradeNotes = trade.PreTradeNotes ?? string.Empty;
                PostTradeNotes = trade.PostTradeNotes ?? string.Empty;
                Mistakes = trade.Mistakes ?? string.Empty;
                Lessons = trade.Lessons ?? string.Empty;
                FollowedPlan = trade.FollowedPlan ?? false;
                IsImpulsive = trade.IsImpulsive ?? false;
                IsClosed = trade.IsClosed;

                if (trade.ExecutionRating.HasValue && trade.ExecutionRating.Value > 0)
                {
                    SetRating(trade.ExecutionRating.Value);
                }

                // مقادیر محاسبه‌شده
                _calculatedProfitLoss = trade.ProfitLoss ?? 0;
                _calculatedPips = trade.ProfitLossPips ?? 0;
                _calculatedRR = trade.RiskRewardRatio;

                OnPropertyChanged(nameof(ProfitLossDisplay));
                OnPropertyChanged(nameof(PipsDisplay));
                OnPropertyChanged(nameof(RiskRewardDisplay));
                OnPropertyChanged(nameof(ProfitLossColor));
                OnPropertyChanged(nameof(PipsColor));

                System.Diagnostics.Debug.WriteLine($"معامله #{tradeId} بارگذاری شد: {Symbol}");
            }
            else
            {
                ShowError(result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            ShowError($"خطا در بارگذاری معامله: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateTradeAsync(int accountId)
    {
        var dto = BuildCreateDto(accountId);
        var result = await _tradeService.CreateAsync(dto);

        if (result.IsSuccess)
        {
            _eventBus.Publish(new TradeCreatedEvent(result.Value.Id, accountId));
            ShowSuccess("معامله با موفقیت ثبت شد");
            await _navigationEngine.NavigateToAsync("Trades");
        }
        else
        {
            ShowError(result.Error.Message);
        }
    }

    private async Task UpdateTradeAsync(int accountId)
    {
        var dto = BuildUpdateDto(accountId);
        var result = await _tradeService.UpdateAsync(dto);

        if (result.IsSuccess)
        {
            _eventBus.Publish(new TradeUpdatedEvent(dto.Id));
            ShowSuccess("معامله با موفقیت بروزرسانی شد");
            await _navigationEngine.NavigateToAsync("Trades");
        }
        else
        {
            ShowError(result.Error.Message);
        }
    }

    private TradeCreateDto BuildCreateDto(int accountId)
    {
        var hasExit = !string.IsNullOrEmpty(ExitPrice) && decimal.TryParse(ExitPrice, out _);
        
        return new TradeCreateDto
        {
            AccountId = accountId,
            Symbol = Symbol,
            Direction = Direction,
            Volume = ParseDecimal(Volume),
            EntryPrice = ParseDecimal(EntryPrice),
            ExitPrice = ParseNullableDecimal(ExitPrice),
            StopLoss = ParseNullableDecimal(StopLoss),
            TakeProfit = ParseNullableDecimal(TakeProfit),
            EntryTime = GetEntryDateTime(),
            ExitTime = GetExitDateTime(),
            Commission = ParseDecimal(Commission),
            Swap = ParseDecimal(Swap),
            EntryReason = EntryReason,
            PreTradeNotes = PreTradeNotes,
            PostTradeNotes = PostTradeNotes,
            Mistakes = Mistakes,
            Lessons = Lessons,
            ExecutionRating = _executionRating > 0 ? _executionRating : null,
            FollowedPlan = FollowedPlan,
            IsImpulsive = IsImpulsive
            // IsClosed محاسبه‌شده است در DTO
        };
    }

    private TradeUpdateDto BuildUpdateDto(int accountId)
    {
        var hasExit = !string.IsNullOrEmpty(ExitPrice) && decimal.TryParse(ExitPrice, out _);
        
        return new TradeUpdateDto
        {
            Id = _tradeId!.Value,
            AccountId = accountId,
            Symbol = Symbol,
            Direction = Direction,
            Volume = ParseDecimal(Volume),
            EntryPrice = ParseDecimal(EntryPrice),
            ExitPrice = ParseNullableDecimal(ExitPrice),
            StopLoss = ParseNullableDecimal(StopLoss),
            TakeProfit = ParseNullableDecimal(TakeProfit),
            EntryTime = GetEntryDateTime(),
            ExitTime = GetExitDateTime(),
            Commission = ParseDecimal(Commission),
            Swap = ParseDecimal(Swap),
            EntryReason = EntryReason,
            PreTradeNotes = PreTradeNotes,
            PostTradeNotes = PostTradeNotes,
            Mistakes = Mistakes,
            Lessons = Lessons,
            ExecutionRating = _executionRating > 0 ? _executionRating : null,
            FollowedPlan = FollowedPlan,
            IsImpulsive = IsImpulsive,
            IsClosed = IsClosed || hasExit
        };
    }

    private bool ValidateForm()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(Symbol))
        {
            ShowError("نماد را وارد کنید");
            return false;
        }

        if (!decimal.TryParse(Volume, out var vol) || vol <= 0)
        {
            ShowError("حجم معتبر وارد کنید");
            return false;
        }

        if (!decimal.TryParse(EntryPrice, out var entry) || entry <= 0)
        {
            ShowError("قیمت ورود معتبر وارد کنید");
            return false;
        }

        if (!EntryDate.HasValue)
        {
            ShowError("تاریخ ورود را انتخاب کنید");
            return false;
        }

        // اعتبارسنجی SL برای Buy
        if (!string.IsNullOrEmpty(StopLoss) && decimal.TryParse(StopLoss, out var sl))
        {
            if (Direction == TradeDirection.Buy && sl >= entry)
            {
                ShowError("حد ضرر خرید باید کمتر از قیمت ورود باشد");
                return false;
            }
            if (Direction == TradeDirection.Sell && sl <= entry)
            {
                ShowError("حد ضرر فروش باید بیشتر از قیمت ورود باشد");
                return false;
            }
        }

        return true;
    }

    private void ClearForm()
    {
        _tradeId = null;
        Symbol = string.Empty;
        DirectionIndex = 0;
        Volume = "0.01";
        EntryPrice = string.Empty;
        ExitPrice = string.Empty;
        StopLoss = string.Empty;
        TakeProfit = string.Empty;
        EntryDate = DateTime.Today;
        EntryTimeText = DateTime.Now.ToString("HH:mm");
        ExitDate = null;
        ExitTimeText = string.Empty;
        Commission = "0";
        Swap = "0";
        EntryReason = string.Empty;
        PreTradeNotes = string.Empty;
        PostTradeNotes = string.Empty;
        Mistakes = string.Empty;
        Lessons = string.Empty;
        FollowedPlan = false;
        IsImpulsive = false;
        IsClosed = false;
        _executionRating = 0;
        _calculatedProfitLoss = 0;
        _calculatedPips = 0;
        _calculatedRR = null;

        OnPropertyChanged(nameof(PageTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(IsNewTrade));
        OnPropertyChanged(nameof(ProfitLossDisplay));
        OnPropertyChanged(nameof(PipsDisplay));
        OnPropertyChanged(nameof(RiskRewardDisplay));
        OnPropertyChanged(nameof(Rating1));
        OnPropertyChanged(nameof(Rating2));
        OnPropertyChanged(nameof(Rating3));
        OnPropertyChanged(nameof(Rating4));
        OnPropertyChanged(nameof(Rating5));
    }

    private void CalculateAll()
    {
        CalculateProfitLoss();
        CalculatePips();
        CalculateRiskReward();
    }

    private void CalculateProfitLoss()
    {
        if (!decimal.TryParse(EntryPrice, out var entry) ||
            !decimal.TryParse(ExitPrice, out var exit) ||
            !decimal.TryParse(Volume, out var vol) ||
            vol <= 0 || entry <= 0)
        {
            _calculatedProfitLoss = 0;
            OnPropertyChanged(nameof(ProfitLossDisplay));
            OnPropertyChanged(nameof(ProfitLossColor));
            return;
        }

        var priceDiff = Direction == TradeDirection.Buy ? exit - entry : entry - exit;
        var commission = ParseDecimal(Commission);
        var swap = ParseDecimal(Swap);

        // محاسبه برای فارکس استاندارد
        var pipValue = GetPipValue(Symbol);
        var pips = priceDiff / pipValue;
        
        // هر لات استاندارد تقریباً $10 به ازای هر پیپ
        _calculatedProfitLoss = (pips * vol * 10m) - commission - swap;

        OnPropertyChanged(nameof(ProfitLossDisplay));
        OnPropertyChanged(nameof(ProfitLossColor));
    }

    private void CalculatePips()
    {
        if (!decimal.TryParse(EntryPrice, out var entry) ||
            !decimal.TryParse(ExitPrice, out var exit) ||
            entry <= 0)
        {
            _calculatedPips = 0;
            OnPropertyChanged(nameof(PipsDisplay));
            OnPropertyChanged(nameof(PipsColor));
            return;
        }

        var priceDiff = Direction == TradeDirection.Buy ? exit - entry : entry - exit;
        var pipValue = GetPipValue(Symbol);
        _calculatedPips = Math.Round(priceDiff / pipValue, 1);

        OnPropertyChanged(nameof(PipsDisplay));
        OnPropertyChanged(nameof(PipsColor));
    }

    private void CalculateRiskReward()
    {
        if (!decimal.TryParse(EntryPrice, out var entry) ||
            !decimal.TryParse(StopLoss, out var sl) ||
            entry <= 0 || sl <= 0)
        {
            _calculatedRR = null;
            OnPropertyChanged(nameof(RiskRewardDisplay));
            return;
        }

        var risk = Math.Abs(entry - sl);
        if (risk == 0)
        {
            _calculatedRR = null;
            OnPropertyChanged(nameof(RiskRewardDisplay));
            return;
        }

        // اگر قیمت خروج وجود دارد
        if (decimal.TryParse(ExitPrice, out var exit) && exit > 0)
        {
            var reward = Direction == TradeDirection.Buy ? exit - entry : entry - exit;
            _calculatedRR = Math.Round(reward / risk, 2);
        }
        // اگر TP وجود دارد
        else if (decimal.TryParse(TakeProfit, out var tp) && tp > 0)
        {
            var reward = Direction == TradeDirection.Buy ? tp - entry : entry - tp;
            _calculatedRR = Math.Round(reward / risk, 2);
        }
        else
        {
            _calculatedRR = null;
        }

        OnPropertyChanged(nameof(RiskRewardDisplay));
    }

    private decimal GetPipValue(string symbol)
    {
        if (string.IsNullOrEmpty(symbol))
            return 0.0001m;

        symbol = symbol.ToUpperInvariant();

        if (symbol.Contains("JPY"))
            return 0.01m;

        if (symbol.Contains("XAU") || symbol.Contains("GOLD"))
            return 0.1m;

        if (symbol.Contains("XAG") || symbol.Contains("SILVER"))
            return 0.01m;

        if (symbol.Contains("US30") || symbol.Contains("SPX") || symbol.Contains("NAS"))
            return 1m;

        return 0.0001m;
    }

    private DateTime GetEntryDateTime()
    {
        var date = EntryDate ?? DateTime.Today;
        if (TimeSpan.TryParse(EntryTimeText, out var time))
        {
            return date.Add(time);
        }
        return date;
    }

    private DateTime? GetExitDateTime()
    {
        if (!ExitDate.HasValue)
            return null;

        var date = ExitDate.Value;
        if (!string.IsNullOrEmpty(ExitTimeText) && TimeSpan.TryParse(ExitTimeText, out var time))
        {
            return date.Add(time);
        }
        return date;
    }

    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;
        return decimal.TryParse(value, out var result) ? result : 0;
    }

    private decimal? ParseNullableDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return decimal.TryParse(value, out var result) ? result : null;
    }

    #endregion

    #region Lifecycle

    public override async Task InitializeAsync()
    {
        // اگر پارامتری از Navigation Engine آمده باشد
        var parameter = _stateEngine.Get<object?>("NavigationParameter");
        if (parameter != null)
        {
            await SetParameterAsync(parameter);
            _stateEngine.Remove("NavigationParameter");
        }
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/UI/ViewModels/TradeEntryViewModel.cs
// =============================================================================