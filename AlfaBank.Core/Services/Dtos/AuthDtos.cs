using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

/// <summary>
/// Данные для входа в систему.
/// </summary>
/// <param name="Login">Логин пользователя.</param>
/// <param name="Password">Пароль пользователя.</param>
public sealed record LoginRequest(string Login, string Password);

/// <summary>
/// Данные самостоятельной регистрации клиента.
/// </summary>
/// <param name="Login">Придумываемый логин.</param>
/// <param name="Password">Придумываемый пароль.</param>
/// <param name="FullName">Полное имя клиента.</param>
/// <param name="PassportNumber">Номер и серия паспорта.</param>
/// <param name="BirthDate">Дата рождения.</param>
/// <param name="RegistrationAddress">Адрес регистрации.</param>
/// <param name="EmployerName">Место работы.</param>
/// <param name="EmploymentMonths">Стаж работы в месяцах.</param>
/// <param name="MonthlyIncome">Ежемесячный доход в рублях.</param>
/// <param name="MonthlyExpenses">Ежемесячные расходы в рублях.</param>
public sealed record RegistrationRequest(
    string Login,
    string Password,
    string FullName,
    string PassportNumber,
    DateOnly BirthDate,
    string RegistrationAddress,
    string EmployerName,
    int EmploymentMonths,
    decimal MonthlyIncome,
    decimal MonthlyExpenses);

/// <summary>
/// Данные текущего пользователя, полученные после успешного входа в систему.
/// </summary>
/// <param name="UserId">Идентификатор пользователя.</param>
/// <param name="Login">Логин пользователя.</param>
/// <param name="FullName">Полное имя пользователя.</param>
/// <param name="Role">Роль пользователя.</param>
/// <param name="RoleText">Название роли на русском языке.</param>
/// <param name="ClientProfileId">Идентификатор анкеты клиента. Не заполняется у сотрудников банка.</param>
public sealed record UserSessionDto(
    int UserId,
    string Login,
    string FullName,
    UserRole Role,
    string RoleText,
    int? ClientProfileId);

/// <summary>
/// Данные учётной записи для списка пользователей в режиме администратора.
/// </summary>
/// <param name="Id">Идентификатор пользователя.</param>
/// <param name="Login">Логин пользователя.</param>
/// <param name="FullName">Полное имя пользователя.</param>
/// <param name="Role">Роль пользователя.</param>
/// <param name="RoleText">Название роли на русском языке.</param>
/// <param name="IsActive">Признак активной учётной записи.</param>
/// <param name="HasClientProfile">Признак наличия анкеты клиента.</param>
/// <param name="CreatedAt">Дата регистрации учётной записи.</param>
public sealed record UserDto(
    int Id,
    string Login,
    string FullName,
    UserRole Role,
    string RoleText,
    bool IsActive,
    bool HasClientProfile,
    DateTime CreatedAt);

/// <summary>
/// Данные для регистрации нового сотрудника банка.
/// </summary>
/// <param name="Login">Логин сотрудника.</param>
/// <param name="Password">Пароль сотрудника.</param>
/// <param name="FullName">Полное имя сотрудника.</param>
/// <param name="Role">Роль сотрудника: кредитный специалист или администратор.</param>
public sealed record StaffRegistrationDto(string Login, string Password, string FullName, UserRole Role);

/// <summary>
/// Данные для изменения учётной записи пользователя.
/// </summary>
/// <param name="UserId">Идентификатор пользователя.</param>
/// <param name="FullName">Новое полное имя.</param>
/// <param name="Role">Новая роль пользователя.</param>
/// <param name="IsActive">Новый признак активной учётной записи.</param>
/// <param name="NewPassword">Новый пароль. Пустое значение означает, что пароль не меняется.</param>
public sealed record UserEditorDto(int UserId, string FullName, UserRole Role, bool IsActive, string NewPassword);
