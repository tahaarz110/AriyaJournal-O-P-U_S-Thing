// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/FieldDefinitionConfiguration.cs
// توضیح: پیکربندی جدول FieldDefinition - اصلاح‌شده
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول FieldDefinition
/// </summary>
public class FieldDefinitionConfiguration : IEntityTypeConfiguration<FieldDefinition>
{
    public void Configure(EntityTypeBuilder<FieldDefinition> builder)
    {
        builder.ToTable("FieldDefinitions");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.EntityType)
            .HasMaxLength(50)
            .HasDefaultValue("Trade");

        builder.Property(f => f.FieldName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Options)
            .HasMaxLength(4000);

        builder.Property(f => f.DefaultValue)
            .HasMaxLength(500);

        builder.Property(f => f.ValidationRules)
            .HasMaxLength(2000);

        builder.Property(f => f.HelpText)
            .HasMaxLength(500);

        builder.Property(f => f.Category)
            .HasMaxLength(50);

        builder.Property(f => f.Metadata)
            .HasMaxLength(4000);

        builder.Property(f => f.Order)
            .HasDefaultValue(0);

        builder.Property(f => f.IsActive)
            .HasDefaultValue(true);

        builder.Property(f => f.IsSystem)
            .HasDefaultValue(false);

        builder.Property(f => f.IsRequired)
            .HasDefaultValue(false);

        builder.Property(f => f.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        // ایندکس
        builder.HasIndex(f => new { f.UserId, f.FieldName })
            .IsUnique();

        builder.HasIndex(f => f.Category);

        builder.HasIndex(f => f.EntityType);

        // روابط
        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.TradeCustomFields)
            .WithOne(tcf => tcf.FieldDefinition)
            .HasForeignKey(tcf => tcf.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/FieldDefinitionConfiguration.cs
// =============================================================================