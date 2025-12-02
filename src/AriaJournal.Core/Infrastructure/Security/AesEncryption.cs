// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Security/AesEncryption.cs
// شماره فایل: 37
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using System.IO;
namespace AriaJournal.Core.Infrastructure.Security;

/// <summary>
/// رمزنگاری AES-256
/// </summary>
public static class AesEncryption
{
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int SaltSize = 32;
    private const int Iterations = 10000;

    /// <summary>
    /// رمزنگاری متن
    /// </summary>
    public static string Encrypt(string plainText, string password)
    {
        if (string.IsNullOrEmpty(plainText))
            throw new ArgumentException("متن نمی‌تواند خالی باشد", nameof(plainText));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

        var salt = GenerateSalt();
        var key = DeriveKey(password, salt);
        var iv = GenerateIV();

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // ترکیب: Salt + IV + EncryptedData
        var result = new byte[salt.Length + iv.Length + encryptedBytes.Length];
        Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
        Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, salt.Length + iv.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// رمزگشایی متن
    /// </summary>
    public static string Decrypt(string cipherText, string password)
    {
        if (string.IsNullOrEmpty(cipherText))
            throw new ArgumentException("متن رمزشده نمی‌تواند خالی باشد", nameof(cipherText));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

        var cipherBytes = Convert.FromBase64String(cipherText);

        // استخراج Salt, IV, EncryptedData
        var salt = new byte[SaltSize];
        var iv = new byte[BlockSize / 8];
        var encryptedBytes = new byte[cipherBytes.Length - salt.Length - iv.Length];

        Buffer.BlockCopy(cipherBytes, 0, salt, 0, salt.Length);
        Buffer.BlockCopy(cipherBytes, salt.Length, iv, 0, iv.Length);
        Buffer.BlockCopy(cipherBytes, salt.Length + iv.Length, encryptedBytes, 0, encryptedBytes.Length);

        var key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// رمزنگاری فایل
    /// </summary>
    public static async Task EncryptFileAsync(string inputPath, string outputPath, string password)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("فایل ورودی یافت نشد", inputPath);

        var salt = GenerateSalt();
        var key = DeriveKey(password, salt);
        var iv = GenerateIV();

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        await using var outputStream = new FileStream(outputPath, FileMode.Create);
        
        // نوشتن Salt و IV در ابتدای فایل
        await outputStream.WriteAsync(salt);
        await outputStream.WriteAsync(iv);

        await using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await using var inputStream = new FileStream(inputPath, FileMode.Open);
        
        await inputStream.CopyToAsync(cryptoStream);
    }

    /// <summary>
    /// رمزگشایی فایل
    /// </summary>
    public static async Task DecryptFileAsync(string inputPath, string outputPath, string password)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("فایل ورودی یافت نشد", inputPath);

        await using var inputStream = new FileStream(inputPath, FileMode.Open);

        // خواندن Salt و IV
        var salt = new byte[SaltSize];
        var iv = new byte[BlockSize / 8];
        
        await inputStream.ReadAsync(salt.AsMemory(0, salt.Length));
        await inputStream.ReadAsync(iv.AsMemory(0, iv.Length));

        var key = DeriveKey(password, salt);

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        await using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        await using var outputStream = new FileStream(outputPath, FileMode.Create);
        
        await cryptoStream.CopyToAsync(outputStream);
    }

    /// <summary>
    /// تولید Salt تصادفی
    /// </summary>
    private static byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// تولید IV تصادفی
    /// </summary>
    private static byte[] GenerateIV()
    {
        var iv = new byte[BlockSize / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(iv);
        return iv;
    }

    /// <summary>
    /// استخراج کلید از رمز عبور
    /// </summary>
    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256);
        
        return pbkdf2.GetBytes(KeySize / 8);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Security/AesEncryption.cs
// =============================================================================