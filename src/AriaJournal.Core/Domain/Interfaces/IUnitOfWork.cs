// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/IUnitOfWork.cs
// شماره فایل: 21
// =============================================================================

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// اینترفیس Unit of Work
/// </summary>
public interface IUnitOfWork : IDisposable
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
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/IUnitOfWork.cs
// =============================================================================