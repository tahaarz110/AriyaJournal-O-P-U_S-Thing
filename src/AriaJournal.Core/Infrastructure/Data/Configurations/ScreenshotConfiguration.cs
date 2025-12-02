// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/ScreenshotConfiguration.cs
// شماره فایل: 44
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول Screenshot
/// </summary>
public class ScreenshotConfiguration : IEntityTypeConfiguration<Screenshot>
{
    public void Configure(EntityTypeBuilder<Screenshot> builder)
    {
        builder.ToTable("Screenshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        builder.Property(s => s.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(s => s.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.ImageType)
            .HasMaxLength(50);

        builder.Property(s => s.Timeframe)
            .HasMaxLength(10);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        // ایندکس
        builder.HasIndex(s => s.TradeId);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/ScreenshotConfiguration.cs
// =============================================================================