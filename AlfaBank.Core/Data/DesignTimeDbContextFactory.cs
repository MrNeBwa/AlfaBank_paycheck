using AlfaBank.Core.Data;
using Microsoft.EntityFrameworkCore.Design;

namespace AlfaBank.Core.Data;

/// <summary>
/// Фабрика контекста базы данных, используемая средствами EF Core
/// (команда dotnet ef database update) вне приложения.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Создаёт контекст базы данных для генерации миграций.
    /// </summary>
    /// <param name="args">Аргументы командной строки (не используются).</param>
    /// <returns>Контекст базы данных.</returns>
    public AppDbContext CreateDbContext(string[] args) => AppDbContextFactory.Create();
}
