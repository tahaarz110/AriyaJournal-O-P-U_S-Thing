// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Repositories/GenericRepository.cs
// توضیح: پیاده‌سازی Repository عمومی - نسخه کامل با همه متدها
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Interfaces;
using AriaJournal.Core.Infrastructure.Data;

namespace AriaJournal.Core.Infrastructure.Repositories;

/// <summary>
/// پیاده‌سازی Repository عمومی
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AriaDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AriaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    #region Read Operations

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, "Id");
        var constant = Expression.Constant(id);
        var equal = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync<TKey>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TKey>> orderBy,
        bool descending = false)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        query = descending 
            ? query.OrderByDescending(orderBy) 
            : query.OrderBy(orderBy);

        return await query.ToListAsync();
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = descending 
                ? query.OrderByDescending(orderBy) 
                : query.OrderBy(orderBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>>? orderBy,
        bool descending,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null)
        {
            query = descending 
                ? query.OrderByDescending(orderBy) 
                : query.OrderBy(orderBy);
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).LastOrDefaultAsync();
    }

    public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }

    /// <summary>
    /// یافتن با شرط (Alias برای FirstOrDefaultAsync)
    /// </summary>
    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// یافتن با شرط و Include
    /// </summary>
    public virtual async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        return await FirstOrDefaultAsync(predicate, includes);
    }

    #endregion

    #region Aggregate Operations

    public virtual async Task<bool> AnyAsync()
    {
        return await _dbSet.AnyAsync();
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <summary>
    /// بررسی وجود (Alias برای AnyAsync)
    /// </summary>
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await AnyAsync(predicate);
    }

    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public virtual async Task<long> LongCountAsync()
    {
        return await _dbSet.LongCountAsync();
    }

    public virtual async Task<long> LongCountAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.LongCountAsync(predicate);
    }

    public virtual async Task<TResult?> MaxAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        if (!await _dbSet.AnyAsync())
            return default;
        return await _dbSet.MaxAsync(selector);
    }

    public virtual async Task<TResult?> MaxAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector)
    {
        var query = _dbSet.Where(predicate);
        if (!await query.AnyAsync())
            return default;
        return await query.MaxAsync(selector);
    }

    public virtual async Task<TResult?> MinAsync<TResult>(Expression<Func<T, TResult>> selector)
    {
        if (!await _dbSet.AnyAsync())
            return default;
        return await _dbSet.MinAsync(selector);
    }

    public virtual async Task<TResult?> MinAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector)
    {
        var query = _dbSet.Where(predicate);
        if (!await query.AnyAsync())
            return default;
        return await query.MinAsync(selector);
    }

    public virtual async Task<decimal> SumAsync(Expression<Func<T, decimal>> selector)
    {
        if (!await _dbSet.AnyAsync())
            return 0;
        return await _dbSet.SumAsync(selector);
    }

    public virtual async Task<decimal> SumAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector)
    {
        var query = _dbSet.Where(predicate);
        if (!await query.AnyAsync())
            return 0;
        return await query.SumAsync(selector);
    }

    public virtual async Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector)
    {
        if (!await _dbSet.AnyAsync())
            return null;
        return await _dbSet.AverageAsync(selector);
    }

    public virtual async Task<decimal?> AverageAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector)
    {
        var query = _dbSet.Where(predicate);
        if (!await query.AnyAsync())
            return null;
        return await query.AverageAsync(selector);
    }

    #endregion

    #region Write Operations

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        return result.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void UpdateRange(IEnumerable<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<T> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public virtual async Task DeleteWhereAsync(Expression<Func<T, bool>> predicate)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync();
        _dbSet.RemoveRange(entities);
    }

    #endregion

    #region Query Operations

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }

    public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public virtual IQueryable<T> QueryNoTracking()
    {
        return _dbSet.AsNoTracking();
    }

    public virtual IQueryable<T> QueryNoTracking(Expression<Func<T, bool>> predicate)
    {
        return _dbSet.AsNoTracking().Where(predicate);
    }

    public virtual async Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters)
    {
        return await _dbSet.FromSqlRaw(sql, parameters).ToListAsync();
    }

    #endregion

    #region Bulk Operations

    public virtual async Task<T> AddOrUpdateAsync(T entity, Expression<Func<T, bool>> predicate)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(predicate);
        
        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(entity);
            return existing;
        }
        else
        {
            var result = await _dbSet.AddAsync(entity);
            return result.Entity;
        }
    }

    public virtual async Task AddOrUpdateRangeAsync(
        IEnumerable<T> entities, 
        Func<T, Expression<Func<T, bool>>> predicateFactory)
    {
        foreach (var entity in entities)
        {
            var predicate = predicateFactory(entity);
            await AddOrUpdateAsync(entity, predicate);
        }
    }

    #endregion

    #region Attach/Detach

    public virtual void Attach(T entity)
    {
        _dbSet.Attach(entity);
    }

    public virtual void Detach(T entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    public virtual async Task ReloadAsync(T entity)
    {
        await _context.Entry(entity).ReloadAsync();
    }

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Repositories/GenericRepository.cs
// =============================================================================