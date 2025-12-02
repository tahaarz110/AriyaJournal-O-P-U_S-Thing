// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/PluginStateConfiguration.cs
// شماره فایل: 47
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول PluginState
/// </summary>
public class PluginStateConfiguration : IEntityTypeConfiguration<PluginState>
{
    public void Configure(EntityTypeBuilder<PluginState> builder)
    {
        builder.ToTable("PluginStates");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.PluginId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.PluginId)
            .IsUnique();

        builder.Property(p => p.PluginName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Version)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(p => p.Settings)
            .HasMaxLength(10000);

        builder.Property(p => p.InstalledAt)
            .HasDefaultValueSql("datetime('now')");
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/PluginStateConfiguration.cs
// =============================================================================