// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/TradeCustomFieldConfiguration.cs
// شماره فایل: 43
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول TradeCustomField
/// </summary>
public class TradeCustomFieldConfiguration : IEntityTypeConfiguration<TradeCustomField>
{
    public void Configure(EntityTypeBuilder<TradeCustomField> builder)
    {
        builder.ToTable("TradeCustomFields");

        builder.HasKey(tcf => tcf.Id);

        builder.Property(tcf => tcf.Id)
            .ValueGeneratedOnAdd();

        builder.Property(tcf => tcf.Value)
            .HasMaxLength(4000);

        builder.Property(tcf => tcf.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        // ایندکس
        builder.HasIndex(tcf => new { tcf.TradeId, tcf.FieldDefinitionId })
            .IsUnique();
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/TradeCustomFieldConfiguration.cs
// =============================================================================