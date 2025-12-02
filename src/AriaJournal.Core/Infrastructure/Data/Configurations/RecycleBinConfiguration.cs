// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/RecycleBinConfiguration.cs
// شماره فایل: 46
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول RecycleBin
/// </summary>
public class RecycleBinConfiguration : IEntityTypeConfiguration<RecycleBin>
{
    public void Configure(EntityTypeBuilder<RecycleBin> builder)
    {
        builder.ToTable("RecycleBin");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.EntityData)
            .IsRequired();

        builder.Property(r => r.DeletedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.Property(r => r.IsRestored)
            .HasDefaultValue(false);

        // ایندکس
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.EntityType);
        builder.HasIndex(r => r.ExpiresAt);
        builder.HasIndex(r => r.IsRestored);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/RecycleBinConfiguration.cs
// =============================================================================