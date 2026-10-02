using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы кредитных заявок.
/// </summary>
public class CreditApplicationConfiguration : IEntityTypeConfiguration<CreditApplication>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<CreditApplication> builder)
    {
        builder.ToTable("CreditApplications");

        builder.HasKey(application => application.Id);

        builder.Property(application => application.Amount)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(application => application.TermMonths)
            .IsRequired();

        builder.Property(application => application.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(application => application.CreatedAt)
            .IsRequired();

        builder.Property(application => application.ReviewedAt);

        builder.Property(application => application.DecisionComment)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Comment);

        builder.Property(application => application.ScorePoints);

        builder.Property(application => application.PaymentSharePercent)
            .HasPrecision(ColumnLimits.RatePrecision, ColumnLimits.MoneyScale);

        builder.HasIndex(application => new { application.Status, application.CreatedAt })
            .HasDatabaseName("IX_CreditApplications_Status_CreatedAt");

        builder.HasOne(application => application.ClientProfile)
            .WithMany(profile => profile.Applications)
            .HasForeignKey(application => application.ClientProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(application => application.CreditProduct)
            .WithMany(product => product.Applications)
            .HasForeignKey(application => application.CreditProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(application => application.ReviewedBy)
            .WithMany()
            .HasForeignKey(application => application.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
