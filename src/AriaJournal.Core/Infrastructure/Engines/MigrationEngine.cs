// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Engines/MigrationEngine.cs
// شماره فایل: 59
// توضیح: موتور مایگریشن دیتابیس
// =============================================================================

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Common;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Domain.Interfaces.Engines;
using AriaJournal.Core.Infrastructure.Data;

namespace AriaJournal.Core.Infrastructure.Engines;

/// <summary>
/// پیاده‌سازی موتور مایگریشن
/// </summary>
public class MigrationEngine : IMigrationEngine
{
    private readonly AriaDbContext _context;
    private readonly List<IMigration> _migrations;
    private int _currentVersion = -1;
    private const int LatestVersion = 1; // نسخه فعلی Schema

    public int CurrentVersion
    {
        get
        {
            if (_currentVersion < 0)
            {
                _currentVersion = GetCurrentVersionFromDb();
            }
            return _currentVersion;
        }
    }

    public bool NeedsMigration => CurrentVersion < LatestVersion;

    public MigrationEngine(AriaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _migrations = new List<IMigration>();

        // ثبت مایگریشن‌های پیش‌فرض
        RegisterDefaultMigrations();
    }

    public async Task<Result<bool>> MigrateAsync()
    {
        try
        {
            // ابتدا مطمئن شو دیتابیس وجود دارد
            await _context.Database.EnsureCreatedAsync();

            // اگر جدول MigrationHistory وجود ندارد، ایجاد کن
            await EnsureMigrationHistoryTableAsync();

            if (!NeedsMigration)
            {
                return Result.Success(true);
            }

            var pendingMigrations = _migrations
                .Where(m => m.Version > CurrentVersion)
                .OrderBy(m => m.Version)
                .ToList();

            foreach (var migration in pendingMigrations)
            {
                var stopwatch = Stopwatch.StartNew();
                var history = new MigrationHistory
                {
                    Version = migration.Version,
                    Name = migration.Name,
                    Description = migration.Description,
                    AppliedAt = DateTime.Now
                };

                try
                {
                    await migration.UpAsync();
                    
                    stopwatch.Stop();
                    history.IsSuccessful = true;
                    history.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                    _context.MigrationHistories.Add(history);
                    await _context.SaveChangesAsync();

                    _currentVersion = migration.Version;

                    System.Diagnostics.Debug.WriteLine($"مایگریشن {migration.Name} با موفقیت اجرا شد ({stopwatch.ElapsedMilliseconds}ms)");
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    history.IsSuccessful = false;
                    history.ErrorMessage = ex.Message;
                    history.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                    _context.MigrationHistories.Add(history);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"خطا در مایگریشن {migration.Name}: {ex.Message}");
                    return Result.Failure<bool>(Error.Custom("Migration.Failed", $"خطا در مایگریشن {migration.Name}: {ex.Message}"));
                }
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در مایگریشن: {ex.Message}");
            return Result.Failure<bool>(Error.Internal($"خطا در عملیات مایگریشن: {ex.Message}"));
        }
    }

    public void RegisterMigration(IMigration migration)
    {
        if (migration == null)
            throw new ArgumentNullException(nameof(migration));

        // حذف مایگریشن قبلی با همان نسخه
        _migrations.RemoveAll(m => m.Version == migration.Version);
        _migrations.Add(migration);
    }

    public IEnumerable<MigrationInfo> GetMigrations()
    {
        var appliedMigrations = _context.MigrationHistories
            .AsNoTracking()
            .ToDictionary(m => m.Version);

        return _migrations
            .OrderBy(m => m.Version)
            .Select(m =>
            {
                appliedMigrations.TryGetValue(m.Version, out var history);
                return new MigrationInfo
                {
                    Version = m.Version,
                    Name = m.Name,
                    Description = m.Description,
                    IsApplied = history?.IsSuccessful == true,
                    AppliedAt = history?.AppliedAt
                };
            })
            .ToList();
    }

    public async Task<Result<bool>> RollbackAsync(int targetVersion)
    {
        if (targetVersion < 0)
        {
            return Result.Failure<bool>(Error.Validation("نسخه هدف نامعتبر است"));
        }

        if (targetVersion >= CurrentVersion)
        {
            return Result.Failure<bool>(Error.Validation("نسخه هدف باید کمتر از نسخه فعلی باشد"));
        }

        try
        {
            var migrationsToRollback = _migrations
                .Where(m => m.Version > targetVersion && m.Version <= CurrentVersion)
                .OrderByDescending(m => m.Version)
                .ToList();

            foreach (var migration in migrationsToRollback)
            {
                try
                {
                    await migration.DownAsync();

                    // حذف از تاریخچه
                    var history = await _context.MigrationHistories
                        .FirstOrDefaultAsync(h => h.Version == migration.Version);

                    if (history != null)
                    {
                        _context.MigrationHistories.Remove(history);
                        await _context.SaveChangesAsync();
                    }

                    System.Diagnostics.Debug.WriteLine($"Rollback مایگریشن {migration.Name} انجام شد");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"خطا در Rollback مایگریشن {migration.Name}: {ex.Message}");
                    return Result.Failure<bool>(Error.Custom("Migration.RollbackFailed", $"خطا در Rollback: {ex.Message}"));
                }
            }

            _currentVersion = targetVersion;
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(Error.Internal($"خطا در عملیات Rollback: {ex.Message}"));
        }
    }

    #region Private Methods

    private int GetCurrentVersionFromDb()
    {
        try
        {
            // بررسی وجود جدول
            var tableExists = _context.Database
                .ExecuteSqlRaw("SELECT name FROM sqlite_master WHERE type='table' AND name='MigrationHistories'");

            if (tableExists == 0)
            {
                return 0;
            }

            var latestMigration = _context.MigrationHistories
                .AsNoTracking()
                .Where(m => m.IsSuccessful)
                .OrderByDescending(m => m.Version)
                .FirstOrDefault();

            return latestMigration?.Version ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private async Task EnsureMigrationHistoryTableAsync()
    {
        try
        {
            // این کار توسط EF Core انجام می‌شود
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد جدول MigrationHistory: {ex.Message}");
        }
    }

    private void RegisterDefaultMigrations()
    {
        // مایگریشن اولیه - ایجاد Schema پایه
        RegisterMigration(new InitialMigration(_context));
    }

    #endregion
}

/// <summary>
/// مایگریشن اولیه - ایجاد Schema پایه
/// </summary>
internal class InitialMigration : IMigration
{
    private readonly AriaDbContext _context;

    public int Version => 1;
    public string Name => "InitialCreate";
    public string Description => "ایجاد جداول اولیه دیتابیس";

    public InitialMigration(AriaDbContext context)
    {
        _context = context;
    }

    public async Task UpAsync()
    {
        // EF Core به صورت خودکار جداول را ایجاد می‌کند
        await _context.Database.EnsureCreatedAsync();

        // ایجاد ایندکس‌های اضافی
        await CreateIndexesAsync();
    }

    public async Task DownAsync()
    {
        // حذف دیتابیس (برای توسعه)
        await _context.Database.EnsureDeletedAsync();
    }

    private async Task CreateIndexesAsync()
    {
        try
        {
            // ایندکس‌های اضافی که در Configuration تعریف نشده‌اند
            var sql = @"
                CREATE INDEX IF NOT EXISTS IX_Trades_EntryTime_AccountId ON Trades(EntryTime, AccountId);
                CREATE INDEX IF NOT EXISTS IX_Trades_Symbol_Direction ON Trades(Symbol, Direction);
            ";

            await _context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطا در ایجاد ایندکس‌ها: {ex.Message}");
        }
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Engines/MigrationEngine.cs
// =============================================================================