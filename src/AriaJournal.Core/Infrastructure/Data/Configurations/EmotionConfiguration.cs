// ═══════════════════════════════════════════════════════════════════════
// فایل: EmotionConfiguration.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Data/Configurations/EmotionConfiguration.cs
// توضیح: تنظیمات Entity Framework برای Emotion
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// تنظیمات جدول Emotions
/// </summary>
public class EmotionConfiguration : IEntityTypeConfiguration<Emotion>
{
    public void Configure(EntityTypeBuilder<Emotion> builder)
    {
        // نام جدول
        builder.ToTable("Emotions");

        // کلید اصلی
        builder.HasKey(e => e.Id);

        // شناسه - Auto Increment
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        // شناسه کاربر - الزامی
        builder.Property(e => e.UserId)
            .IsRequired();

        // شناسه معامله - اختیاری
        builder.Property(e => e.TradeId);

        // زمان ثبت - الزامی
        builder.Property(e => e.RecordedAt)
            .IsRequired();

        // نوع احساس - الزامی
        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>();

        // شدت - الزامی با مقدار پیش‌فرض
        builder.Property(e => e.Intensity)
            .IsRequired()
            .HasDefaultValue(5);

        // مرحله معامله
        builder.Property(e => e.Phase)
            .IsRequired()
            .HasConversion<int>();

        // توضیحات
        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        // تگ‌ها
        builder.Property(e => e.Tags)
            .HasMaxLength(500);

        // آیا منجر به اشتباه شد؟
        builder.Property(e => e.LedToMistake);

        // درس آموخته‌شده
        builder.Property(e => e.LessonLearned)
            .HasMaxLength(2000);

        // زمان ایجاد
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("datetime('now')");

        // رابطه با User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // رابطه با Trade
        builder.HasOne(e => e.Trade)
            .WithMany(t => t.Emotions)
            .HasForeignKey(e => e.TradeId)
            .OnDelete(DeleteBehavior.SetNull);

        // ایندکس‌ها
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_Emotions_UserId");

        builder.HasIndex(e => e.TradeId)
            .HasDatabaseName("IX_Emotions_TradeId");

        builder.HasIndex(e => e.RecordedAt)
            .HasDatabaseName("IX_Emotions_RecordedAt");

        builder.HasIndex(e => e.Type)
            .HasDatabaseName("IX_Emotions_Type");

        builder.HasIndex(e => new { e.UserId, e.RecordedAt })
            .HasDatabaseName("IX_Emotions_User_Time");
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: EmotionConfiguration.cs
// ═══════════════════════════════════════════════════════════════════════