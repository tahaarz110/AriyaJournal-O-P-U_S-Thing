// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/TradeConfiguration.cs
// شماره فایل: 41
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول Trade
/// </summary>
public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Ticket)
            .HasMaxLength(50);

        builder.Property(t => t.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Volume)
            .HasPrecision(18, 4);

        builder.Property(t => t.EntryPrice)
            .HasPrecision(18, 8);

        builder.Property(t => t.ExitPrice)
            .HasPrecision(18, 8);

        builder.Property(t => t.StopLoss)
            .HasPrecision(18, 8);

        builder.Property(t => t.TakeProfit)
            .HasPrecision(18, 8);

        builder.Property(t => t.Commission)
            .HasPrecision(18, 4);

        builder.Property(t => t.Swap)
            .HasPrecision(18, 4);

        builder.Property(t => t.ProfitLoss)
            .HasPrecision(18, 4);

        builder.Property(t => t.ProfitLossPips)
            .HasPrecision(18, 2);

        builder.Property(t => t.RiskRewardRatio)
            .HasPrecision(10, 2);

        builder.Property(t => t.RiskPercent)
            .HasPrecision(10, 4);

        builder.Property(t => t.PreTradeNotes)
            .HasMaxLength(2000);

        builder.Property(t => t.PostTradeNotes)
            .HasMaxLength(2000);

        builder.Property(t => t.EntryReason)
            .HasMaxLength(1000);

        builder.Property(t => t.ExitReason)
            .HasMaxLength(1000);

        builder.Property(t => t.Mistakes)
            .HasMaxLength(2000);

        builder.Property(t => t.Lessons)
            .HasMaxLength(2000);

        builder.Property(t => t.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        // ایندکس‌ها
        builder.HasIndex(t => t.AccountId);
        builder.HasIndex(t => t.Symbol);
        builder.HasIndex(t => t.EntryTime);
        builder.HasIndex(t => t.IsDeleted);
        builder.HasIndex(t => new { t.AccountId, t.EntryTime });

        // روابط
        builder.HasMany(t => t.CustomFields)
            .WithOne(cf => cf.Trade)
            .HasForeignKey(cf => cf.TradeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Screenshots)
            .WithOne(s => s.Trade)
            .HasForeignKey(s => s.TradeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/TradeConfiguration.cs
// =============================================================================