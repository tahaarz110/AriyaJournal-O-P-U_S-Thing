// ═══════════════════════════════════════════════════════════════════════
// فایل: AccountService.cs
// مسیر: src/AriaJournal.Core/Application/Services/AccountService.cs
// توضیح: سرویس حساب - نسخه اصلاح‌شده (حذف تعاریف تکراری)
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Application.DTOs;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
// ⚠️ استفاده از Events مرکزی
using AriaJournal.Core.Domain.Events;

namespace AriaJournal.Core.Application.Services;

// ═══════════════════════════════════════════════════════════════════════
// ⚠️ حذف شد: StateKeys و AccountChangedEvent و AccountSelectedEvent
// این‌ها الان در Domain/Events/CoreEvents.cs هستند
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// سرویس مدیریت حساب‌های معاملاتی
/// </summary>
public class AccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly IStateEngine _stateEngine;

    public AccountService(
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        IStateEngine stateEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
    }

    /// <summary>
    /// دریافت همه حساب‌های کاربر
    /// </summary>
    public async Task<Result<List<AccountDto>>> GetUserAccountsAsync(int userId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var tradeRepo = _unitOfWork.Repository<Trade>();

            var allAccounts = await accountRepo.GetAllAsync(a => a.UserId == userId && a.IsActive);
            var accounts = allAccounts.ToList();

            var result = new List<AccountDto>();

            foreach (var account in accounts)
            {
                var trades = await tradeRepo.GetAllAsync(t => t.AccountId == account.Id && !t.IsDeleted);
                var tradesList = trades.ToList();

                var dto = MapToDto(account);
                dto.TradesCount = tradesList.Count;
                dto.TotalProfitLoss = tradesList.Where(t => t.ProfitLoss.HasValue).Sum(t => t.ProfitLoss!.Value);
                
                var closedTrades = tradesList.Where(t => t.IsClosed && t.ProfitLoss.HasValue).ToList();
                if (closedTrades.Any())
                {
                    dto.WinRate = (decimal)closedTrades.Count(t => t.ProfitLoss > 0) / closedTrades.Count * 100;
                }

                dto.CurrentBalance = account.InitialBalance + dto.TotalProfitLoss;

                result.Add(dto);
            }

            return Result.Success(result.OrderByDescending(a => a.IsDefault).ThenBy(a => a.Name).ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت حساب‌ها: {ex.Message}");
            return Result.Failure<List<AccountDto>>(Error.Failure("خطا در دریافت لیست حساب‌ها"));
        }
    }

    /// <summary>
    /// دریافت حساب با شناسه
    /// </summary>
    public async Task<Result<AccountDto>> GetByIdAsync(int accountId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var account = await accountRepo.GetByIdAsync(accountId);

            if (account == null)
            {
                return Result.Failure<AccountDto>(Error.AccountNotFound);
            }

            return Result.Success(MapToDto(account));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت حساب: {ex.Message}");
            return Result.Failure<AccountDto>(Error.Failure("خطا در دریافت اطلاعات حساب"));
        }
    }

    /// <summary>
    /// ایجاد حساب جدید
    /// </summary>
    public async Task<Result<AccountDto>> CreateAsync(int userId, AccountCreateDto dto)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();

            // اگر پیش‌فرض است، ابتدا بقیه را غیرپیش‌فرض کن
            if (dto.IsDefault)
            {
                var existingDefaults = await accountRepo.GetAllAsync(a => a.UserId == userId && a.IsDefault);
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                    existing.UpdatedAt = DateTime.Now;
                }
            }

            var account = new Account
            {
                UserId = userId,
                Name = dto.Name,
                Type = dto.Type,
                BrokerName = dto.BrokerName,
                AccountNumber = dto.AccountNumber,
                Server = dto.Server,
                InitialBalance = dto.InitialBalance,
                CurrentBalance = dto.InitialBalance,
                Currency = dto.Currency,
                Leverage = dto.Leverage,
                Description = dto.Description,
                IsDefault = dto.IsDefault,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await accountRepo.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            if (dto.IsDefault)
            {
                _stateEngine.Set(StateKeys.CurrentAccountId, account.Id);
            }

            _eventBus.Publish(new AccountChangedEvent(account.Id, "Created"));

            return Result.Success(MapToDto(account));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد حساب: {ex.Message}");
            return Result.Failure<AccountDto>(Error.Failure("خطا در ذخیره حساب"));
        }
    }

    /// <summary>
    /// ویرایش حساب
    /// </summary>
    public async Task<Result<AccountDto>> UpdateAsync(AccountUpdateDto dto)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var account = await accountRepo.GetByIdAsync(dto.Id);

            if (account == null)
            {
                return Result.Failure<AccountDto>(Error.AccountNotFound);
            }

            // اگر پیش‌فرض شده، بقیه را غیرپیش‌فرض کن
            if (dto.IsDefault && !account.IsDefault)
            {
                var existingDefaults = await accountRepo.GetAllAsync(a => 
                    a.UserId == account.UserId && a.IsDefault && a.Id != dto.Id);
                foreach (var existing in existingDefaults)
                {
                    existing.IsDefault = false;
                    existing.UpdatedAt = DateTime.Now;
                }
            }

            account.Name = dto.Name;
            account.Type = dto.Type;
            account.BrokerName = dto.BrokerName;
            account.AccountNumber = dto.AccountNumber;
            account.Server = dto.Server;
            account.InitialBalance = dto.InitialBalance;
            account.Currency = dto.Currency;
            account.Leverage = dto.Leverage;
            account.Description = dto.Description;
            account.IsDefault = dto.IsDefault;
            account.IsActive = dto.IsActive;
            account.UpdatedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();

            _eventBus.Publish(new AccountChangedEvent(account.Id, "Updated"));

            return Result.Success(MapToDto(account));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ویرایش حساب: {ex.Message}");
            return Result.Failure<AccountDto>(Error.Failure("خطا در ذخیره حساب"));
        }
    }

    /// <summary>
    /// حذف حساب
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(int accountId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var account = await accountRepo.GetByIdAsync(accountId);

            if (account == null)
            {
                return Result.Failure<bool>(Error.AccountNotFound);
            }

            account.IsActive = false;
            account.UpdatedAt = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();

            _eventBus.Publish(new AccountChangedEvent(accountId, "Deleted"));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در حذف حساب: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در حذف حساب"));
        }
    }

    /// <summary>
    /// تنظیم حساب پیش‌فرض
    /// </summary>
    public async Task<Result<bool>> SetDefaultAsync(int userId, int accountId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();

            // دریافت حساب مورد نظر
            var account = await accountRepo.GetByIdAsync(accountId);
            if (account == null)
            {
                return Result.Failure<bool>(Error.AccountNotFound);
            }

            // بررسی مالکیت
            if (account.UserId != userId)
            {
                return Result.Failure<bool>(Error.Unauthorized("این حساب متعلق به شما نیست"));
            }

            // اگر قبلاً پیش‌فرض است، کاری نکن
            if (account.IsDefault)
            {
                return Result.Success(true);
            }

            // غیرفعال کردن پیش‌فرض‌های قبلی
            var allUserAccounts = await accountRepo.GetAllAsync(a => a.UserId == userId && a.IsActive);
            foreach (var acc in allUserAccounts)
            {
                if (acc.Id == accountId)
                {
                    acc.IsDefault = true;
                }
                else
                {
                    acc.IsDefault = false;
                }
                acc.UpdatedAt = DateTime.Now;
            }

            await _unitOfWork.SaveChangesAsync();

            // ذخیره در State
            _stateEngine.Set(StateKeys.CurrentAccountId, accountId);
            _eventBus.Publish(new AccountSelectedEvent(accountId));
            _eventBus.Publish(new AccountChangedEvent(accountId, "SetDefault"));

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در تنظیم حساب پیش‌فرض: {ex.Message}\n{ex.StackTrace}");
            return Result.Failure<bool>(Error.Failure("خطا در ذخیره تنظیمات"));
        }
    }

    /// <summary>
    /// دریافت حساب پیش‌فرض
    /// </summary>
    public async Task<Result<AccountDto?>> GetDefaultAsync(int userId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var account = await accountRepo.FirstOrDefaultAsync(a => 
                a.UserId == userId && a.IsDefault && a.IsActive);

            if (account == null)
            {
                account = await accountRepo.FirstOrDefaultAsync(a => a.UserId == userId && a.IsActive);
            }

            return Result.Success(account != null ? MapToDto(account) : null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت حساب پیش‌فرض: {ex.Message}");
            return Result.Failure<AccountDto?>(Error.Failure("خطا در دریافت حساب پیش‌فرض"));
        }
    }

    /// <summary>
    /// دریافت خلاصه حساب‌ها برای ComboBox
    /// </summary>
    public async Task<Result<List<AccountSummaryDto>>> GetAccountSummariesAsync(int userId)
    {
        try
        {
            var accountRepo = _unitOfWork.Repository<Account>();
            var accounts = await accountRepo.GetAllAsync(a => a.UserId == userId && a.IsActive);

            var result = accounts.Select(a => new AccountSummaryDto
            {
                Id = a.Id,
                Name = a.Name,
                BrokerName = a.BrokerName ?? string.Empty,
                TypeDisplayName = a.Type.ToString(),
                CurrentBalance = a.CurrentBalance,
                Currency = a.Currency,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive
            }).OrderByDescending(a => a.IsDefault).ThenBy(a => a.Name).ToList();

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در دریافت خلاصه حساب‌ها: {ex.Message}");
            return Result.Failure<List<AccountSummaryDto>>(Error.Failure("خطا در دریافت لیست حساب‌ها"));
        }
    }

    #region Private Methods

    private static AccountDto MapToDto(Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Name = account.Name,
            Type = account.Type,
            BrokerName = account.BrokerName ?? string.Empty,
            AccountNumber = account.AccountNumber,
            InitialBalance = account.InitialBalance,
            CurrentBalance = account.CurrentBalance,
            Currency = account.Currency,
            Leverage = account.Leverage,
            Description = account.Description,
            IsActive = account.IsActive,
            IsDefault = account.IsDefault,
            CreatedAt = account.CreatedAt
        };
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: AccountService.cs
// ═══════════════════════════════════════════════════════════════════════