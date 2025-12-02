// =============================================================================
// فایل: src/AriaJournal.Core/Domain/Interfaces/IGenericRepository.cs
// توضیح: اینترفیس Repository عمومی - نسخه کامل با همه متدها
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AriaJournal.Core.Domain.Interfaces;

/// <summary>
/// اینترفیس Repository عمومی
/// </summary>
public interface IGenericRepository<T> where T : class
{
    #region Read Operations

    /// <summary>
    /// دریافت بر اساس شناسه
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// دریافت بر اساس شناسه با Include
    /// </summary>
    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// دریافت همه رکوردها
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// دریافت همه با شرط
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// دریافت همه با شرط و Include
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// دریافت همه با مرتب‌سازی
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync<TKey>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TKey>> orderBy,
        bool descending = false);

    /// <summary>
    /// دریافت با صفحه‌بندی
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false);

    /// <summary>
    /// دریافت با صفحه‌بندی و Include
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, object>>? orderBy,
        bool descending,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// دریافت اولین یا پیش‌فرض
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// دریافت اولین یا پیش‌فرض با Include
    /// </summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// دریافت آخرین یا پیش‌فرض
    /// </summary>
    Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// دریافت تکی یا پیش‌فرض
    /// </summary>
    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// یافتن با شرط (Alias برای FirstOrDefaultAsync)
    /// </summary>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// یافتن با شرط و Include
    /// </summary>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);

    #endregion

    #region Aggregate Operations

    /// <summary>
    /// بررسی وجود
    /// </summary>
    Task<bool> AnyAsync();

    /// <summary>
    /// بررسی وجود با شرط
    /// </summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// بررسی وجود (Alias برای AnyAsync)
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// شمارش
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// شمارش با شرط
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// شمارش بلند
    /// </summary>
    Task<long> LongCountAsync();

    /// <summary>
    /// شمارش بلند با شرط
    /// </summary>
    Task<long> LongCountAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// حداکثر
    /// </summary>
    Task<TResult?> MaxAsync<TResult>(Expression<Func<T, TResult>> selector);

    /// <summary>
    /// حداکثر با شرط
    /// </summary>
    Task<TResult?> MaxAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    /// <summary>
    /// حداقل
    /// </summary>
    Task<TResult?> MinAsync<TResult>(Expression<Func<T, TResult>> selector);

    /// <summary>
    /// حداقل با شرط
    /// </summary>
    Task<TResult?> MinAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector);

    /// <summary>
    /// مجموع
    /// </summary>
    Task<decimal> SumAsync(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// مجموع با شرط
    /// </summary>
    Task<decimal> SumAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector);

    /// <summary>
    /// میانگین
    /// </summary>
    Task<decimal?> AverageAsync(Expression<Func<T, decimal>> selector);

    /// <summary>
    /// میانگین با شرط
    /// </summary>
    Task<decimal?> AverageAsync(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, decimal>> selector);

    #endregion

    #region Write Operations

    /// <summary>
    /// افزودن
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// افزودن چندتایی
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// بروزرسانی
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// بروزرسانی چندتایی
    /// </summary>
    void UpdateRange(IEnumerable<T> entities);

    /// <summary>
    /// حذف
    /// </summary>
    void Delete(T entity);

    /// <summary>
    /// حذف چندتایی
    /// </summary>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// حذف با شرط
    /// </summary>
    Task DeleteWhereAsync(Expression<Func<T, bool>> predicate);

    #endregion

    #region Query Operations

    /// <summary>
    /// دریافت Query برای کوئری‌های پیچیده
    /// </summary>
    IQueryable<T> Query();

    /// <summary>
    /// دریافت Query با شرط
    /// </summary>
    IQueryable<T> Query(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// دریافت Query بدون Tracking
    /// </summary>
    IQueryable<T> QueryNoTracking();

    /// <summary>
    /// دریافت Query بدون Tracking با شرط
    /// </summary>
    IQueryable<T> QueryNoTracking(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// اجرای SQL خام
    /// </summary>
    Task<IEnumerable<T>> FromSqlRawAsync(string sql, params object[] parameters);

    #endregion

    #region Bulk Operations

    /// <summary>
    /// افزودن یا بروزرسانی
    /// </summary>
    Task<T> AddOrUpdateAsync(T entity, Expression<Func<T, bool>> predicate);

    /// <summary>
    /// افزودن یا بروزرسانی چندتایی
    /// </summary>
    Task AddOrUpdateRangeAsync(
        IEnumerable<T> entities, 
        Func<T, Expression<Func<T, bool>>> predicateFactory);

    #endregion

    #region Attach/Detach

    /// <summary>
    /// الصاق Entity
    /// </summary>
    void Attach(T entity);

    /// <summary>
    /// جدا کردن Entity
    /// </summary>
    void Detach(T entity);

    /// <summary>
    /// بارگذاری مجدد Entity
    /// </summary>
    Task ReloadAsync(T entity);

    #endregion
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Domain/Interfaces/IGenericRepository.cs
// =============================================================================