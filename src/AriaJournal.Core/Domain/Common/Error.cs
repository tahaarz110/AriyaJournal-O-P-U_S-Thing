// ═══════════════════════════════════════════════════════════════════════
// فایل: Error.cs
// مسیر: src/AriaJournal.Core/Domain/Common/Error.cs
// توضیح: کلاس خطاها برای Result Pattern - نسخه کامل
// ═══════════════════════════════════════════════════════════════════════

namespace AriaJournal.Core.Domain.Common;

/// <summary>
/// کلاس نمایش خطا
/// </summary>
public sealed class Error
{
    /// <summary>
    /// کد خطا
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// پیام خطا
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// جزئیات اضافی
    /// </summary>
    public string? Details { get; }

    /// <summary>
    /// سازنده
    /// </summary>
    private Error(string code, string message, string? details = null)
    {
        Code = code;
        Message = message;
        Details = details;
    }

    /// <summary>
    /// خطای خالی (بدون خطا)
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای عمومی
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// خطای داخلی سیستم
    /// </summary>
    public static Error Internal(string? details = null) =>
        new("Internal", "خطای داخلی سیستم", details);

    /// <summary>
    /// خطای سفارشی
    /// </summary>
    public static Error Custom(string code, string message, string? details = null) =>
        new(code, message, details);

    /// <summary>
    /// خطای نامشخص
    /// </summary>
    public static Error Unknown(string? details = null) =>
        new("Unknown", "خطای نامشخص", details);

    /// <summary>
    /// خطای اعتبارسنجی
    /// </summary>
    public static Error Validation(string message, string? details = null) =>
        new("Validation", message, details);

    /// <summary>
    /// عملیات لغو شد
    /// </summary>
    public static Error Cancelled(string? details = null) =>
        new("Cancelled", "عملیات لغو شد", details);

    /// <summary>
    /// عملیات تکراری
    /// </summary>
    public static Error Duplicate(string message, string? details = null) =>
        new("Duplicate", message, details);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای فایل و پوشه
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// فایل یافت نشد
    /// </summary>
    public static Error FileNotFound(string? path = null) =>
        new("FileNotFound", "فایل یافت نشد", path);

    /// <summary>
    /// پوشه یافت نشد
    /// </summary>
    public static Error FolderNotFound(string? path = null) =>
        new("FolderNotFound", "پوشه یافت نشد", path);

    /// <summary>
    /// فرمت فایل نامعتبر
    /// </summary>
    public static Error InvalidFileFormat(string? details = null) =>
        new("InvalidFileFormat", "فرمت فایل نامعتبر است", details);

    /// <summary>
    /// خطا در خواندن فایل
    /// </summary>
    public static Error FileReadError(string? details = null) =>
        new("FileReadError", "خطا در خواندن فایل", details);

    /// <summary>
    /// خطا در نوشتن فایل
    /// </summary>
    public static Error FileWriteError(string? details = null) =>
        new("FileWriteError", "خطا در نوشتن فایل", details);

    /// <summary>
    /// خطا در حذف فایل
    /// </summary>
    public static Error DeleteFailed(string? details = null) =>
        new("DeleteFailed", "خطا در حذف فایل", details);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای احراز هویت
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// نام کاربری یا رمز عبور اشتباه
    /// </summary>
    public static Error InvalidCredentials() =>
        new("InvalidCredentials", "نام کاربری یا رمز عبور اشتباه است");

    /// <summary>
    /// حساب کاربری قفل شده
    /// </summary>
    public static Error AccountLocked(int remainingMinutes) =>
        new("AccountLocked", $"حساب کاربری قفل شده است. {remainingMinutes} دقیقه دیگر تلاش کنید");

    /// <summary>
    /// کاربر یافت نشد
    /// </summary>
    public static Error UserNotFound() =>
        new("UserNotFound", "کاربر یافت نشد");

    /// <summary>
    /// نام کاربری تکراری
    /// </summary>
    public static Error DuplicateUsername() =>
        new("DuplicateUsername", "این نام کاربری قبلاً ثبت شده است");

    /// <summary>
    /// رمز عبور ضعیف
    /// </summary>
    public static Error WeakPassword() =>
        new("WeakPassword", "رمز عبور باید حداقل ۶ کاراکتر باشد");

    /// <summary>
    /// کلید بازیابی نامعتبر
    /// </summary>
    public static Error InvalidRecoveryKey() =>
        new("InvalidRecoveryKey", "کلید بازیابی نامعتبر است");

    /// <summary>
    /// رمز عبور فعلی اشتباه
    /// </summary>
    public static Error InvalidCurrentPassword() =>
        new("InvalidCurrentPassword", "رمز عبور فعلی اشتباه است");

    // ═══════════════════════════════════════════════════════════════
    // خطاهای بکاپ
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// خطا در ایجاد بکاپ
    /// </summary>
    public static Error BackupFailed(string? details = null) =>
        new("BackupFailed", "خطا در ایجاد بکاپ", details);

    /// <summary>
    /// خطا در بازیابی بکاپ
    /// </summary>
    public static Error RestoreFailed(string? details = null) =>
        new("RestoreFailed", "خطا در بازیابی بکاپ", details);

    /// <summary>
    /// رمز بکاپ اشتباه
    /// </summary>
    public static Error InvalidBackupPassword() =>
        new("InvalidBackupPassword", "رمز بکاپ اشتباه است");

    /// <summary>
    /// بکاپ خراب است
    /// </summary>
    public static Error CorruptedBackup() =>
        new("CorruptedBackup", "فایل بکاپ خراب است");

    // ═══════════════════════════════════════════════════════════════
    // خطاهای Schema
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Schema نامعتبر
    /// </summary>
    public static Error InvalidSchema(string? details = null) =>
        new("InvalidSchema", "Schema نامعتبر است", details);

    /// <summary>
    /// Schema یافت نشد
    /// </summary>
    public static Error SchemaNotFound(string? schemaId = null) =>
        new("SchemaNotFound", "Schema یافت نشد", schemaId);

    /// <summary>
    /// خطا در پردازش Schema
    /// </summary>
    public static Error SchemaParseError(string? details = null) =>
        new("SchemaParseError", "خطا در پردازش Schema", details);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای دیتابیس
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// خطا در اتصال به دیتابیس
    /// </summary>
    public static Error DatabaseConnectionFailed(string? details = null) =>
        new("DatabaseConnectionFailed", "خطا در اتصال به دیتابیس", details);

    /// <summary>
    /// خطا در Migration
    /// </summary>
    public static Error MigrationFailed(string? details = null) =>
        new("MigrationFailed", "خطا در Migration دیتابیس", details);

    /// <summary>
    /// خطا در ذخیره تغییرات
    /// </summary>
    public static Error SaveChangesFailed(string? details = null) =>
        new("SaveChangesFailed", "خطا در ذخیره تغییرات", details);

    /// <summary>
    /// رکورد یافت نشد
    /// </summary>
    public static Error NotFound(string entityName) =>
        new("NotFound", $"{entityName} یافت نشد");

    // ═══════════════════════════════════════════════════════════════
    // خطاهای پلاگین
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// پلاگین یافت نشد
    /// </summary>
    public static Error PluginNotFound(string? pluginId = null) =>
        new("PluginNotFound", "پلاگین یافت نشد", pluginId);

    /// <summary>
    /// خطا در بارگذاری پلاگین
    /// </summary>
    public static Error PluginLoadFailed(string? details = null) =>
        new("PluginLoadFailed", "خطا در بارگذاری پلاگین", details);

    /// <summary>
    /// وابستگی پلاگین یافت نشد
    /// </summary>
    public static Error PluginDependencyMissing(string? dependency = null) =>
        new("PluginDependencyMissing", "وابستگی پلاگین یافت نشد", dependency);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای Import
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// خطا در Import
    /// </summary>
    public static Error ImportFailed(string? details = null) =>
        new("ImportFailed", "خطا در Import داده‌ها", details);

    /// <summary>
    /// فرمت Import نامعتبر
    /// </summary>
    public static Error InvalidImportFormat(string? details = null) =>
        new("InvalidImportFormat", "فرمت فایل Import نامعتبر است", details);

    // ═══════════════════════════════════════════════════════════════
    // خطاهای معامله
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// معامله یافت نشد
    /// </summary>
    public static Error TradeNotFound(int? tradeId = null) =>
        new("TradeNotFound", "معامله یافت نشد", tradeId?.ToString());

    /// <summary>
    /// حساب یافت نشد
    /// </summary>
    public static Error AccountNotFound(int? accountId = null) =>
        new("AccountNotFound", "حساب یافت نشد", accountId?.ToString());

    /// <summary>
    /// نماد نامعتبر
    /// </summary>
    public static Error InvalidSymbol(string? symbol = null) =>
        new("InvalidSymbol", "نماد نامعتبر است", symbol);

    // ═══════════════════════════════════════════════════════════════
    // متدهای کمکی
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// ایجاد خطا از Exception
    /// </summary>
    public static Error FromException(Exception ex) =>
        new("Exception", ex.Message, ex.StackTrace);

    /// <summary>
    /// تبدیل به رشته
    /// </summary>
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Details))
            return $"[{Code}] {Message}";
        return $"[{Code}] {Message} - {Details}";
    }

    /// <summary>
    /// مقایسه
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Error other)
            return Code == other.Code;
        return false;
    }

    /// <summary>
    /// HashCode
    /// </summary>
    public override int GetHashCode() => Code.GetHashCode();

    /// <summary>
    /// آیا خطا وجود دارد؟
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(Code);
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: Error.cs
// ═══════════════════════════════════════════════════════════════════════