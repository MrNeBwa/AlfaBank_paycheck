using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы графика платежей по кредиту.
/// </summary>
public class CreditScheduleItemConfiguration : IEntityTypeConfiguration<CreditScheduleItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CreditScheduleItem> builder)
    {
        builder.ToTable("CreditScheduleItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Number)
            .IsRequired();

        builder.Property(item => item.DueDate)
            .IsRequired();

        builder.Property(item => item.PaymentAmount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(item => item.InterestAmount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(item => item.PrincipalAmount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(item => item.RemainingDebt)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(item => item.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(item => item.PaidOn);

        builder.HasIndex(item => new { item.CreditId, item.Number })
            .IsUnique()
            .HasDatabaseName("IX_CreditScheduleItems_CreditId_Number");

        builder.HasOne(item => item.Credit)
            .WithMany(credit => credit.ScheduleItems)
            .HasForeignKey(item => item.CreditId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
