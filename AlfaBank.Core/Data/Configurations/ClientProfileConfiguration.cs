using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы анкет клиентов.
/// </summary>
public class ClientProfileConfiguration : IEntityTypeConfiguration<ClientProfile>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ClientProfile> builder)
    {
        builder.ToTable("ClientProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.PassportNumber)
            .IsRequired()
            .HasMaxLength(ColumnLimits.PassportNumber);

        builder.Property(profile => profile.BirthDate)
            .IsRequired();

        builder.Property(profile => profile.RegistrationAddress)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Address);

        builder.Property(profile => profile.EmployerName)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Address);

        builder.Property(profile => profile.EmploymentMonths)
            .IsRequired();

        builder.Property(profile => profile.MonthlyIncome)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.Property(profile => profile.MonthlyExpenses)
            .IsRequired()
            .HasPrecision(ColumnLimits.MoneyPrecision, ColumnLimits.MoneyScale);

        builder.HasIndex(profile => profile.UserId)
            .IsUnique()
            .HasDatabaseName("IX_ClientProfiles_UserId");
    }
}
