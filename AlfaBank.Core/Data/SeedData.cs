using AlfaBank.Core.Models;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace AlfaBank.Core.Data;

public static class SeedData
{
    public const string SpecialistLogin = "specialist";

    public const string SpecialistPassword = "Specialist2024";

    public const string AdministratorLogin = "administrator";

    public const string AdministratorPassword = "Administrator2024";

    public const string FirstClientLogin = "client";

    public const string SecondClientLogin = "client2";

    public const string ClientPassword = "Client2024";

    public const int SpecialistUserId = 1;

    public const int AdministratorUserId = 2;

    public const int FirstClientUserId = 3;

    public const int SecondClientUserId = 4;

    public const int FirstClientProfileId = 1;

    public const int SecondClientProfileId = 2;

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

    public static void Configure(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<CreditProduct>().HasData(CreateCreditProducts());
        modelBuilder.Entity<User>().HasData(CreateUsers());
        modelBuilder.Entity<ClientProfile>().HasData(CreateClientProfiles());
    }

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
