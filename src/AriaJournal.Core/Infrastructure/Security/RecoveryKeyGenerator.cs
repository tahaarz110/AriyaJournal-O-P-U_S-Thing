// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Security/RecoveryKeyGenerator.cs
// شماره فایل: 36
// =============================================================================

using System.Security.Cryptography;

namespace AriaJournal.Core.Infrastructure.Security;

/// <summary>
/// تولید و مدیریت کلید بازیابی
/// </summary>
public static class RecoveryKeyGenerator
{
    // طول کلید بازیابی (32 کاراکتر)
    private const int KeyLength = 32;

    // کاراکترهای مجاز (بدون کاراکترهای مشابه مثل 0/O و 1/l/I)
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    /// <summary>
    /// تولید کلید بازیابی جدید
    /// </summary>
    public static string Generate()
    {
        var key = new char[KeyLength];
        var randomBytes = new byte[KeyLength];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < KeyLength; i++)
        {
            key[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
        }

        // فرمت: XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX
        return FormatKey(new string(key));
    }

    /// <summary>
    /// هش کردن کلید بازیابی
    /// </summary>
    public static string Hash(string recoveryKey)
    {
        if (string.IsNullOrWhiteSpace(recoveryKey))
            throw new ArgumentException("کلید بازیابی نمی‌تواند خالی باشد", nameof(recoveryKey));

        // حذف خط‌تیره‌ها و تبدیل به حروف بزرگ
        var cleanKey = recoveryKey.Replace("-", "").ToUpperInvariant();

        // استفاده از BCrypt برای هش
        return BCrypt.Net.BCrypt.HashPassword(cleanKey, 10);
    }

    /// <summary>
    /// بررسی صحت کلید بازیابی
    /// </summary>
    public static bool Verify(string recoveryKey, string hash)
    {
        if (string.IsNullOrWhiteSpace(recoveryKey) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            var cleanKey = recoveryKey.Replace("-", "").ToUpperInvariant();
            return BCrypt.Net.BCrypt.Verify(cleanKey, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// فرمت کردن کلید با خط‌تیره
    /// </summary>
    public static string FormatKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        var cleanKey = key.Replace("-", "").ToUpperInvariant();

        if (cleanKey.Length != KeyLength)
            return key;

        // تقسیم به گروه‌های ۴ کاراکتری
        var groups = new List<string>();
        for (int i = 0; i < cleanKey.Length; i += 4)
        {
            groups.Add(cleanKey.Substring(i, Math.Min(4, cleanKey.Length - i)));
        }

        return string.Join("-", groups);
    }

    /// <summary>
    /// اعتبارسنجی فرمت کلید
    /// </summary>
    public static bool IsValidFormat(string recoveryKey)
    {
        if (string.IsNullOrWhiteSpace(recoveryKey))
            return false;

        var cleanKey = recoveryKey.Replace("-", "").ToUpperInvariant();

        if (cleanKey.Length != KeyLength)
            return false;

        return cleanKey.All(c => AllowedChars.Contains(c));
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Security/RecoveryKeyGenerator.cs
// =============================================================================