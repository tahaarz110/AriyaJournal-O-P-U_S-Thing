// =============================================================================
// فایل: src/AriaJournal.Core/Application/Services/TradeService.cs
// توضیح: سرویس مدیریت معاملات - اصلاح‌شده برای سازگاری با IGenericRepository
// =============================================================================

using System.Text.Json;
using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Application.Validators;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// Event ایجاد معامله
/// </summary>
public record TradeCreatedEvent(int TradeId, int AccountId);

/// <summary>
/// Event بروزرسانی معامله
/// </summary>
public record TradeUpdatedEvent(int TradeId);

/// <summary>
/// Event حذف معامله
/// </summary>
public record TradeDeletedEvent(int TradeId);

/// <summary>
/// سرویس مدیریت معاملات
/// </summary>
public class TradeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly ICacheEngine _cacheEngine;
    private readonly TradeCreateValidator _createValidator;
    private readonly TradeCloseValidator _closeValidator;

    public TradeService(
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        ICacheEngine cacheEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));
        _createValidator = new TradeCreateValidator();
        _closeValidator = new TradeCloseValidator();
    }

    /// <summary>
    /// دریافت معاملات با فیلتر
    /// </summary>
    public async Task<Result<TradePagedResultDto>> GetTradesAsync(TradeFilterDto filter)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();

            // ساخت کوئری
            var query = tradeRepo.Query().Where(t => !t.IsDeleted);

            // اعمال فیلترها
            if (filter.AccountId.HasValue)
                query = query.Where(t => t.AccountId == filter.AccountId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Symbol))
                query = query.Where(t => t.Symbol.Contains(filter.Symbol));

            if (filter.Direction.HasValue)
                query = query.Where(t => t.Direction == filter.Direction.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.EntryTime >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.EntryTime <= filter.ToDate.Value);

            if (filter.IsClosed.HasValue)
                query = query.Where(t => t.IsClosed == filter.IsClosed.Value);

            if (filter.IsProfit.HasValue)
            {
                if (filter.IsProfit.Value)
                    query = query.Where(t => t.ProfitLoss > 0);
                else
                    query = query.Where(t => t.ProfitLoss < 0);
            }

            if (filter.MinRating.HasValue)
                query = query.Where(t => t.ExecutionRating >= filter.MinRating.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.ToLower();
                query = query.Where(t =>
                    t.Symbol.ToLower().Contains(search) ||
                    (t.EntryReason != null && t.EntryReason.ToLower().Contains(search)) ||
                    (t.PreTradeNotes != null && t.PreTradeNotes.ToLower().Contains(search)));
            }

            // شمارش کل
            var totalCount = query.Count();

            // محاسبه آمار
            var allFiltered = query.ToList();
            var totalProfitLoss = allFiltered.Where(t => t.ProfitLoss.HasValue).Sum(t => t.ProfitLoss!.Value);
            var closedTrades = allFiltered.Where(t => t.IsClosed && t.ProfitLoss.HasValue).ToList();
            var winCount = closedTrades.Count(t => t.ProfitLoss > 0);
            var lossCount = closedTrades.Count(t => t.ProfitLoss <= 0);

            // مرتب‌سازی
            query = filter.SortBy?.ToLower() switch
            {
                "symbol" => filter.SortDescending ? query.OrderByDescending(t => t.Symbol) : query.OrderBy(t => t.Symbol),
                "profitloss" => filter.SortDescending ? query.OrderByDescending(t => t.ProfitLoss) : query.OrderBy(t => t.ProfitLoss),
                "volume" => filter.SortDescending ? query.OrderByDescending(t => t.Volume) : query.OrderBy(t => t.Volume),
                _ => filter.SortDescending ? query.OrderByDescending(t => t.EntryTime) : query.OrderBy(t => t.EntryTime)
            };

            // صفحه‌بندی
            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var result = new TradePagedResultDto
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalProfitLoss = totalProfitLoss,
                WinCount = winCount,
                LossCount = lossCount
            };

            return await Task.FromResult(Result.Success(result));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت معاملات: {ex.Message}");
            return Result.Failure<TradePagedResultDto>(Error.Failure("خطا در دریافت لیست معاملات"));
        }
    }

    /// <summary>
    /// دریافت معامله با شناسه
    /// </summary>
    public async Task<Result<TradeDto>> GetByIdAsync(int tradeId)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(tradeId);

            if (trade == null || trade.IsDeleted)
            {
                return Result.Failure<TradeDto>(Error.TradeNotFound);
            }

            var dto = MapToDto(trade);

            // بارگذاری فیلدهای سفارشی
            var customFieldRepo = _unitOfWork.Repository<TradeCustomField>();
            var customFields = await customFieldRepo.GetAllAsync(cf => cf.TradeId == tradeId);

            var fieldDefRepo = _unitOfWork.Repository<FieldDefinition>();
            foreach (var cf in customFields)
            {
                var fieldDef = await fieldDefRepo.GetByIdAsync(cf.FieldDefinitionId);
                var fieldName = fieldDef?.FieldName ?? cf.FieldDefinitionId.ToString();
                dto.CustomFields[fieldName] = cf.Value;
            }

            // شمارش اسکرین‌شات‌ها
            var screenshotRepo = _unitOfWork.Repository<Screenshot>();
            dto.ScreenshotsCount = await screenshotRepo.CountAsync(s => s.TradeId == tradeId);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت معامله: {ex.Message}");
            return Result.Failure<TradeDto>(Error.Failure("خطا در دریافت اطلاعات معامله"));
        }
    }

    /// <summary>
    /// ایجاد معامله جدید
    /// </summary>
    public async Task<Result<TradeDto>> CreateAsync(TradeCreateDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TradeDto>(Error.Validation(errors));
        }

        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();

            var trade = new Trade
            {
                AccountId = dto.AccountId,
                Ticket = dto.Ticket,
                Symbol = dto.Symbol.ToUpperInvariant(),
                Direction = dto.Direction,
                Volume = dto.Volume,
                EntryPrice = dto.EntryPrice,
                ExitPrice = dto.ExitPrice,
                StopLoss = dto.StopLoss,
                TakeProfit = dto.TakeProfit,
                EntryTime = dto.EntryTime,
                ExitTime = dto.ExitTime,
                Commission = dto.Commission,
                Swap = dto.Swap,
                PreTradeNotes = dto.PreTradeNotes,
                PostTradeNotes = dto.PostTradeNotes,
                EntryReason = dto.EntryReason,
                ExitReason = dto.ExitReason,
                Mistakes = dto.Mistakes,
                Lessons = dto.Lessons,
                ExecutionRating = dto.ExecutionRating,
                IsImpulsive = dto.IsImpulsive,
                FollowedPlan = dto.FollowedPlan,
                IsClosed = dto.IsClosed,
                CreatedAt = DateTime.Now
            };

            // محاسبه سود/زیان
            if (trade.IsClosed && trade.ExitPrice.HasValue)
            {
                trade.ProfitLoss = CalculateProfitLoss(trade);
                trade.ProfitLossPips = CalculatePips(trade);
                trade.RiskRewardRatio = CalculateRR(trade);
            }

            await tradeRepo.AddAsync(trade);
            await _unitOfWork.SaveChangesAsync();

            // ذخیره فیلدهای سفارشی
            if (dto.CustomFields.Any())
            {
                await SaveCustomFieldsAsync(trade.Id, dto.CustomFields);
            }

            _eventBus.Publish(new TradeCreatedEvent(trade.Id, trade.AccountId));

            // پاک کردن کش
            _cacheEngine.Remove($"trades:account:{dto.AccountId}");

            return Result.Success(MapToDto(trade));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد معامله: {ex.Message}");
            return Result.Failure<TradeDto>(Error.Failure("خطا در ذخیره معامله"));
        }
    }

    /// <summary>
    /// ویرایش معامله
    /// </summary>
    public async Task<Result<TradeDto>> UpdateAsync(TradeUpdateDto dto)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(dto.Id);

            if (trade == null || trade.IsDeleted)
            {
                return Result.Failure<TradeDto>(Error.TradeNotFound);
            }

            trade.AccountId = dto.AccountId;
            trade.Ticket = dto.Ticket;
            trade.Symbol = dto.Symbol.ToUpperInvariant();
            trade.Direction = dto.Direction;
            trade.Volume = dto.Volume;
            trade.EntryPrice = dto.EntryPrice;
            trade.ExitPrice = dto.ExitPrice;
            trade.StopLoss = dto.StopLoss;
            trade.TakeProfit = dto.TakeProfit;
            trade.EntryTime = dto.EntryTime;
            trade.ExitTime = dto.ExitTime;
            trade.Commission = dto.Commission;
            trade.Swap = dto.Swap;
            trade.PreTradeNotes = dto.PreTradeNotes;
            trade.PostTradeNotes = dto.PostTradeNotes;
            trade.EntryReason = dto.EntryReason;
            trade.ExitReason = dto.ExitReason;
            trade.Mistakes = dto.Mistakes;
            trade.Lessons = dto.Lessons;
            trade.ExecutionRating = dto.ExecutionRating;
            trade.IsImpulsive = dto.IsImpulsive;
            trade.FollowedPlan = dto.FollowedPlan;
            trade.IsClosed = dto.IsClosed;
            trade.UpdatedAt = DateTime.Now;

            // محاسبه مجدد سود/زیان
            if (trade.IsClosed && trade.ExitPrice.HasValue)
            {
                trade.ProfitLoss = CalculateProfitLoss(trade);
                trade.ProfitLossPips = CalculatePips(trade);
                trade.RiskRewardRatio = CalculateRR(trade);
            }

            tradeRepo.Update(trade);
            await _unitOfWork.SaveChangesAsync();

            // بروزرسانی فیلدهای سفارشی
            if (dto.CustomFields.Any())
            {
                await SaveCustomFieldsAsync(trade.Id, dto.CustomFields);
            }

            _eventBus.Publish(new TradeUpdatedEvent(trade.Id));

            // پاک کردن کش
            _cacheEngine.Remove($"trades:account:{trade.AccountId}");

            return Result.Success(MapToDto(trade));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ویرایش معامله: {ex.Message}");
            return Result.Failure<TradeDto>(Error.Failure("خطا در ذخیره معامله"));
        }
    }

    /// <summary>
    /// بستن معامله
    /// </summary>
    public async Task<Result<TradeDto>> CloseTradeAsync(TradeCloseDto dto)
    {
        // اعتبارسنجی
        var validationResult = await _closeValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TradeDto>(Error.Validation(errors));
        }

        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(dto.TradeId);

            if (trade == null || trade.IsDeleted)
            {
                return Result.Failure<TradeDto>(Error.TradeNotFound);
            }

            if (trade.IsClosed)
            {
                return Result.Failure<TradeDto>(Error.Validation("این معامله قبلاً بسته شده است"));
            }

            trade.ExitPrice = dto.ExitPrice;
            trade.ExitTime = dto.ExitTime;
            trade.ExitReason = dto.ExitReason;
            trade.IsClosed = true;
            trade.UpdatedAt = DateTime.Now;

            if (dto.Commission.HasValue)
                trade.Commission = dto.Commission.Value;
            if (dto.Swap.HasValue)
                trade.Swap = dto.Swap.Value;

            // محاسبه سود/زیان
            trade.ProfitLoss = CalculateProfitLoss(trade);
            trade.ProfitLossPips = CalculatePips(trade);
            trade.RiskRewardRatio = CalculateRR(trade);

            tradeRepo.Update(trade);
            await _unitOfWork.SaveChangesAsync();

            _eventBus.Publish(new TradeUpdatedEvent(trade.Id));

            return Result.Success(MapToDto(trade));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بستن معامله: {ex.Message}");
            return Result.Failure<TradeDto>(Error.Failure("خطا در ذخیره معامله"));
        }
    }

    /// <summary>
    /// حذف معامله (انتقال به سطل زباله)
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int tradeId)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(tradeId);

            if (trade == null)
            {
                return Result.Failure<bool>(Error.TradeNotFound);
            }

            trade.IsDeleted = true;
            trade.DeletedAt = DateTime.Now;

            tradeRepo.Update(trade);
            await _unitOfWork.SaveChangesAsync();

            _eventBus.Publish(new TradeDeletedEvent(tradeId));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در حذف معامله: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در حذف معامله"));
        }
    }

    /// <summary>
    /// بازیابی معامله از سطل زباله
    /// </summary>
    public async Task<Result<bool>> RestoreAsync(int tradeId)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var trade = await tradeRepo.GetByIdAsync(tradeId);

            if (trade == null)
            {
                return Result.Failure<bool>(Error.TradeNotFound);
            }

            trade.IsDeleted = false;
            trade.DeletedAt = null;
            trade.UpdatedAt = DateTime.Now;

            tradeRepo.Update(trade);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بازیابی معامله: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در بازیابی معامله"));
        }
    }

    /// <summary>
    /// دریافت نمادهای استفاده شده
    /// </summary>
    public async Task<Result<List<string>>> GetUsedSymbolsAsync(int accountId)
    {
        try
        {
            var tradeRepo = _unitOfWork.Repository<Trade>();
            var symbols = tradeRepo.Query()
                .Where(t => t.AccountId == accountId && !t.IsDeleted)
                .Select(t => t.Symbol)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return await Task.FromResult(Result.Success(symbols));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت نمادها: {ex.Message}");
            return Result.Failure<List<string>>(Error.Failure("خطا در دریافت نمادها"));
        }
    }

    #region Private Methods

    private static TradeDto MapToDto(Trade trade)
    {
        return new TradeDto
        {
            Id = trade.Id,
            AccountId = trade.AccountId,
            AccountName = trade.Account?.Name ?? "",
            Ticket = trade.Ticket,
            Symbol = trade.Symbol,
            Direction = trade.Direction,
            Volume = trade.Volume,
            EntryPrice = trade.EntryPrice,
            ExitPrice = trade.ExitPrice,
            StopLoss = trade.StopLoss,
            TakeProfit = trade.TakeProfit,
            EntryTime = trade.EntryTime,
            ExitTime = trade.ExitTime,
            Commission = trade.Commission,
            Swap = trade.Swap,
            ProfitLoss = trade.ProfitLoss,
            ProfitLossPips = trade.ProfitLossPips,
            RiskRewardRatio = trade.RiskRewardRatio,
            RiskPercent = trade.RiskPercent,
            IsClosed = trade.IsClosed,
            PreTradeNotes = trade.PreTradeNotes,
            PostTradeNotes = trade.PostTradeNotes,
            EntryReason = trade.EntryReason,
            ExitReason = trade.ExitReason,
            Mistakes = trade.Mistakes,
            Lessons = trade.Lessons,
            ExecutionRating = trade.ExecutionRating,
            IsImpulsive = trade.IsImpulsive,
            FollowedPlan = trade.FollowedPlan,
            CreatedAt = trade.CreatedAt
        };
    }

    private decimal CalculateProfitLoss(Trade trade)
    {
        if (!trade.ExitPrice.HasValue) return 0;

        var priceDiff = trade.Direction == TradeDirection.Buy
            ? trade.ExitPrice.Value - trade.EntryPrice
            : trade.EntryPrice - trade.ExitPrice.Value;

        // محاسبه ساده (بدون در نظر گرفتن pip value دقیق)
        var pipValue = GetPipValue(trade.Symbol);
        var pips = priceDiff / pipValue;
        var profit = pips * trade.Volume * 10; // تقریبی برای استاندارد لات

        return profit - trade.Commission - trade.Swap;
    }

    private decimal CalculatePips(Trade trade)
    {
        if (!trade.ExitPrice.HasValue) return 0;

        var priceDiff = trade.Direction == TradeDirection.Buy
            ? trade.ExitPrice.Value - trade.EntryPrice
            : trade.EntryPrice - trade.ExitPrice.Value;

        var pipValue = GetPipValue(trade.Symbol);
        return Math.Round(priceDiff / pipValue, 1);
    }

    private decimal? CalculateRR(Trade trade)
    {
        if (!trade.StopLoss.HasValue || !trade.ExitPrice.HasValue)
            return null;

        var risk = Math.Abs(trade.EntryPrice - trade.StopLoss.Value);
        if (risk == 0) return null;

        var reward = trade.Direction == TradeDirection.Buy
            ? trade.ExitPrice.Value - trade.EntryPrice
            : trade.EntryPrice - trade.ExitPrice.Value;

        return Math.Round(reward / risk, 2);
    }

    private decimal GetPipValue(string symbol)
    {
        // JPY pairs و طلا pip value متفاوت دارند
        symbol = symbol.ToUpperInvariant();

        if (symbol.Contains("JPY"))
            return 0.01m;

        if (symbol.Contains("XAU") || symbol.Contains("GOLD"))
            return 0.1m;

        return 0.0001m;
    }

    private async Task SaveCustomFieldsAsync(int tradeId, Dictionary<string, object?> customFields)
    {
        var customFieldRepo = _unitOfWork.Repository<TradeCustomField>();
        var fieldDefRepo = _unitOfWork.Repository<FieldDefinition>();

        // حذف فیلدهای قبلی
        var existingFields = await customFieldRepo.GetAllAsync(cf => cf.TradeId == tradeId);
        foreach (var existing in existingFields)
        {
            customFieldRepo.Delete(existing);
        }

        // افزودن فیلدهای جدید
        foreach (var kvp in customFields)
        {
            if (kvp.Value == null) continue;

            // یافتن تعریف فیلد
            var fieldDef = await fieldDefRepo.FirstOrDefaultAsync(f => f.FieldName == kvp.Key);
            if (fieldDef == null) continue;

            var customField = new TradeCustomField
            {
                TradeId = tradeId,
                FieldDefinitionId = fieldDef.Id,
                Value = kvp.Value.ToString(),
                CreatedAt = DateTime.Now
            };

            await customFieldRepo.AddAsync(customField);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Application/Services/TradeService.cs
// =============================================================================