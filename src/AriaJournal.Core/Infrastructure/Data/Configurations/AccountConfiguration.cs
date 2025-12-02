// =============================================================================
// فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/AccountConfiguration.cs
// شماره فایل: 40
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AriaJournal.Core.Domain.Entities;

namespace AriaJournal.Core.Infrastructure.Data.Configurations;

/// <summary>
/// پیکربندی جدول Account
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedOnAdd();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.BrokerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AccountNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Server)
            .HasMaxLength(100);

        builder.Property(a => a.InitialBalance)
            .HasPrecision(18, 2);

        builder.Property(a => a.CurrentBalance)
            .HasPrecision(18, 2);

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(a => a.Leverage)
            .HasDefaultValue(100);

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.Property(a => a.IsDefault)
            .HasDefaultValue(false);

        builder.Property(a => a.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        // ایندکس
        builder.HasIndex(a => new { a.UserId, a.Name });
        builder.HasIndex(a => a.IsDefault);

        // روابط
        builder.HasMany(a => a.Trades)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// =============================================================================
// پایان فایل: src/AriaJournal.Core/Infrastructure/Data/Configurations/AccountConfiguration.cs
// =============================================================================