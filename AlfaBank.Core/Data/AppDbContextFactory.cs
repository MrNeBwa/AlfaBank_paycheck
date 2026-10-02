using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

/// <summary>
/// Создаёт настроенный контекст базы данных для приложения и для средств миграции.
/// </summary>
public static class AppDbContextFactory
{
    /// <summary>
    /// Формирует параметры подключения к базе данных проекта.
    /// </summary>
    /// <returns>Параметры контекста базы данных.</returns>
    public static DbContextOptions<AppDbContext> CreateOptions()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(ConnectionStringProvider.GetConnectionString());

        return optionsBuilder.Options;
    }

    /// <summary>
    /// Создаёт новый экземпляр контекста базы данных.
    /// </summary>
    /// <returns>Контекст базы данных.</returns>
    public static AppDbContext Create() => new(CreateOptions());
}
