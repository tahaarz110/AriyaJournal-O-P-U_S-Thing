// ═══════════════════════════════════════════════════════════════════════
// فایل: TradeEventConfiguration.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Data/Configurations/TradeEventConfiguration.cs
// توضیح: تنظیمات Entity Framework برای TradeEvent
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// تنظیمات جدول TradeEvents
/// </summary>
public class TradeEventConfiguration : IEntityTypeConfiguration<TradeEvent>
{
    public void Configure(EntityTypeBuilder<TradeEvent> builder)
    {
        // نام جدول
        builder.ToTable("TradeEvents");

        // کلید اصلی
        builder.HasKey(e => e.Id);

        // شناسه - Auto Increment
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // شناسه معامله - الزامی
        builder.Property(e => e.TradeId)
            .IsRequired();

        // نوع رویداد - الزامی
        builder.Property(e => e.EventType)
            .IsRequired()
            .HasConversion<int>();

        // زمان رویداد - الزامی
        builder.Property(e => e.EventTime)
            .IsRequired();

        // قیمت - اختیاری
        builder.Property(e => e.Price)
            .HasPrecision(18, 8);

        // مقدار قبلی
        builder.Property(e => e.OldValue)
            .HasPrecision(18, 8);

        // مقدار جدید
        builder.Property(e => e.NewValue)
            .HasPrecision(18, 8);

        // تغییر حجم
        builder.Property(e => e.VolumeChange)
            .HasPrecision(18, 8);

        // دلیل
        builder.Property(e => e.Reason)
            .HasMaxLength(500);

        // یادداشت
        builder.Property(e => e.Note)
            .HasMaxLength(2000);

        // مسیر اسکرین‌شات
        builder.Property(e => e.ScreenshotPath)
            .HasMaxLength(500);

        // زمان ایجاد
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')");

        // رابطه با Trade
        builder.HasOne(e => e.Trade)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TradeId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با Emotion
        builder.HasOne(e => e.Emotion)
            .WithMany()
            .HasForeignKey(e => e.EmotionId)
            .OnDelete(DeleteBehavior.SetNull);

        // ایندکس‌ها
        builder.HasIndex(e => e.TradeId)
            .HasDatabaseName("IX_TradeEvents_TradeId");

        builder.HasIndex(e => e.EventTime)
            .HasDatabaseName("IX_TradeEvents_EventTime");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_TradeEvents_EventType");

        builder.HasIndex(e => new { e.TradeId, e.EventTime })
            .HasDatabaseName("IX_TradeEvents_Trade_Time");
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: TradeEventConfiguration.cs
// ═══════════════════════════════════════════════════════════════════════