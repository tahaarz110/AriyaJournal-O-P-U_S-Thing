// ═══════════════════════════════════════════════════════════════════════
// فایل: CoreEvents.cs
// مسیر: src/AriaJournal.Core/Domain/Events/CoreEvents.cs
// توضیح: تمام Events و StateKeys - تنها محل تعریف (حذف تکراری‌ها)
// ═══════════════════════════════════════════════════════════════════════

using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Domain.Events;

// ═══════════════════════════════════════════════════════════════════════
// کلیدهای State - تنها محل تعریف
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// کلیدهای استاندارد State
/// ⚠️ این تنها محل تعریف StateKeys است - در فایل‌های دیگر تعریف نکنید
/// </summary>
public static class StateKeys
{
    // کاربر
    public const string CurrentUser = "CurrentUser";
    public const string CurrentUserId = "CurrentUserId";
    public const string CurrentUsername = "CurrentUsername";
    public const string IsAuthenticated = "IsAuthenticated";
    public const string IsLoggedIn = "IsLoggedIn";

    // حساب
    public const string CurrentAccount = "CurrentAccount";
    public const string CurrentAccountId = "CurrentAccountId";
    public const string CurrentAccountName = "CurrentAccountName";

    // تنظیمات
    public const string Theme = "Theme";
    public const string CurrentTheme = "CurrentTheme";
    public const string Language = "Language";
    public const string CurrentLanguage = "CurrentLanguage";

    // سیستم
    public const string LastBackupDate = "LastBackupDate";
    public const string LoadedPlugins = "LoadedPlugins";
    public const string LastError = "LastError";
    public const string IsLoading = "IsLoading";
    public const string StatusMessage = "StatusMessage";
    public const string NavigationParameter = "NavigationParameter";
}

// ═══════════════════════════════════════════════════════════════════════
// Event های کاربر
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// کاربر وارد شد
/// </summary>
public record UserLoggedInEvent(int UserId, string Username);

/// <summary>
/// کاربر خارج شد
/// </summary>
public record UserLoggedOutEvent(int UserId);

/// <summary>
/// ثبت‌نام کاربر جدید
/// </summary>
public record UserRegisteredEvent(int UserId, string Username);

/// <summary>
/// رمز عبور تغییر کرد
/// </summary>
public record PasswordChangedEvent(int UserId);

// ═══════════════════════════════════════════════════════════════════════
// Event های حساب
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// حساب تغییر کرد (ایجاد/ویرایش/حذف)
/// </summary>
public record AccountChangedEvent(int AccountId, string ChangeType = "Changed");

/// <summary>
/// حساب انتخاب شد
/// </summary>
public record AccountSelectedEvent(int AccountId);

/// <summary>
/// حساب ایجاد شد
/// </summary>
public record AccountCreatedEvent(int AccountId, string AccountName);

/// <summary>
/// حساب حذف شد
/// </summary>
public record AccountDeletedEvent(int AccountId);

// ═══════════════════════════════════════════════════════════════════════
// Event های معامله
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// معامله ایجاد شد
/// </summary>
public record TradeCreatedEvent(int TradeId, int AccountId);

/// <summary>
/// معامله به‌روزرسانی شد
/// </summary>
public record TradeUpdatedEvent(int TradeId);

/// <summary>
/// معامله حذف شد
/// </summary>
public record TradeDeletedEvent(int TradeId);

/// <summary>
/// معاملات Import شدند
/// </summary>
public record TradesImportedEvent(int Count, int AccountId);

// ═══════════════════════════════════════════════════════════════════════
// Event های پلاگین
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// پلاگین بارگذاری شد
/// </summary>
public record PluginLoadedEvent(string PluginId, string PluginName);

/// <summary>
/// پلاگین حذف شد
/// </summary>
public record PluginUnloadedEvent(string PluginId);

/// <summary>
/// پلاگین فعال شد
/// </summary>
public record PluginEnabledEvent(string PluginId);

/// <summary>
/// پلاگین غیرفعال شد
/// </summary>
public record PluginDisabledEvent(string PluginId);

// ═══════════════════════════════════════════════════════════════════════
// Event های سیستم
// ═══════════════════════════════════════════════════════════════════════

/// <summary>
/// Schema بارگذاری مجدد شد
/// </summary>
public record SchemaReloadedEvent();

/// <summary>
/// بکاپ ایجاد شد
/// </summary>
public record BackupCreatedEvent(string Path, DateTime CreatedAt);

/// <summary>
/// بکاپ بازیابی شد
/// </summary>
public record BackupRestoredEvent(string Path);

/// <summary>
/// تنظیمات تغییر کرد
/// </summary>
public record SettingsChangedEvent(string SettingKey, object? NewValue);

/// <summary>
/// تم تغییر کرد
/// </summary>
public record ThemeChangedEvent(string ThemeName);

/// <summary>
/// ناوبری انجام شد
/// </summary>
public record NavigatedEvent(string ViewName, object? Parameter);

/// <summary>
/// خطا رخ داد
/// </summary>
public record ErrorOccurredEvent(string Message, string? Details, DateTime OccurredAt);

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: CoreEvents.cs
// ═══════════════════════════════════════════════════════════════════════