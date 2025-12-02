// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IDataEngine.cs
// شماره فایل: 26
// =============================================================================

namespace AriaJournal.Core.Domain.Interfaces.Engines;

/// <summary>
/// موتور مدیریت داده
/// </summary>
public interface IDataEngine
{
    /// <summary>
    /// دریافت Repository برای یک Entity
    /// </summary>
    IGenericRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// ذخیره تغییرات
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// شروع تراکنش
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// تأیید تراکنش
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// لغو تراکنش
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// آیا تراکنش فعال است
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// اجرای SQL خام
    /// </summary>
    Task<int> ExecuteSqlAsync(string sql, params object[] parameters);

    /// <summary>
    /// دریافت اتصال دیتابیس
    /// </summary>
    string GetConnectionString();
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/Engines/IDataEngine.cs
// =============================================================================