using AlfaBank.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

/// <summary>
/// Контекст базы данных системы автоматизации кредитных операций.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Создаёт контекст базы данных с указанными параметрами.
    /// </summary>
    /// <param name="options">Параметры подключения к базе данных.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Учётные записи пользователей.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Анкеты клиентов.
    /// </summary>
    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();

    /// <summary>
    /// Справочник кредитных продуктов.
    /// </summary>
    public DbSet<CreditProduct> CreditProducts => Set<CreditProduct>();

    /// <summary>
    /// Заявки клиентов на кредит.
    /// </summary>
    public DbSet<CreditApplication> CreditApplications => Set<CreditApplication>();

    /// <summary>
    /// Выданные кредиты.
    /// </summary>
    public DbSet<Credit> Credits => Set<Credit>();

    /// <summary>
    /// Графики платежей по кредитам.
    /// </summary>
    public DbSet<CreditScheduleItem> CreditScheduleItems => Set<CreditScheduleItem>();

    /// <summary>
    /// Зарегистрированные платежи по кредитам.
    /// </summary>
    public DbSet<Payment> Payments => Set<Payment>();

    /// <summary>
    /// История изменения статусов заявок.
    /// </summary>
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistory => Set<ApplicationStatusHistory>();

    /// <summary>
    /// Применяет конфигурации сущностей к модели данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Configure(modelBuilder);
    }
}
