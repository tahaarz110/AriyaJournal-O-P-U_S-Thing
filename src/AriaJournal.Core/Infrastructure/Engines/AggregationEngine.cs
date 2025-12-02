// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/AggregationEngine.cs
// توضیح: موتور محاسبات تجمیعی و آماری - نسخه اصلاح‌شده
// =============================================================================

using System.Collections.Concurrent;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces.Engines;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// اینترفیس موتور محاسبات تجمیعی
/// </summary>
public interface IAggregationEngine
{
    Task<TradeStatistics> CalculateStatisticsAsync(IEnumerable<Trade> trades);
    Task<Dictionary<string, TradeStatistics>> CalculateGroupedStatisticsAsync(IEnumerable<Trade> trades, Func<Trade, string> groupBy);
    Task<TimeDistribution> CalculateTimeDistributionAsync(IEnumerable<Trade> trades);
    Task<List<EquityPoint>> CalculateEquityCurveAsync(IEnumerable<Trade> trades, decimal initialBalance);
    Task<DrawdownInfo> CalculateDrawdownAsync(IEnumerable<Trade> trades, decimal initialBalance);
    Task<PerformanceMetrics> CalculatePerformanceMetricsAsync(IEnumerable<Trade> trades, decimal initialBalance);
    Task<Dictionary<string, SymbolStatistics>> CalculateSymbolStatisticsAsync(IEnumerable<Trade> trades);
    Task<List<PeriodStatistics>> CalculatePeriodStatisticsAsync(IEnumerable<Trade> trades, PeriodType periodType);
}

#region Data Models

public class TradeStatistics
{
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public int BreakEvenTrades { get; set; }
    public decimal WinRate => TotalTrades > 0 ? (decimal)WinningTrades / TotalTrades * 100 : 0;
    public decimal LossRate => TotalTrades > 0 ? (decimal)LosingTrades / TotalTrades * 100 : 0;
    public decimal TotalProfit { get; set; }
    public decimal TotalLoss { get; set; }
    public decimal NetProfitLoss => TotalProfit + TotalLoss;
    public decimal GrossProfit { get; set; }
    public decimal GrossLoss { get; set; }
    public decimal ProfitFactor => GrossLoss != 0 ? Math.Abs(GrossProfit / GrossLoss) : 0;
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal AverageTrade { get; set; }
    public decimal LargestWin { get; set; }
    public decimal LargestLoss { get; set; }
    public decimal AverageRR { get; set; }
    public decimal ExpectedValue { get; set; }
    public int MaxConsecutiveWins { get; set; }
    public int MaxConsecutiveLosses { get; set; }
    public decimal AverageHoldingTime { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalSwap { get; set; }
}

public class TimeDistribution
{
    public Dictionary<int, int> ByHour { get; set; } = new();
    public Dictionary<DayOfWeek, int> ByDayOfWeek { get; set; } = new();
    public Dictionary<int, int> ByMonth { get; set; } = new();
    public Dictionary<string, int> BySession { get; set; } = new();
    public Dictionary<int, decimal> ProfitByHour { get; set; } = new();
    public Dictionary<DayOfWeek, decimal> ProfitByDayOfWeek { get; set; } = new();
    public Dictionary<string, decimal> ProfitBySession { get; set; } = new();
    public Dictionary<int, decimal> WinRateByHour { get; set; } = new();
    public Dictionary<DayOfWeek, decimal> WinRateByDayOfWeek { get; set; } = new();
}

public class EquityPoint
{
    public DateTime Date { get; set; }
    public decimal Balance { get; set; }
    public decimal Equity { get; set; }
    public decimal ProfitLoss { get; set; }
    public int TradeNumber { get; set; }
}

public class DrawdownInfo
{
    public decimal MaxDrawdown { get; set; }
    public decimal MaxDrawdownPercent { get; set; }
    public DateTime MaxDrawdownDate { get; set; }
    public decimal CurrentDrawdown { get; set; }
    public decimal CurrentDrawdownPercent { get; set; }
    public int MaxDrawdownDuration { get; set; }
    public List<DrawdownPeriod> DrawdownPeriods { get; set; } = new();
}

public class DrawdownPeriod
{
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal PeakBalance { get; set; }
    public decimal TroughBalance { get; set; }
    public decimal DrawdownAmount => PeakBalance - TroughBalance;
    public decimal DrawdownPercent => PeakBalance > 0 ? DrawdownAmount / PeakBalance * 100 : 0;
    public int DurationDays => EndDate.HasValue ? (EndDate.Value - StartDate).Days : (DateTime.Now - StartDate).Days;
}

public class PerformanceMetrics
{
    public decimal SharpeRatio { get; set; }
    public decimal SortinoRatio { get; set; }
    public decimal CalmarRatio { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public decimal AnnualizedReturn { get; set; }
    public decimal Volatility { get; set; }
    public decimal RecoveryFactor { get; set; }
    public decimal PayoffRatio { get; set; }
    public decimal KellyPercent { get; set; }
    public decimal ZScore { get; set; }
}

public class SymbolStatistics
{
    public string Symbol { get; set; } = string.Empty;
    public int TradeCount { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal AverageRR { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal AverageHoldingTime { get; set; }
}

public class PeriodStatistics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int TradeCount { get; set; }
    public decimal WinRate { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal StartBalance { get; set; }
    public decimal EndBalance { get; set; }
    public decimal ReturnPercent { get; set; }
}

public enum PeriodType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

#endregion

/// <summary>
/// پیاده‌سازی موتور محاسبات تجمیعی
/// </summary>
public class AggregationEngine : IAggregationEngine
{
    private readonly ICacheEngine _cacheEngine;

    public AggregationEngine(ICacheEngine cacheEngine)
    {
        _cacheEngine = cacheEngine;
    }

    #region Main Statistics

    public async Task<TradeStatistics> CalculateStatisticsAsync(IEnumerable<Trade> trades)
    {
        return await Task.Run(() =>
        {
            var tradeList = trades.Where(t => t.IsClosed).OrderBy(t => t.ExitTime).ToList();

            if (!tradeList.Any())
                return new TradeStatistics();

            var stats = new TradeStatistics
            {
                TotalTrades = tradeList.Count,
                WinningTrades = tradeList.Count(t => (t.ProfitLoss ?? 0) > 0),
                LosingTrades = tradeList.Count(t => (t.ProfitLoss ?? 0) < 0),
                BreakEvenTrades = tradeList.Count(t => (t.ProfitLoss ?? 0) == 0),

                TotalProfit = tradeList.Where(t => (t.ProfitLoss ?? 0) > 0).Sum(t => t.ProfitLoss ?? 0),
                TotalLoss = tradeList.Where(t => (t.ProfitLoss ?? 0) < 0).Sum(t => t.ProfitLoss ?? 0),

                GrossProfit = tradeList.Where(t => (t.ProfitLoss ?? 0) > 0).Sum(t => t.ProfitLoss ?? 0),
                GrossLoss = Math.Abs(tradeList.Where(t => (t.ProfitLoss ?? 0) < 0).Sum(t => t.ProfitLoss ?? 0)),

                LargestWin = tradeList.Any() ? tradeList.Max(t => t.ProfitLoss ?? 0) : 0,
                LargestLoss = tradeList.Any() ? tradeList.Min(t => t.ProfitLoss ?? 0) : 0,

                TotalVolume = tradeList.Sum(t => t.Volume),
                TotalCommission = tradeList.Sum(t => t.Commission ?? 0),
                TotalSwap = tradeList.Sum(t => t.Swap ?? 0)
            };

            // میانگین‌ها
            var winningTrades = tradeList.Where(t => (t.ProfitLoss ?? 0) > 0).ToList();
            var losingTrades = tradeList.Where(t => (t.ProfitLoss ?? 0) < 0).ToList();

            stats.AverageWin = winningTrades.Any() ? winningTrades.Average(t => t.ProfitLoss ?? 0) : 0;
            stats.AverageLoss = losingTrades.Any() ? losingTrades.Average(t => t.ProfitLoss ?? 0) : 0;
            stats.AverageTrade = tradeList.Any() ? tradeList.Average(t => t.ProfitLoss ?? 0) : 0;

            // R:R میانگین
            var tradesWithRR = tradeList.Where(t => t.StopLoss.HasValue && t.EntryPrice > 0).ToList();
            if (tradesWithRR.Any())
            {
                stats.AverageRR = tradesWithRR.Average(t => CalculateRR(t));
            }

            // Expected Value
            stats.ExpectedValue = (stats.WinRate / 100 * stats.AverageWin) +
                                  ((100 - stats.WinRate) / 100 * stats.AverageLoss);

            // معاملات متوالی
            CalculateConsecutiveStats(tradeList, stats);

            // میانگین زمان نگهداری
            var tradesWithTime = tradeList.Where(t => t.EntryTime.HasValue && t.ExitTime.HasValue).ToList();
            if (tradesWithTime.Any())
            {
                stats.AverageHoldingTime = (decimal)tradesWithTime
                    .Average(t => (t.ExitTime!.Value - t.EntryTime!.Value).TotalMinutes);
            }

            return stats;
        });
    }

    private decimal CalculateRR(Trade trade)
    {
        if (!trade.StopLoss.HasValue || trade.EntryPrice <= 0)
            return 0;

        var risk = Math.Abs(trade.EntryPrice - trade.StopLoss.Value);
        if (risk == 0) return 0;

        var reward = trade.ProfitLoss ?? 0;
        var riskInCurrency = risk * trade.Volume * 100000m;
        if (riskInCurrency == 0) return 0;

        return reward / riskInCurrency;
    }

    private void CalculateConsecutiveStats(List<Trade> trades, TradeStatistics stats)
    {
        int currentWins = 0, currentLosses = 0;
        int maxWins = 0, maxLosses = 0;

        foreach (var trade in trades)
        {
            if ((trade.ProfitLoss ?? 0) > 0)
            {
                currentWins++;
                currentLosses = 0;
                maxWins = Math.Max(maxWins, currentWins);
            }
            else if ((trade.ProfitLoss ?? 0) < 0)
            {
                currentLosses++;
                currentWins = 0;
                maxLosses = Math.Max(maxLosses, currentLosses);
            }
        }

        stats.MaxConsecutiveWins = maxWins;
        stats.MaxConsecutiveLosses = maxLosses;
    }

    #endregion

    #region Grouped Statistics

    public async Task<Dictionary<string, TradeStatistics>> CalculateGroupedStatisticsAsync(
        IEnumerable<Trade> trades,
        Func<Trade, string> groupBy)
    {
        var result = new Dictionary<string, TradeStatistics>();
        var groups = trades.GroupBy(groupBy);

        foreach (var group in groups)
        {
            var stats = await CalculateStatisticsAsync(group);
            result[group.Key] = stats;
        }

        return result;
    }

    #endregion

    #region Time Distribution

    public async Task<TimeDistribution> CalculateTimeDistributionAsync(IEnumerable<Trade> trades)
    {
        return await Task.Run(() =>
        {
            var tradeList = trades.Where(t => t.EntryTime.HasValue).ToList();
            var distribution = new TimeDistribution();

            // توزیع بر اساس ساعت
            for (int h = 0; h < 24; h++)
            {
                var hourTrades = tradeList.Where(t => t.EntryTime!.Value.Hour == h).ToList();
                distribution.ByHour[h] = hourTrades.Count;
                distribution.ProfitByHour[h] = hourTrades.Sum(t => t.ProfitLoss ?? 0);

                var winCount = hourTrades.Count(t => (t.ProfitLoss ?? 0) > 0);
                distribution.WinRateByHour[h] = hourTrades.Any()
                    ? (decimal)winCount / hourTrades.Count * 100
                    : 0;
            }

            // توزیع بر اساس روز هفته
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var dayTrades = tradeList.Where(t => t.EntryTime!.Value.DayOfWeek == day).ToList();
                distribution.ByDayOfWeek[day] = dayTrades.Count;
                distribution.ProfitByDayOfWeek[day] = dayTrades.Sum(t => t.ProfitLoss ?? 0);

                var winCount = dayTrades.Count(t => (t.ProfitLoss ?? 0) > 0);
                distribution.WinRateByDayOfWeek[day] = dayTrades.Any()
                    ? (decimal)winCount / dayTrades.Count * 100
                    : 0;
            }

            // توزیع بر اساس ماه
            for (int m = 1; m <= 12; m++)
            {
                var monthTrades = tradeList.Where(t => t.EntryTime!.Value.Month == m).ToList();
                distribution.ByMonth[m] = monthTrades.Count;
            }

            // توزیع بر اساس Session
            var sessions = new[] { "London", "NewYork", "Asian", "Sydney" };
            foreach (var session in sessions)
            {
                var sessionTrades = tradeList.Where(t => GetSession(t.EntryTime!.Value) == session).ToList();
                distribution.BySession[session] = sessionTrades.Count;
                distribution.ProfitBySession[session] = sessionTrades.Sum(t => t.ProfitLoss ?? 0);
            }

            return distribution;
        });
    }

    private string GetSession(DateTime time)
    {
        var hour = time.Hour;
        return hour switch
        {
            >= 0 and < 7 => "Asian",
            >= 7 and < 8 => "Sydney",
            >= 8 and < 12 => "London",
            >= 12 and < 21 => "NewYork",
            _ => "Asian"
        };
    }

    #endregion

    #region Equity Curve

    public async Task<List<EquityPoint>> CalculateEquityCurveAsync(IEnumerable<Trade> trades, decimal initialBalance)
    {
        return await Task.Run(() =>
        {
            var points = new List<EquityPoint>();
            var balance = initialBalance;
            var tradeNumber = 0;

            points.Add(new EquityPoint
            {
                Date = DateTime.Now.AddYears(-1),
                Balance = balance,
                Equity = balance,
                ProfitLoss = 0,
                TradeNumber = 0
            });

            var sortedTrades = trades
                .Where(t => t.IsClosed && t.ExitTime.HasValue)
                .OrderBy(t => t.ExitTime)
                .ToList();

            foreach (var trade in sortedTrades)
            {
                tradeNumber++;
                var pl = trade.ProfitLoss ?? 0;
                balance += pl;

                points.Add(new EquityPoint
                {
                    Date = trade.ExitTime!.Value,
                    Balance = balance,
                    Equity = balance,
                    ProfitLoss = pl,
                    TradeNumber = tradeNumber
                });
            }

            return points;
        });
    }

    #endregion

    #region Drawdown

    public async Task<DrawdownInfo> CalculateDrawdownAsync(IEnumerable<Trade> trades, decimal initialBalance)
    {
        return await Task.Run(() =>
        {
            var info = new DrawdownInfo();
            var equity = initialBalance;
            var peak = initialBalance;
            DateTime? currentDrawdownStart = null;
            var currentPeak = initialBalance;

            var sortedTrades = trades
                .Where(t => t.IsClosed && t.ExitTime.HasValue)
                .OrderBy(t => t.ExitTime)
                .ToList();

            foreach (var trade in sortedTrades)
            {
                equity += trade.ProfitLoss ?? 0;

                if (equity > peak)
                {
                    if (currentDrawdownStart.HasValue)
                    {
                        info.DrawdownPeriods.Add(new DrawdownPeriod
                        {
                            StartDate = currentDrawdownStart.Value,
                            EndDate = trade.ExitTime!.Value,
                            PeakBalance = currentPeak,
                            TroughBalance = peak
                        });
                        currentDrawdownStart = null;
                    }

                    peak = equity;
                    currentPeak = equity;
                }
                else
                {
                    if (!currentDrawdownStart.HasValue)
                    {
                        currentDrawdownStart = trade.ExitTime!.Value;
                    }

                    var drawdown = peak - equity;
                    var drawdownPercent = peak > 0 ? drawdown / peak * 100 : 0;

                    if (drawdown > info.MaxDrawdown)
                    {
                        info.MaxDrawdown = drawdown;
                        info.MaxDrawdownPercent = drawdownPercent;
                        info.MaxDrawdownDate = trade.ExitTime!.Value;
                    }
                }
            }

            info.CurrentDrawdown = peak - equity;
            info.CurrentDrawdownPercent = peak > 0 ? info.CurrentDrawdown / peak * 100 : 0;

            if (info.DrawdownPeriods.Any())
            {
                info.MaxDrawdownDuration = info.DrawdownPeriods.Max(p => p.DurationDays);
            }

            return info;
        });
    }

    #endregion

    #region Performance Metrics

    public async Task<PerformanceMetrics> CalculatePerformanceMetricsAsync(IEnumerable<Trade> trades, decimal initialBalance)
    {
        return await Task.Run(async () =>
        {
            var metrics = new PerformanceMetrics();
            var tradeList = trades.Where(t => t.IsClosed).ToList();

            if (!tradeList.Any())
                return metrics;

            var stats = await CalculateStatisticsAsync(tradeList);
            var drawdown = await CalculateDrawdownAsync(tradeList, initialBalance);
            var equityCurve = await CalculateEquityCurveAsync(tradeList, initialBalance);

            var finalBalance = equityCurve.LastOrDefault()?.Balance ?? initialBalance;
            metrics.ReturnOnInvestment = ((finalBalance - initialBalance) / initialBalance) * 100;

            // Annualized Return
            var orderedTrades = tradeList.Where(t => t.EntryTime.HasValue).OrderBy(t => t.EntryTime).ToList();
            if (orderedTrades.Any())
            {
                var firstTrade = orderedTrades.First();
                var lastTrade = tradeList.Where(t => t.ExitTime.HasValue).OrderBy(t => t.ExitTime).LastOrDefault();

                if (firstTrade.EntryTime.HasValue && lastTrade?.ExitTime != null)
                {
                    var years = (lastTrade.ExitTime.Value - firstTrade.EntryTime.Value).TotalDays / 365.0;
                    if (years > 0 && finalBalance > 0 && initialBalance > 0)
                    {
                        metrics.AnnualizedReturn = (decimal)(Math.Pow((double)(finalBalance / initialBalance), 1.0 / years) - 1) * 100;
                    }
                }
            }

            // Volatility
            var returns = tradeList.Select(t => (t.ProfitLoss ?? 0) / initialBalance * 100).ToList();
            if (returns.Any())
            {
                var avgReturn = returns.Average();
                var sumSquares = returns.Sum(r => (r - avgReturn) * (r - avgReturn));
                metrics.Volatility = (decimal)Math.Sqrt((double)(sumSquares / returns.Count));
            }

            // Sharpe Ratio
            if (metrics.Volatility > 0)
            {
                var avgDailyReturn = metrics.AnnualizedReturn / 252;
                metrics.SharpeRatio = avgDailyReturn / metrics.Volatility * (decimal)Math.Sqrt(252);
            }

            // Sortino Ratio
            var negativeReturns = returns.Where(r => r < 0).ToList();
            if (negativeReturns.Any())
            {
                var sumNegSquares = negativeReturns.Sum(r => r * r);
                var downside = (decimal)Math.Sqrt((double)(sumNegSquares / negativeReturns.Count));
                if (downside > 0)
                {
                    metrics.SortinoRatio = metrics.AnnualizedReturn / downside;
                }
            }

            // Calmar Ratio
            if (drawdown.MaxDrawdownPercent > 0)
            {
                metrics.CalmarRatio = metrics.AnnualizedReturn / drawdown.MaxDrawdownPercent;
            }

            // Recovery Factor
            if (drawdown.MaxDrawdown > 0)
            {
                metrics.RecoveryFactor = stats.NetProfitLoss / drawdown.MaxDrawdown;
            }

            // Payoff Ratio
            if (stats.AverageLoss != 0)
            {
                metrics.PayoffRatio = Math.Abs(stats.AverageWin / stats.AverageLoss);
            }

            // Kelly Percent
            var winProb = stats.WinRate / 100;
            var lossProb = 1 - winProb;
            if (metrics.PayoffRatio > 0 && lossProb > 0)
            {
                metrics.KellyPercent = (winProb - (lossProb / metrics.PayoffRatio)) * 100;
            }

            return metrics;
        });
    }

    #endregion

    #region Symbol Statistics

    public async Task<Dictionary<string, SymbolStatistics>> CalculateSymbolStatisticsAsync(IEnumerable<Trade> trades)
    {
        return await Task.Run(() =>
        {
            var result = new Dictionary<string, SymbolStatistics>();
            var grouped = trades.Where(t => !string.IsNullOrEmpty(t.Symbol)).GroupBy(t => t.Symbol);

            foreach (var group in grouped)
            {
                var symbol = group.Key;
                var symbolTrades = group.ToList();
                var winCount = symbolTrades.Count(t => (t.ProfitLoss ?? 0) > 0);

                var stats = new SymbolStatistics
                {
                    Symbol = symbol,
                    TradeCount = symbolTrades.Count,
                    WinRate = symbolTrades.Any() ? (decimal)winCount / symbolTrades.Count * 100 : 0,
                    TotalProfitLoss = symbolTrades.Sum(t => t.ProfitLoss ?? 0),
                    TotalVolume = symbolTrades.Sum(t => t.Volume)
                };

                var tradesWithRR = symbolTrades.Where(t => t.StopLoss.HasValue).ToList();
                if (tradesWithRR.Any())
                {
                    stats.AverageRR = tradesWithRR.Average(t => CalculateRR(t));
                }

                var tradesWithTime = symbolTrades.Where(t => t.EntryTime.HasValue && t.ExitTime.HasValue).ToList();
                if (tradesWithTime.Any())
                {
                    stats.AverageHoldingTime = (decimal)tradesWithTime
                        .Average(t => (t.ExitTime!.Value - t.EntryTime!.Value).TotalMinutes);
                }

                result[symbol] = stats;
            }

            return result;
        });
    }

    #endregion

    #region Period Statistics

    public async Task<List<PeriodStatistics>> CalculatePeriodStatisticsAsync(
        IEnumerable<Trade> trades,
        PeriodType periodType)
    {
        return await Task.Run(() =>
        {
            var result = new List<PeriodStatistics>();
            var sortedTrades = trades
                .Where(t => t.IsClosed && t.ExitTime.HasValue)
                .OrderBy(t => t.ExitTime)
                .ToList();

            if (!sortedTrades.Any())
                return result;

            var grouped = periodType switch
            {
                PeriodType.Daily => sortedTrades.GroupBy(t => t.ExitTime!.Value.Date),
                PeriodType.Weekly => sortedTrades.GroupBy(t => GetWeekStart(t.ExitTime!.Value)),
                PeriodType.Monthly => sortedTrades.GroupBy(t => new DateTime(t.ExitTime!.Value.Year, t.ExitTime!.Value.Month, 1)),
                PeriodType.Quarterly => sortedTrades.GroupBy(t => GetQuarterStart(t.ExitTime!.Value)),
                PeriodType.Yearly => sortedTrades.GroupBy(t => new DateTime(t.ExitTime!.Value.Year, 1, 1)),
                _ => sortedTrades.GroupBy(t => t.ExitTime!.Value.Date)
            };

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                var periodTrades = group.ToList();
                var winCount = periodTrades.Count(t => (t.ProfitLoss ?? 0) > 0);
                var profitLoss = periodTrades.Sum(t => t.ProfitLoss ?? 0);

                var stats = new PeriodStatistics
                {
                    PeriodStart = group.Key,
                    PeriodEnd = GetPeriodEnd(group.Key, periodType),
                    PeriodLabel = GetPeriodLabel(group.Key, periodType),
                    TradeCount = periodTrades.Count,
                    WinRate = periodTrades.Any() ? (decimal)winCount / periodTrades.Count * 100 : 0,
                    ProfitLoss = profitLoss
                };

                result.Add(stats);
            }

            return result;
        });
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private DateTime GetQuarterStart(DateTime date)
    {
        var quarter = (date.Month - 1) / 3;
        return new DateTime(date.Year, quarter * 3 + 1, 1);
    }

    private DateTime GetPeriodEnd(DateTime start, PeriodType periodType)
    {
        return periodType switch
        {
            PeriodType.Daily => start.AddDays(1).AddSeconds(-1),
            PeriodType.Weekly => start.AddDays(7).AddSeconds(-1),
            PeriodType.Monthly => start.AddMonths(1).AddSeconds(-1),
            PeriodType.Quarterly => start.AddMonths(3).AddSeconds(-1),
            PeriodType.Yearly => start.AddYears(1).AddSeconds(-1),
            _ => start.AddDays(1).AddSeconds(-1)
        };
    }

    private string GetPeriodLabel(DateTime date, PeriodType periodType)
    {
        return periodType switch
        {
            PeriodType.Daily => date.ToString("yyyy/MM/dd"),
            PeriodType.Weekly => $"هفته {date:yyyy/MM/dd}",
            PeriodType.Monthly => date.ToString("yyyy/MM"),
            PeriodType.Quarterly => $"Q{(date.Month - 1) / 3 + 1} {date.Year}",
            PeriodType.Yearly => date.Year.ToString(),
            _ => date.ToString("yyyy/MM/dd")
        };
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/AggregationEngine.cs
// =============================================================================