// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/DataEngine.cs
// شماره فایل: 58
// توضیح: موتور مدیریت داده
// =============================================================================

using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;
using AriaJournal.Core.Infrastructure.Repositories;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور مدیریت داده
/// </summary>
public class DataEngine : IDataEngine, IDisposable
{
    private readonly AriaDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;
    private bool _disposed;

    public bool HasActiveTransaction => _transaction != null;

    public DataEngine(AriaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new Dictionary<Type, object>();
    }

    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            var repository = new GenericRepository<T>(_context);
            _repositories[type] = repository;
        }

        return (IGenericRepository<T>)_repositories[type];
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطای همزمانی: {ex.Message}");
            throw new InvalidOperationException("خطای همزمانی در بروزرسانی داده‌ها. لطفاً مجدداً تلاش کنید.", ex);
        }
        catch (DbUpdateException ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطای بروزرسانی: {ex.Message}");
            throw new InvalidOperationException("خطا در ذخیره‌سازی اطلاعات در دیتابیس.", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("یک تراکنش از قبل فعال است");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("تراکنش فعالی وجود ندارد");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task<int> ExecuteSqlAsync(string sql, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("دستور SQL نمی‌تواند خالی باشد", nameof(sql));
        }

        return await _context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    public string GetConnectionString()
    {
        return _context.Database.GetConnectionString() ?? string.Empty;
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _repositories.Clear();
            }

            _disposed = true;
        }
    }

    ~DataEngine()
    {
        Dispose(false);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/DataEngine.cs
// =============================================================================