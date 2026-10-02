using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы истории изменения статусов заявок.
/// </summary>
public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> builder)
    {
        builder.ToTable("ApplicationStatusHistory");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.FromStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(record => record.ToStatus)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(record => record.ChangedAt)
            .IsRequired();

        builder.Property(record => record.Comment)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Comment);

        builder.HasOne(record => record.CreditApplication)
            .WithMany(application => application.StatusHistory)
            .HasForeignKey(record => record.CreditApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(record => record.ChangedBy)
            .WithMany()
            .HasForeignKey(record => record.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
