using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaBank.Core.Data.Configurations;

/// <summary>
/// Конфигурация таблицы учётных записей пользователей.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Login)
            .IsRequired()
            .HasMaxLength(ColumnLimits.Login);

        builder.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(ColumnLimits.PasswordHash);

        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(ColumnLimits.FullName);

        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(user => user.IsActive)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.HasIndex(user => user.Login)
            .IsUnique()
            .HasDatabaseName("IX_Users_Login");

        builder.HasOne(user => user.ClientProfile)
            .WithOne(profile => profile.User)
            .HasForeignKey<ClientProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
