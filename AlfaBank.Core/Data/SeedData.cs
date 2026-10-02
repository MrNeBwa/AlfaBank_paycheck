using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

/// <summary>
/// Демонстрационные данные проекта: учётные записи, анкеты клиентов и кредитные продукты.
/// Данные фиксируются в модели средствами Code First (HasData) и попадают в базу при миграции.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Логин кредитного специалиста.
    /// </summary>
    public const string SpecialistLogin = "specialist";

    /// <summary>
    /// Пароль кредитного специалиста.
    /// </summary>
    public const string SpecialistPassword = "Specialist2024";

    /// <summary>
    /// Логин администратора.
    /// </summary>
    public const string AdministratorLogin = "administrator";

    /// <summary>
    /// Пароль администратора.
    /// </summary>
    public const string AdministratorPassword = "Administrator2024";

    /// <summary>
    /// Логин первого демонстрационного клиента.
    /// </summary>
    public const string FirstClientLogin = "client";

    /// <summary>
    /// Логин второго демонстрационного клиента.
    /// </summary>
    public const string SecondClientLogin = "client2";

    /// <summary>
    /// Пароль демонстрационных клиентов.
    /// </summary>
    public const string ClientPassword = "Client2024";

    /// <summary>
    /// Идентификатор учётной записи кредитного специалиста.
    /// </summary>
    public const int SpecialistUserId = 1;

    /// <summary>
    /// Идентификатор учётной записи администратора.
    /// </summary>
    public const int AdministratorUserId = 2;

    /// <summary>
    /// Идентификатор учётной записи первого клиента.
    /// </summary>
    public const int FirstClientUserId = 3;

    /// <summary>
    /// Идентификатор учётной записи второго клиента.
    /// </summary>
    public const int SecondClientUserId = 4;

    /// <summary>
    /// Идентификатор анкеты первого клиента.
    /// </summary>
    public const int FirstClientProfileId = 1;

    /// <summary>
    /// Идентификатор анкеты второго клиента.
    /// </summary>
    public const int SecondClientProfileId = 2;

    /// <summary>
    /// Фиксированная дата регистрации демонстрационных записей.
    /// </summary>
    private static readonly DateTime SeedTimestamp = new(2024, 1, 15, 9, 0, 0, DateTimeKind.Unspecified);

    private static readonly Pbkdf2PasswordHasher PasswordHasher = new();

    private static readonly string SpecialistPasswordHash =
        PasswordHasher.Hash(SpecialistPassword, "66LaxU3dH0LzWUowf+15zQ==");

    private static readonly string AdministratorPasswordHash =
        PasswordHasher.Hash(AdministratorPassword, "LRfRnYHrBGfkj+TgEUqiwA==");

    private static readonly string FirstClientPasswordHash =
        PasswordHasher.Hash(ClientPassword, "Y6SL/xSlIgIdQAWKWoIrMQ==");

    private static readonly string SecondClientPasswordHash =
        PasswordHasher.Hash(ClientPassword, "YYHFp2WHnQA7lebRkA7u7w==");

    /// <summary>
    /// Добавляет демонстрационные данные в модель данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели данных.</param>
    public static void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<CreditProduct>().HasData(CreateCreditProducts());
        modelBuilder.Entity<User>().HasData(CreateUsers());
        modelBuilder.Entity<ClientProfile>().HasData(CreateClientProfiles());
    }

    /// <summary>
    /// Создаёт три демонстрационных кредитных продукта.
    /// </summary>
    /// <returns>Коллекция кредитных продуктов.</returns>
    private static CreditProduct[] CreateCreditProducts() =>
    [
        new CreditProduct
        {
            Id = 1,
            Name = "Потребительский кредит",
            AnnualInterestRate = 18.5m,
            MinAmount = 50_000m,
            MaxAmount = 1_500_000m,
            MinTermMonths = 6,
            MaxTermMonths = 60,
            Description = "Кредит наличными на любые нужды без залога и поручителей.",
            IsActive = true,
            CreatedAt = SeedTimestamp
        },
        new CreditProduct
        {
            Id = 2,
            Name = "Рефинансирование",
            AnnualInterestRate = 16.0m,
            MinAmount = 100_000m,
            MaxAmount = 3_000_000m,
            MinTermMonths = 12,
            MaxTermMonths = 84,
            Description = "Объединение нескольких кредитов в один по сниженной ставке.",
            IsActive = true,
            CreatedAt = SeedTimestamp
        },
        new CreditProduct
        {
            Id = 3,
            Name = "Кредит для предпринимателей",
            AnnualInterestRate = 22.0m,
            MinAmount = 200_000m,
            MaxAmount = 5_000_000m,
            MinTermMonths = 6,
            MaxTermMonths = 36,
            Description = "Оборотные средства для малого и среднего бизнеса.",
            IsActive = true,
            CreatedAt = SeedTimestamp
        }
    ];

    /// <summary>
    /// Создаёт учётные записи двух клиентов, кредитного специалиста и администратора.
    /// </summary>
    /// <returns>Коллекция пользователей.</returns>
    private static User[] CreateUsers() =>
    [
        new User
        {
            Id = SpecialistUserId,
            Login = SpecialistLogin,
            PasswordHash = SpecialistPasswordHash,
            FullName = "Соколова Ирина Петровна",
            Role = UserRole.CreditSpecialist,
            IsActive = true,
            CreatedAt = SeedTimestamp
        },
        new User
        {
            Id = AdministratorUserId,
            Login = AdministratorLogin,
            PasswordHash = AdministratorPasswordHash,
            FullName = "Ковалёв Артём Игоревич",
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedAt = SeedTimestamp
        },
        new User
        {
            Id = FirstClientUserId,
            Login = FirstClientLogin,
            PasswordHash = FirstClientPasswordHash,
            FullName = "Иванов Иван Иванович",
            Role = UserRole.Client,
            IsActive = true,
            CreatedAt = SeedTimestamp
        },
        new User
        {
            Id = SecondClientUserId,
            Login = SecondClientLogin,
            PasswordHash = SecondClientPasswordHash,
            FullName = "Петрова Анна Сергеевна",
            Role = UserRole.Client,
            IsActive = true,
            CreatedAt = SeedTimestamp
        }
    ];

    /// <summary>
    /// Создаёт анкеты двух демонстрационных клиентов.
    /// </summary>
    /// <returns>Коллекция анкет клиентов.</returns>
    private static ClientProfile[] CreateClientProfiles() =>
    [
        new ClientProfile
        {
            Id = FirstClientProfileId,
            UserId = FirstClientUserId,
            PassportNumber = "45 12 345678",
            BirthDate = new DateOnly(1990, 4, 15),
            RegistrationAddress = "г. Москва, ул. Тверская, д. 1, кв. 5",
            EmployerName = "ООО «Ромашка»",
            EmploymentMonths = 84,
            MonthlyIncome = 120_000m,
            MonthlyExpenses = 45_000m
        },
        new ClientProfile
        {
            Id = SecondClientProfileId,
            UserId = SecondClientUserId,
            PassportNumber = "52 98 765432",
            BirthDate = new DateOnly(1996, 11, 2),
            RegistrationAddress = "г. Санкт-Петербург, пр-т Невский, д. 20, кв. 44",
            EmployerName = "АО «Вектор»",
            EmploymentMonths = 30,
            MonthlyIncome = 85_000m,
            MonthlyExpenses = 30_000m
        }
    ];
}
