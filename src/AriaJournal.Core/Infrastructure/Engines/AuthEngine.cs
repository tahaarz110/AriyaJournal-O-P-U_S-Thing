// ═══════════════════════════════════════════════════════════════════════
// فایل: AuthEngine.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Engines/AuthEngine.cs
// توضیح: موتور احراز هویت - نسخه اصلاح‌شده (حذف Events تکراری)
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Enums;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Security;
// ⚠️ استفاده از Events مرکزی
using AriaJournal.Core.Domain.Events;

namespace AriaJournal.Core.Infrastructure.Engines;

// ═══════════════════════════════════════════════════════════════════════
// ⚠️ حذف شد: UserLoggedInEvent و UserLoggedOutEvent
// این‌ها الان در Domain/Events/CoreEvents.cs هستند
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// پیاده‌سازی موتور احراز هویت
/// </summary>
public class AuthEngine : IAuthEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusEngine _eventBus;
    private readonly IStateEngine _stateEngine;
    private readonly ICacheEngine _cacheEngine;

    private const int MaxFailedAttempts = 3;
    private const int LockoutMinutes = 5;
    private const int MinPasswordLength = 6;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public AuthEngine(
        IUnitOfWork unitOfWork,
        IEventBusEngine eventBus,
        IStateEngine stateEngine,
        ICacheEngine cacheEngine)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _stateEngine = stateEngine ?? throw new ArgumentNullException(nameof(stateEngine));
        _cacheEngine = cacheEngine ?? throw new ArgumentNullException(nameof(cacheEngine));
    }

    public async Task<Result<LoginResult>> LoginAsync(string username, string password)
    {
        // اعتبارسنجی ورودی
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<LoginResult>(Error.Validation("نام کاربری را وارد کنید"));

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<LoginResult>(Error.Validation("رمز عبور را وارد کنید"));

        try
        {
            // یافتن کاربر
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => 
                u.Username.ToLower() == username.ToLower().Trim() && u.IsActive);

            if (user == null)
                return Result.Failure<LoginResult>(Error.InvalidCredentials);

            // بررسی قفل بودن
            if (IsUserLockedOut(user))
                return Result.Failure<LoginResult>(Error.AccountLocked);

            // بررسی رمز عبور
            if (!PasswordHasher.Verify(password, user.PasswordHash))
            {
                await HandleFailedLoginAsync(user);
                return Result.Failure<LoginResult>(Error.InvalidCredentials);
            }

            // ورود موفق
            await HandleSuccessfulLoginAsync(user);

            return Result.Success(new LoginResult
            {
                User = user,
                Token = GenerateSessionToken()
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ورود: {ex.Message}");
            return Result.Failure<LoginResult>(Error.Failure("خطا در عملیات ورود"));
        }
    }

    public async Task<Result<RegisterResult>> RegisterAsync(string username, string password)
    {
        // اعتبارسنجی ورودی
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<RegisterResult>(Error.Validation("نام کاربری را وارد کنید"));

        if (username.Trim().Length < 3)
            return Result.Failure<RegisterResult>(Error.Validation("نام کاربری باید حداقل ۳ کاراکتر باشد"));

        if (string.IsNullOrWhiteSpace(password))
            return Result.Failure<RegisterResult>(Error.Validation("رمز عبور را وارد کنید"));

        if (password.Length < MinPasswordLength)
            return Result.Failure<RegisterResult>(Error.Validation($"رمز عبور باید حداقل {MinPasswordLength} کاراکتر باشد"));

        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            
            // بررسی تکراری نبودن
            var exists = await userRepo.AnyAsync(u => 
                u.Username.ToLower() == username.ToLower().Trim());

            if (exists)
                return Result.Failure<RegisterResult>(Error.DuplicateUsername);

            // تولید کلید بازیابی
            var recoveryKey = RecoveryKeyGenerator.Generate();

            // ایجاد کاربر
            var user = new User
            {
                Username = username.Trim(),
                PasswordHash = PasswordHasher.Hash(password),
                RecoveryKeyHash = RecoveryKeyGenerator.Hash(recoveryKey),
                CreatedAt = DateTime.Now,
                IsActive = true,
                FailedLoginAttempts = 0
            };

            await userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // ایجاد تنظیمات پیش‌فرض
            await CreateDefaultSettingsAsync(user.Id);

            // ایجاد فیلدهای پیش‌فرض
            await CreateDefaultFieldDefinitionsAsync(user.Id);

            return Result.Success(new RegisterResult
            {
                User = user,
                RecoveryKey = recoveryKey
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ثبت‌نام: {ex.Message}");
            return Result.Failure<RegisterResult>(Error.Failure("خطا در عملیات ثبت‌نام"));
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
            return Result.Failure<bool>(Error.Validation("رمز عبور فعلی را وارد کنید"));

        if (string.IsNullOrWhiteSpace(newPassword))
            return Result.Failure<bool>(Error.Validation("رمز عبور جدید را وارد کنید"));

        if (newPassword.Length < MinPasswordLength)
            return Result.Failure<bool>(Error.Validation($"رمز عبور باید حداقل {MinPasswordLength} کاراکتر باشد"));

        if (currentPassword == newPassword)
            return Result.Failure<bool>(Error.Validation("رمز عبور جدید باید متفاوت از رمز فعلی باشد"));

        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(userId);

            if (user == null)
                return Result.Failure<bool>(Error.UserNotFound);

            if (!PasswordHasher.Verify(currentPassword, user.PasswordHash))
                return Result.Failure<bool>(Error.InvalidCurrentPassword);

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.UpdatedAt = DateTime.Now;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش کاربر
            _cacheEngine.Remove($"user:{userId}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در تغییر رمز: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در تغییر رمز عبور"));
        }
    }

    public async Task<Result<bool>> RecoverAsync(string username, string recoveryKey, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<bool>(Error.Validation("نام کاربری را وارد کنید"));

        if (string.IsNullOrWhiteSpace(recoveryKey))
            return Result.Failure<bool>(Error.Validation("کلید بازیابی را وارد کنید"));

        if (string.IsNullOrWhiteSpace(newPassword))
            return Result.Failure<bool>(Error.Validation("رمز عبور جدید را وارد کنید"));

        if (newPassword.Length < MinPasswordLength)
            return Result.Failure<bool>(Error.Validation($"رمز عبور باید حداقل {MinPasswordLength} کاراکتر باشد"));

        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.FirstOrDefaultAsync(u => 
                u.Username.ToLower() == username.ToLower().Trim());

            if (user == null)
                return Result.Failure<bool>(Error.UserNotFound);

            if (!RecoveryKeyGenerator.Verify(recoveryKey, user.RecoveryKeyHash))
                return Result.Failure<bool>(Error.InvalidRecoveryKey);

            // تغییر رمز عبور و باز کردن قفل
            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.FailedLoginAttempts = 0;
            user.LockoutEndTime = null;
            user.UpdatedAt = DateTime.Now;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // پاک کردن کش
            _cacheEngine.Remove($"user:{user.Id}");
            _cacheEngine.Remove($"lockout:{user.Id}");

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در بازیابی: {ex.Message}");
            return Result.Failure<bool>(Error.Failure("خطا در بازیابی رمز عبور"));
        }
    }

    public bool IsLockedOut(int userId)
    {
        // بررسی از کش
        var lockKey = $"lockout:{userId}";
        var lockoutEnd = _cacheEngine.Get<DateTime?>(lockKey);

        if (lockoutEnd.HasValue && lockoutEnd.Value > DateTime.Now)
            return true;

        return false;
    }

    public async Task LogoutAsync()
    {
        if (CurrentUser != null)
        {
            var userId = CurrentUser.Id;

            // پاک کردن State
            _stateEngine.Remove(StateKeys.CurrentUser);
            _stateEngine.Remove(StateKeys.CurrentUserId);
            _stateEngine.Remove(StateKeys.CurrentAccount);
            _stateEngine.Remove(StateKeys.CurrentAccountId);
            _stateEngine.Set(StateKeys.IsAuthenticated, false);

            // پاک کردن کش کاربر
            _cacheEngine.Remove($"user:{userId}");

            // انتشار Event
            _eventBus.Publish(new UserLoggedOutEvent(userId));

            CurrentUser = null;
        }

        await Task.CompletedTask;
    }

    #region Private Methods

    private bool IsUserLockedOut(User user)
    {
        if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.Now)
        {
            // کش کردن زمان قفل
            _cacheEngine.Set($"lockout:{user.Id}", user.LockoutEndTime.Value, 
                user.LockoutEndTime.Value - DateTime.Now);
            return true;
        }

        return false;
    }

    private async Task HandleFailedLoginAsync(User user)
    {
        user.FailedLoginAttempts++;

        if (user.FailedLoginAttempts >= MaxFailedAttempts)
        {
            user.LockoutEndTime = DateTime.Now.AddMinutes(LockoutMinutes);
            _cacheEngine.Set($"lockout:{user.Id}", user.LockoutEndTime.Value, 
                TimeSpan.FromMinutes(LockoutMinutes));
        }

        var userRepo = _unitOfWork.Repository<User>();
        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task HandleSuccessfulLoginAsync(User user)
    {
        user.FailedLoginAttempts = 0;
        user.LockoutEndTime = null;
        user.LastLoginAt = DateTime.Now;

        var userRepo = _unitOfWork.Repository<User>();
        userRepo.Update(user);
        await _unitOfWork.SaveChangesAsync();

        // تنظیم State
        CurrentUser = user;
        _stateEngine.Set(StateKeys.CurrentUser, user);
        _stateEngine.Set(StateKeys.CurrentUserId, user.Id);
        _stateEngine.Set(StateKeys.IsAuthenticated, true);

        // کش کردن
        _cacheEngine.Set($"user:{user.Id}", user, TimeSpan.FromHours(1));

        // پاک کردن کش قفل
        _cacheEngine.Remove($"lockout:{user.Id}");

        // انتشار Event
        _eventBus.Publish(new UserLoggedInEvent(user.Id, user.Username));
    }

    private string GenerateSessionToken()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    private async Task CreateDefaultSettingsAsync(int userId)
    {
        var settingsRepo = _unitOfWork.Repository<Settings>();
        var settings = new Settings
        {
            UserId = userId,
            Theme = "Dark",
            Language = "fa",
            DateFormat = "yyyy/MM/dd",
            TimeFormat = "HH:mm",
            DefaultCurrency = "USD",
            PriceDecimals = 5,
            VolumeDecimals = 2,
            AutoBackup = true,
            AutoBackupInterval = 7,
            ShowNotifications = true,
            PlaySounds = false,
            TradeListViewMode = "Table",
            ItemsPerPage = 50
        };

        await settingsRepo.AddAsync(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task CreateDefaultFieldDefinitionsAsync(int userId)
    {
        var fieldRepo = _unitOfWork.Repository<FieldDefinition>();

        var defaultFields = new List<FieldDefinition>
        {
            new()
            {
                UserId = userId,
                FieldName = "session",
                DisplayName = "سشن معاملاتی",
                FieldType = FieldType.Select,
                Options = "[\"London\",\"New York\",\"Asian\",\"Sydney\"]",
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 1,
                Order = 1,
                Category = "زمان‌بندی",
                CreatedAt = DateTime.Now
            },
            new()
            {
                UserId = userId,
                FieldName = "timeframe",
                DisplayName = "تایم‌فریم",
                FieldType = FieldType.Select,
                Options = "[\"M1\",\"M5\",\"M15\",\"M30\",\"H1\",\"H4\",\"D1\",\"W1\"]",
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 2,
                Order = 2,
                Category = "تحلیل",
                CreatedAt = DateTime.Now
            },
            new()
            {
                UserId = userId,
                FieldName = "setup_type",
                DisplayName = "نوع ستاپ",
                FieldType = FieldType.Select,
                Options = "[\"Order Block\",\"FVG\",\"Breaker\",\"Mitigation\",\"Liquidity Sweep\",\"Supply/Demand\"]",
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 3,
                Order = 3,
                Category = "استراتژی",
                CreatedAt = DateTime.Now
            },
            new()
            {
                UserId = userId,
                FieldName = "market_structure",
                DisplayName = "ساختار بازار",
                FieldType = FieldType.Select,
                Options = "[\"BOS\",\"CHOCH\",\"Range\",\"Trending Up\",\"Trending Down\"]",
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 4,
                Order = 4,
                Category = "تحلیل",
                CreatedAt = DateTime.Now
            },
            new()
            {
                UserId = userId,
                FieldName = "mood",
                DisplayName = "حالت روحی",
                FieldType = FieldType.Select,
                Options = "[\"عالی\",\"خوب\",\"معمولی\",\"استرس\",\"خسته\",\"عصبانی\"]",
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 5,
                Order = 5,
                Category = "روانشناسی",
                CreatedAt = DateTime.Now
            },
            new()
            {
                UserId = userId,
                FieldName = "confidence",
                DisplayName = "میزان اطمینان",
                FieldType = FieldType.Rating,
                IsSystem = true,
                IsActive = true,
                DisplayOrder = 6,
                Order = 6,
                Category = "روانشناسی",
                CreatedAt = DateTime.Now
            }
        };

        await fieldRepo.AddRangeAsync(defaultFields);
        await _unitOfWork.SaveChangesAsync();
    }

    #endregion
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: AuthEngine.cs
// ═══════════════════════════════════════════════════════════════════════