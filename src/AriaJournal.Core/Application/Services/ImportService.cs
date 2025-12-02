// ═══════════════════════════════════════════════════════════════════════
// فایل: ImportService.cs
// مسیر: src/AriaJournal.Core/Application/Services/ImportService.cs
// توضیح: سرویس Import داده از CSV/JSON
// ═══════════════════════════════════════════════════════════════════════

using System.Globalization;
using System.Text.Json;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Domain.Events;

namespace AriaJournal.Core.Application.Services;

/// <summary>
/// سرویس Import داده از فایل‌های خارجی
/// </summary>
public class ImportService : IImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly ICacheEngine _cacheEngine;

    private static readonly string[] SupportedExtensions = { ".csv", ".json" };

    public ImportService(
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        ICacheEngine cacheEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));
    }

    /// <summary>
    /// Import از فایل CSV
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromCsvAsync(
        string filePath, 
        int accountId, 
        ImportOptions? options = null)
    {
        try
        {
            var validateResult = await ValidateFileAsync(filePath);
            if (!validateResult.IsSuccess)
                return Result.Failure<ImportResult>(validateResult.Error);

            var csvText = await File.ReadAllTextAsync(filePath);
            return await ImportFromCsvTextAsync(csvText, accountId, options);
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportResult>(Error.Failure($"خطا در خواندن فایل: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import از فایل JSON
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromJsonAsync(
        string filePath, 
        int accountId, 
        ImportOptions? options = null)
    {
        try
        {
            var validateResult = await ValidateFileAsync(filePath);
            if (!validateResult.IsSuccess)
                return Result.Failure<ImportResult>(validateResult.Error);

            var jsonText = await File.ReadAllTextAsync(filePath);
            return await ImportFromJsonTextAsync(jsonText, accountId, options);
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportResult>(Error.Failure($"خطا در خواندن فایل: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import از متن CSV
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromCsvTextAsync(
        string csvText, 
        int accountId, 
        ImportOptions? options = null)
    {
        options ??= new ImportOptions();
        var result = new ImportResult { StartTime = DateTime.Now };
        var errors = new List<ImportError>();
        var importedTrades = new List<Trade>();

        try
        {
            var lines = csvText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                return Result.Failure<ImportResult>(Error.Validation("فایل خالی است"));
            }

            // خواندن Header
            var headerLine = options.HasHeader ? lines[0] : null;
            var headers = headerLine?.Split(options.CsvDelimiter)
                .Select(h => h.Trim().Trim('"'))
                .ToArray() ?? Array.Empty<string>();

            var mapping = options.ColumnMapping ?? GetDefaultMapping();
            var startIndex = options.HasHeader ? 1 : 0;

            result.TotalRows = lines.Length - startIndex;

            var tradeRepo = _unitOfWork.Repository<Trade>();

            for (int i = startIndex; i < lines.Length; i++)
            {
                if (errors.Count >= options.MaxErrors && !options.IgnoreErrors)
                    break;

                try
                {
                    var values = ParseCsvLine(lines[i], options.CsvDelimiter);
                    var trade = MapToTrade(headers, values, mapping, accountId, options);

                    if (trade == null)
                    {
                        errors.Add(new ImportError
                        {
                            RowNumber = i + 1,
                            Message = "نمی‌توان ردیف را پردازش کرد",
                            ErrorType = ImportErrorType.ParseError
                        });
                        continue;
                    }

                    // بررسی تکراری
                    if (options.SkipDuplicates)
                    {
                        var isDuplicate = await CheckDuplicateAsync(trade, accountId);
                        if (isDuplicate)
                        {
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    // اضافه کردن تگ خودکار
                    if (!string.IsNullOrEmpty(options.AutoTag))
                    {
                        trade.Tags = string.IsNullOrEmpty(trade.Tags) 
                            ? options.AutoTag 
                            : $"{trade.Tags},{options.AutoTag}";
                    }

                    await tradeRepo.AddAsync(trade);
                    importedTrades.Add(trade);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = i + 1,
                        Message = ex.Message,
                        ErrorType = ImportErrorType.Unknown
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            result.EndTime = DateTime.Now;
            result.Errors = errors;
            result.ErrorCount = errors.Count;
            result.ImportedTrades = importedTrades;

            // اطلاع‌رسانی
            if (result.SuccessCount > 0)
            {
                _eventBus.Publish(new TradesImportedEvent(result.SuccessCount, accountId));
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportResult>(Error.Failure($"خطا در Import: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import از متن JSON
    /// </summary>
    public async Task<Result<ImportResult>> ImportFromJsonTextAsync(
        string jsonText, 
        int accountId, 
        ImportOptions? options = null)
    {
        options ??= new ImportOptions();
        var result = new ImportResult { StartTime = DateTime.Now };
        var errors = new List<ImportError>();
        var importedTrades = new List<Trade>();

        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var records = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonText, jsonOptions);
            
            if (records == null || records.Count == 0)
            {
                return Result.Failure<ImportResult>(Error.Validation("فایل خالی است یا فرمت نامعتبر است"));
            }

            result.TotalRows = records.Count;
            var mapping = options.ColumnMapping ?? GetDefaultMapping();
            var tradeRepo = _unitOfWork.Repository<Trade>();

            for (int i = 0; i < records.Count; i++)
            {
                if (errors.Count >= options.MaxErrors && !options.IgnoreErrors)
                    break;

                try
                {
                    var trade = MapJsonToTrade(records[i], mapping, accountId, options);

                    if (trade == null)
                    {
                        errors.Add(new ImportError
                        {
                            RowNumber = i + 1,
                            Message = "نمی‌توان رکورد را پردازش کرد",
                            ErrorType = ImportErrorType.ParseError
                        });
                        continue;
                    }

                    // بررسی تکراری
                    if (options.SkipDuplicates)
                    {
                        var isDuplicate = await CheckDuplicateAsync(trade, accountId);
                        if (isDuplicate)
                        {
                            result.SkippedCount++;
                            continue;
                        }
                    }

                    // تگ خودکار
                    if (!string.IsNullOrEmpty(options.AutoTag))
                    {
                        trade.Tags = string.IsNullOrEmpty(trade.Tags)
                            ? options.AutoTag
                            : $"{trade.Tags},{options.AutoTag}";
                    }

                    await tradeRepo.AddAsync(trade);
                    importedTrades.Add(trade);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(new ImportError
                    {
                        RowNumber = i + 1,
                        Message = ex.Message,
                        ErrorType = ImportErrorType.Unknown
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            result.EndTime = DateTime.Now;
            result.Errors = errors;
            result.ErrorCount = errors.Count;
            result.ImportedTrades = importedTrades;

            if (result.SuccessCount > 0)
            {
                _eventBus.Publish(new TradesImportedEvent(result.SuccessCount, accountId));
            }

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportResult>(Error.Failure($"خطا در Import JSON: {ex.Message}"));
        }
    }

    /// <summary>
    /// پیش‌نمایش Import
    /// </summary>
    public async Task<Result<ImportPreview>> PreviewImportAsync(string filePath, ImportOptions? options = null)
    {
        options ??= new ImportOptions();
        var preview = new ImportPreview();

        try
        {
            var extension = Path.GetExtension(filePath).ToLower();
            var content = await File.ReadAllTextAsync(filePath);

            if (extension == ".csv")
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                preview.TotalRows = options.HasHeader ? lines.Length - 1 : lines.Length;

                if (options.HasHeader && lines.Length > 0)
                {
                    preview.DetectedColumns = ParseCsvLine(lines[0], options.CsvDelimiter)
                        .Select(c => c.Trim().Trim('"'))
                        .ToList();
                }

                // نمونه داده (۵ ردیف اول)
                var startIndex = options.HasHeader ? 1 : 0;
                for (int i = startIndex; i < Math.Min(startIndex + 5, lines.Length); i++)
                {
                    var values = ParseCsvLine(lines[i], options.CsvDelimiter);
                    var row = new Dictionary<string, string>();
                    
                    for (int j = 0; j < preview.DetectedColumns.Count && j < values.Length; j++)
                    {
                        row[preview.DetectedColumns[j]] = values[j].Trim().Trim('"');
                    }
                    
                    preview.SampleData.Add(row);
                }
            }
            else if (extension == ".json")
            {
                var records = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(content);
                
                if (records != null && records.Count > 0)
                {
                    preview.TotalRows = records.Count;
                    preview.DetectedColumns = records[0].Keys.ToList();

                    foreach (var record in records.Take(5))
                    {
                        var row = record.ToDictionary(
                            k => k.Key,
                            v => v.Value.ToString());
                        preview.SampleData.Add(row);
                    }
                }
            }

            // Mapping پیشنهادی
            preview.SuggestedMapping = SuggestMapping(preview.DetectedColumns);

            return Result.Success(preview);
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportPreview>(Error.Failure($"خطا در پیش‌نمایش: {ex.Message}"));
        }
    }

    /// <summary>
    /// Mapping پیش‌فرض
    /// </summary>
    public Dictionary<string, string> GetDefaultMapping()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "symbol", "Symbol" },
            { "pair", "Symbol" },
            { "instrument", "Symbol" },
            { "direction", "Direction" },
            { "type", "Direction" },
            { "side", "Direction" },
            { "volume", "Volume" },
            { "lots", "Volume" },
            { "size", "Volume" },
            { "entry_price", "EntryPrice" },
            { "open_price", "EntryPrice" },
            { "price", "EntryPrice" },
            { "exit_price", "ExitPrice" },
            { "close_price", "ExitPrice" },
            { "stop_loss", "StopLoss" },
            { "sl", "StopLoss" },
            { "take_profit", "TakeProfit" },
            { "tp", "TakeProfit" },
            { "entry_time", "EntryTime" },
            { "open_time", "EntryTime" },
            { "time", "EntryTime" },
            { "exit_time", "ExitTime" },
            { "close_time", "ExitTime" },
            { "profit", "ProfitLoss" },
            { "pnl", "ProfitLoss" },
            { "profit_loss", "ProfitLoss" },
            { "commission", "Commission" },
            { "swap", "Swap" },
            { "comment", "Notes" },
            { "notes", "Notes" }
        };
    }

    /// <summary>
    /// اعتبارسنجی فایل
    /// </summary>
    public async Task<Result<bool>> ValidateFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return Result.Failure<bool>(Error.Validation("مسیر فایل مشخص نشده است"));

        if (!File.Exists(filePath))
            return Result.Failure<bool>(Error.NotFound("فایل یافت نشد"));

        var extension = Path.GetExtension(filePath).ToLower();
        if (!SupportsFormat(extension))
            return Result.Failure<bool>(Error.Validation($"فرمت {extension} پشتیبانی نمی‌شود"));

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
            return Result.Failure<bool>(Error.Validation("فایل خالی است"));

        if (fileInfo.Length > 100 * 1024 * 1024) // 100MB
            return Result.Failure<bool>(Error.Validation("حجم فایل بیش از حد مجاز است"));

        return await Task.FromResult(Result.Success(true));
    }

    /// <summary>
    /// پشتیبانی فرمت
    /// </summary>
    public bool SupportsFormat(string extension)
    {
        return SupportedExtensions.Contains(extension.ToLower());
    }

    #region Private Methods

    private string[] ParseCsvLine(string line, char delimiter)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == delimiter && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        
        result.Add(current);
        return result.ToArray();
    }

    private Trade? MapToTrade(
        string[] headers, 
        string[] values, 
        Dictionary<string, string> mapping, 
        int accountId,
        ImportOptions options)
    {
        try
        {
            var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            for (int i = 0; i < headers.Length && i < values.Length; i++)
            {
                data[headers[i]] = values[i].Trim().Trim('"');
            }

            return CreateTradeFromData(data, mapping, accountId, options);
        }
        catch
        {
            return null;
        }
    }

    private Trade? MapJsonToTrade(
        Dictionary<string, JsonElement> record,
        Dictionary<string, string> mapping,
        int accountId,
        ImportOptions options)
    {
        try
        {
            var data = record.ToDictionary(
                k => k.Key,
                v => v.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

            return CreateTradeFromData(data, mapping, accountId, options);
        }
        catch
        {
            return null;
        }
    }

    private Trade CreateTradeFromData(
        Dictionary<string, string> data,
        Dictionary<string, string> mapping,
        int accountId,
        ImportOptions options)
    {
        var trade = new Trade
        {
            AccountId = accountId,
            CreatedAt = DateTime.Now,
            IsDeleted = false
        };

        // Symbol - الزامی
        var symbolKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Symbol");
        if (symbolKey != null)
        {
            trade.Symbol = data[symbolKey].ToUpperInvariant();
        }

        if (string.IsNullOrEmpty(trade.Symbol))
            throw new Exception("نماد یافت نشد");

        // Direction
        var directionKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Direction");
        if (directionKey != null)
        {
            var dirValue = data[directionKey].ToLower();
            trade.Direction = dirValue.Contains("buy") || dirValue.Contains("long") || dirValue == "1"
                ? TradeDirection.Buy
                : TradeDirection.Sell;
        }

        // Volume
        var volumeKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Volume");
        if (volumeKey != null && decimal.TryParse(data[volumeKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var vol))
        {
            trade.Volume = vol;
        }

        // EntryPrice
        var entryPriceKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "EntryPrice");
        if (entryPriceKey != null && decimal.TryParse(data[entryPriceKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var ep))
        {
            trade.EntryPrice = ep;
        }

        // ExitPrice
        var exitPriceKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "ExitPrice");
        if (exitPriceKey != null && decimal.TryParse(data[exitPriceKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var xp))
        {
            trade.ExitPrice = xp;
            trade.IsClosed = true;
        }

        // EntryTime
        var entryTimeKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "EntryTime");
        if (entryTimeKey != null)
        {
            if (DateTime.TryParseExact(data[entryTimeKey], options.DateFormat, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var et))
            {
                trade.EntryTime = et;
            }
            else if (DateTime.TryParse(data[entryTimeKey], out var et2))
            {
                trade.EntryTime = et2;
            }
        }
        
        if (trade.EntryTime == default)
            trade.EntryTime = DateTime.Now;

        // ExitTime
        var exitTimeKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "ExitTime");
        if (exitTimeKey != null && !string.IsNullOrEmpty(data[exitTimeKey]))
        {
            if (DateTime.TryParseExact(data[exitTimeKey], options.DateFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var xt))
            {
                trade.ExitTime = xt;
            }
            else if (DateTime.TryParse(data[exitTimeKey], out var xt2))
            {
                trade.ExitTime = xt2;
            }
        }

        // StopLoss
        var slKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "StopLoss");
        if (slKey != null && decimal.TryParse(data[slKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var sl))
        {
            trade.StopLoss = sl;
        }

        // TakeProfit
        var tpKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "TakeProfit");
        if (tpKey != null && decimal.TryParse(data[tpKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var tp))
        {
            trade.TakeProfit = tp;
        }

        // ProfitLoss
        var plKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "ProfitLoss");
        if (plKey != null && decimal.TryParse(data[plKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var pl))
        {
            trade.ProfitLoss = pl;
        }

        // Commission
        var commKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Commission");
        if (commKey != null && decimal.TryParse(data[commKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var comm))
        {
            trade.Commission = comm;
        }

        // Swap
        var swapKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Swap");
        if (swapKey != null && decimal.TryParse(data[swapKey], NumberStyles.Any, CultureInfo.InvariantCulture, out var swap))
        {
            trade.Swap = swap;
        }

        // Notes
        var notesKey = mapping.Keys.FirstOrDefault(k => 
            data.ContainsKey(k) && mapping[k] == "Notes");
        if (notesKey != null)
        {
            trade.PostTradeNotes = data[notesKey];
        }

        return trade;
    }

    private async Task<bool> CheckDuplicateAsync(Trade trade, int accountId)
    {
        var tradeRepo = _unitOfWork.Repository<Trade>();
        
        // بررسی بر اساس نماد، زمان ورود و قیمت
        var exists = await tradeRepo.AnyAsync(t =>
            t.AccountId == accountId &&
            t.Symbol == trade.Symbol &&
            t.EntryTime == trade.EntryTime &&
            t.EntryPrice == trade.EntryPrice &&
            !t.IsDeleted);

        return exists;
    }

    private Dictionary<string, string> SuggestMapping(List<string> columns)
    {
        var defaultMapping = GetDefaultMapping();
        var result = new Dictionary<string, string>();

        foreach (var column in columns)
        {
            var columnLower = column.ToLower().Replace(" ", "_").Replace("-", "_");
            
            if (defaultMapping.TryGetValue(columnLower, out var fieldName))
            {
                result[column] = fieldName;
            }
        }

        return result;
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: ImportService.cs
// ═══════════════════════════════════════════════════════════════════════