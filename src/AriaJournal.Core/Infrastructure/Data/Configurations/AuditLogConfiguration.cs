// ═══════════════════════════════════════════════════════════════════════
// فایل: AuditLogConfiguration.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Data/Configurations/AuditLogConfiguration.cs
// توضیح: تنظیمات Entity Framework برای AuditLog
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// تنظیمات جدول AuditLogs
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // نام جدول
        builder.ToTable("AuditLogs");

        // کلید اصلی
        builder.HasKey(e => e.Id);

        // شناسه - Auto Increment
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // شناسه کاربر - اختیاری (برای عملیات سیستمی)
        builder.Property(e => e.UserId);

        // نوع عملیات - الزامی
        builder.Property(e => e.Action)
            .IsRequired()
            .HasConversion<int>();

        // نوع Entity - الزامی
        builder.Property(e => e.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        // شناسه Entity - اختیاری
        builder.Property(e => e.EntityId);

        // مقادیر قبلی (JSON)
        builder.Property(e => e.OldValues)
            .HasMaxLength(10000);

        // مقادیر جدید (JSON)
        builder.Property(e => e.NewValues)
            .HasMaxLength(10000);

        // توضیحات
        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        // آدرس IP
        builder.Property(e => e.IpAddress)
            .HasMaxLength(50);

        // نام دستگاه
        builder.Property(e => e.MachineName)
            .HasMaxLength(100);

        // نسخه نرم‌افزار
        builder.Property(e => e.AppVersion)
            .HasMaxLength(20);

        // شناسه پلاگین
        builder.Property(e => e.PluginId)
            .HasMaxLength(100);

        // سطح اهمیت
        builder.Property(e => e.Level)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(AuditLevel.Info);

        // موفقیت
        builder.Property(e => e.Success)
            .IsRequired()
            .HasDefaultValue(true);

        // پیام خطا
        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        // زمان عملیات
        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')");

        // مدت زمان
        builder.Property(e => e.DurationMs);

        // رابطه با User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ایندکس‌ها
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        builder.HasIndex(e => e.Action)
            .HasDatabaseName("IX_AuditLogs_Action");

        builder.HasIndex(e => e.EntityType)
            .HasDatabaseName("IX_AuditLogs_EntityType");

        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        builder.HasIndex(e => e.Level)
            .HasDatabaseName("IX_AuditLogs_Level");

        builder.HasIndex(e => new { e.EntityType, e.EntityId })
            .HasDatabaseName("IX_AuditLogs_Entity");

        builder.HasIndex(e => new { e.UserId, e.Timestamp })
            .HasDatabaseName("IX_AuditLogs_User_Time");
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: AuditLogConfiguration.cs
// ═══════════════════════════════════════════════════════════════════════