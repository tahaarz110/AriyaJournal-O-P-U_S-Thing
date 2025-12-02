// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Security/PasswordHasher.cs
// شماره فایل: 35
// =============================================================================

using BCrypt.Net;

namespace AriaJournal.Core.Infrastructure.Security;

/// <summary>
/// کلاس هش کردن رمز عبور
/// </summary>
public static class PasswordHasher
{
    // تعداد راندهای هش (بالاتر = امن‌تر ولی کندتر)
    private const int WorkFactor = 12;

    /// <summary>
    /// هش کردن رمز عبور
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// بررسی صحت رمز عبور
    /// </summary>
    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// بررسی نیاز به هش مجدد (در صورت تغییر WorkFactor)
    /// </summary>
    public static bool NeedsRehash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;

        try
        {
            // بررسی work factor در هش
            var parts = hash.Split('$');
            if (parts.Length < 4)
                return true;

            if (int.TryParse(parts[2], out int currentWorkFactor))
            {
                return currentWorkFactor < WorkFactor;
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// اعتبارسنجی قدرت رمز عبور
    /// </summary>
    public static PasswordStrength CheckStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordStrength.VeryWeak;

        int score = 0;

        // طول
        if (password.Length >= 6) score++;
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;

        // حروف بزرگ
        if (password.Any(char.IsUpper)) score++;

        // حروف کوچک
        if (password.Any(char.IsLower)) score++;

        // اعداد
        if (password.Any(char.IsDigit)) score++;

        // کاراکترهای خاص
        if (password.Any(c => !char.IsLetterOrDigit(c))) score++;

        return score switch
        {
            <= 2 => PasswordStrength.VeryWeak,
            3 => PasswordStrength.Weak,
            4 => PasswordStrength.Medium,
            5 => PasswordStrength.Strong,
            _ => PasswordStrength.VeryStrong
        };
    }
}

/// <summary>
/// قدرت رمز عبور
/// </summary>
public enum PasswordStrength
{
    VeryWeak = 1,
    Weak = 2,
    Medium = 3,
    Strong = 4,
    VeryStrong = 5
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Security/PasswordHasher.cs
// =============================================================================