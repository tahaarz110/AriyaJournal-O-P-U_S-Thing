// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IMigrationEngine.cs
// شماره فایل: 29
// =============================================================================

using AriaJournal.Core.Domain.Common;

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور مایگریشن دیتابیس
/// </summary>
public interface IMigrationEngine
{
    /// <summary>
    /// اجرای مایگریشن
    /// </summary>
    Task<Result<bool>> MigrateAsync();

    /// <summary>
    /// نسخه فعلی دیتابیس
    /// </summary>
    int CurrentVersion { get; }

    /// <summary>
    /// آیا نیاز به مایگریشن است
    /// </summary>
    bool NeedsMigration { get; }

    /// <summary>
    /// ثبت مایگریشن جدید
    /// </summary>
    void RegisterMigration(IMigration migration);

    /// <summary>
    /// دریافت لیست مایگریشن‌ها
    /// </summary>
    IEnumerable<MigrationInfo> GetMigrations();

    /// <summary>
    /// بازگشت به نسخه قبل
    /// </summary>
    Task<Result<bool>> RollbackAsync(int targetVersion);
}

/// <summary>
/// اینترفیس مایگریشن
/// </summary>
public interface IMigration
{
    int Version { get; }
    string Name { get; }
    string Description { get; }
    Task UpAsync();
    Task DownAsync();
}

/// <summary>
/// اطلاعات مایگریشن
/// </summary>
public class MigrationInfo
{
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsApplied { get; set; }
    public DateTime? AppliedAt { get; set; }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IMigrationEngine.cs
// =============================================================================