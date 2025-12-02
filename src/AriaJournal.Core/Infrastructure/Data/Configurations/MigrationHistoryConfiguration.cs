// ═══════════════════════════════════════════════════════════════════════
// فایل: MigrationHistoryConfiguration.cs
// مسیر: src/AriaJournal.Core/Infrastructure/Data/Configurations/MigrationHistoryConfiguration.cs
// توضیح: تنظیمات Entity Framework برای MigrationHistory
// ═══════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// تنظیمات جدول MigrationHistory
/// </summary>
public class MigrationHistoryConfiguration : IEntityTypeConfiguration<MigrationHistory>
{
    public void Configure(EntityTypeBuilder<MigrationHistory> builder)
    {
        builder.ToTable("MigrationHistory");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Version)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.AppliedAt)
            .IsRequired();

        builder.Property(e => e.Success)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.Version)
            .IsUnique()
            .HasDatabaseName("IX_MigrationHistory_Version");
    }
}

// ═══════════════════════════════════════════════════════════════════════
// پایان فایل: MigrationHistoryConfiguration.cs
// ═══════════════════════════════════════════════════════════════════════