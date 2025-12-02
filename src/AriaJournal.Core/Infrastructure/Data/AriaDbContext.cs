// ═══════════════════════════════════════════════════════════════════════
// فایل: AriaDbContext.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Data/AriaDbContext.cs
// توضیح: DbContext اصلی - نسخه کامل با Entity های جدید
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using AriaJournal.Core.Domain.Entities;
using AriaJournal.Core.Infrastructure.Data.Configurations;

namespace AriaJournal.Core.Infrastructure.Data;

/// <summary>
/// DbContext اصلی آریا ژورنال
/// </summary>
public class AriaDbContext : DbContext
{
    private readonly string _connectionString;

    public AriaDbContext()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbFolder = Path.Combine(appDataPath, "AriaJournal");
        
        if (!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }

        var dbPath = Path.Combine(dbFolder, "ariajournal.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public AriaDbContext(DbContextOptions<AriaDbContext> options) : base(options)
    {
        _connectionString = string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════
    // DbSets - جداول اصلی
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// کاربران
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// حساب‌های معاملاتی
    /// </summary>
    public DbSet<Account> Accounts => Set<Account>();

    /// <summary>
    /// معاملات
    /// </summary>
    public DbSet<Trade> Trades => Set<Trade>();

    /// <summary>
    /// تعریف فیلدهای سفارشی
    /// </summary>
    public DbSet<FieldDefinition> FieldDefinitions => Set<FieldDefinition>();

    /// <summary>
    /// مقادیر فیلدهای سفارشی معاملات
    /// </summary>
    public DbSet<TradeCustomField> TradeCustomFields => Set<TradeCustomField>();

    /// <summary>
    /// اسکرین‌شات‌ها
    /// </summary>
    public DbSet<Screenshot> Screenshots => Set<Screenshot>();

    /// <summary>
    /// تنظیمات
    /// </summary>
    public DbSet<Settings> Settings => Set<Settings>();

    /// <summary>
    /// سطل زباله
    /// </summary>
    public DbSet<RecycleBin> RecycleBin => Set<RecycleBin>();

    /// <summary>
    /// وضعیت پلاگین‌ها
    /// </summary>
    public DbSet<PluginState> PluginStates => Set<PluginState>();

    /// <summary>
    /// تاریخچه Migration
    /// </summary>
    public DbSet<MigrationHistory> MigrationHistory => Set<MigrationHistory>();

    // ═══════════════════════════════════════════════════════════════
    // DbSets - جداول جدید
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// رویدادهای مدیریت معامله
    /// </summary>
    public DbSet<TradeEvent> TradeEvents => Set<TradeEvent>();

    /// <summary>
    /// احساسات
    /// </summary>
    public DbSet<Emotion> Emotions => Set<Emotion>();

    /// <summary>
    /// لاگ‌های عملیات
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ═══════════════════════════════════════════════════════════════
    // پیکربندی
    // ═══════════════════════════════════════════════════════════════

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
            
            #if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
            #endif
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // اعمال Configurations موجود
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new TradeConfiguration());
        modelBuilder.ApplyConfiguration(new FieldDefinitionConfiguration());
        modelBuilder.ApplyConfiguration(new TradeCustomFieldConfiguration());
        modelBuilder.ApplyConfiguration(new ScreenshotConfiguration());
        modelBuilder.ApplyConfiguration(new SettingsConfiguration());
        modelBuilder.ApplyConfiguration(new RecycleBinConfiguration());
        modelBuilder.ApplyConfiguration(new PluginStateConfiguration());
        modelBuilder.ApplyConfiguration(new MigrationHistoryConfiguration());

        // اعمال Configurations جدید
        modelBuilder.ApplyConfiguration(new TradeEventConfiguration());
        modelBuilder.ApplyConfiguration(new EmotionConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }

    // ═══════════════════════════════════════════════════════════════
    // متدهای کمکی
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// ذخیره تغییرات با ثبت زمان
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// ذخیره تغییرات با ثبت زمان (async)
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// بروزرسانی خودکار زمان‌ها
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // تنظیم UpdatedAt برای Entity های Modified
            if (entry.State == EntityState.Modified)
            {
                var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProperty != null)
                {
                    updatedAtProperty.CurrentValue = DateTime.Now;
                }
            }

            // تنظیم CreatedAt برای Entity های Added
            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProperty != null && createdAtProperty.CurrentValue == null)
                {
                    createdAtProperty.CurrentValue = DateTime.Now;
                }
            }
        }
    }

    /// <summary>
    /// اطمینان از ایجاد دیتابیس
    /// </summary>
    public async Task EnsureCreatedAsync()
    {
        await Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// اجرای Migration ها
    /// </summary>
    public async Task MigrateAsync()
    {
        await Database.MigrateAsync();
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: AriaDbContext.cs
// ═══════════════════════════════════════════════════════════════════════