// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/SettingsConfiguration.cs
// شماره فایل: 45
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول Settings
/// </summary>
public class SettingsConfiguration : IEntityTypeConfiguration<Settings>
{
    public void Configure(EntityTypeBuilder<Settings> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Theme)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Dark");

        builder.Property(s => s.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("fa");

        builder.Property(s => s.DateFormat)
            .HasMaxLength(20)
            .HasDefaultValue("yyyy/MM/dd");

        builder.Property(s => s.TimeFormat)
            .HasMaxLength(20)
            .HasDefaultValue("HH:mm");

        builder.Property(s => s.DefaultCurrency)
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(s => s.PriceDecimals)
            .HasDefaultValue(5);

        builder.Property(s => s.VolumeDecimals)
            .HasDefaultValue(2);

        builder.Property(s => s.AutoBackup)
            .HasDefaultValue(true);

        builder.Property(s => s.AutoBackupInterval)
            .HasDefaultValue(7);

        builder.Property(s => s.BackupPath)
            .HasMaxLength(500);

        builder.Property(s => s.ShowNotifications)
            .HasDefaultValue(true);

        builder.Property(s => s.PlaySounds)
            .HasDefaultValue(false);

        builder.Property(s => s.TradeListViewMode)
            .HasMaxLength(20)
            .HasDefaultValue("Table");

        builder.Property(s => s.ItemsPerPage)
            .HasDefaultValue(50);

        builder.Property(s => s.CustomSettings)
            .HasMaxLength(10000);

        // ایندکس
        builder.HasIndex(s => s.UserId)
            .IsUnique();
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/SettingsConfiguration.cs
// =============================================================================