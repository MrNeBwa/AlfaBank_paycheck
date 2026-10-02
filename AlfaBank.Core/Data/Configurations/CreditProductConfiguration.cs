using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы справочника кредитных продуктов.
/// </summary>
public class CreditProductConfiguration : IEntityTypeConfiguration<CreditProduct>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CreditProduct> builder)
    {
        builder.ToTable("CreditProducts");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(ColumnLimits.ProductName);

        builder.Property(product => product.AnnualInterestRate)
            .IsRequired()
            .HasPrecision(ColumnLimits.RatePrecision, ColumnLimits.MoneyScale);

        builder.Property(product => product.MinAmount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(product => product.MaxAmount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(product => product.MinTermMonths)
            .IsRequired();

        builder.Property(product => product.MaxTermMonths)
            .IsRequired();

        builder.Property(product => product.Description)
            .IsRequired()
            .HasMaxLength(ColumnLimits.ShortDescription);

        builder.Property(product => product.IsActive)
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .IsRequired();
    }
}
