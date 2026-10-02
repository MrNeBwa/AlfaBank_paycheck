using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы платежей по кредитам.
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(payment => payment.Id);

        builder.Property(payment => payment.PaidOn)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(payment => payment.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(payment => payment.Comment)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Comment);

        builder.Property(payment => payment.CreatedAt)
            .IsRequired();

        builder.HasIndex(payment => payment.CreditScheduleItemId)
            .HasDatabaseName("IX_Payments_CreditScheduleItemId");

        builder.HasOne(payment => payment.Credit)
            .WithMany(credit => credit.Payments)
            .HasForeignKey(payment => payment.CreditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(payment => payment.CreditScheduleItem)
            .WithOne(item => item.Payment)
            .HasForeignKey<Payment>(payment => payment.CreditScheduleItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(payment => payment.RegisteredBy)
            .WithMany()
            .HasForeignKey(payment => payment.RegisteredByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
