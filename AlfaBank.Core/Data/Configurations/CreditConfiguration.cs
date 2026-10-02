using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы выданных кредитов.
/// </summary>
public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Credit> builder)
    {
        builder.ToTable("Credits");

        builder.HasKey(credit => credit.Id);

        builder.Property(credit => credit.IssuedOn)
            .IsRequired();

        builder.Property(credit => credit.Amount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(credit => credit.AnnualInterestRate)
            .IsRequired()
            .HasPrecision(ColumnLimits.RatePrecision, ColumnLimits.MoneyScale);

        builder.Property(credit => credit.TermMonths)
            .IsRequired();

        builder.Property(credit => credit.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(credit => credit.ClosedOn);

        builder.HasIndex(credit => credit.CreditApplicationId)
            .IsUnique()
            .HasDatabaseName("IX_Credits_CreditApplicationId");

        builder.HasOne(credit => credit.CreditApplication)
            .WithOne(application => application.Credit)
            .HasForeignKey<Credit>(credit => credit.CreditApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
