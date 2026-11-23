using AlfaBank.Core.Models.Enums;

namespace AlfaBank.Core.Services.Dtos;

public sealed record LoginRequest(string Login, string Password);

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

public sealed record UserSessionDto(
    int UserId,
    string Login,
    string FullName,
    UserRole Role,
    string RoleText,
    int? ClientProfileId);

public sealed record UserDto(
    int Id,
    string Login,
    string FullName,
    UserRole Role,
    string RoleText,
    bool IsActive,
    bool HasClientProfile,
    DateTime CreatedAt,
    ClientProfileInputDto? Profile);

public sealed record ClientProfileInputDto(
    string PassportNumber,
    DateOnly BirthDate,
    string RegistrationAddress,
    string EmployerName,
    int EmploymentMonths,
    decimal MonthlyIncome,
    decimal MonthlyExpenses);

public sealed record StaffRegistrationDto(
    string Login,
    string Password,
    string FullName,
    UserRole Role,
    ClientProfileInputDto? Profile);

public sealed record UserEditorDto(
    int UserId,
    string Login,
    string FullName,
    UserRole Role,
    bool IsActive,
    string NewPassword,
    ClientProfileInputDto? Profile);
