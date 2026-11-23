using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Admin;

public sealed class UserManagementViewModel : ViewModelBase, IPageViewModel
{
    private static readonly DateTime DefaultBirthDate = DateTime.Today.AddYears(-30);

    private readonly IAdminService _adminService;
    private readonly UserSessionHolder _userSessionHolder;
    private readonly ObservableCollection<RoleOption> _roles = [];

    private UserDto? _selectedUser;
    private string _login = string.Empty;
    private string _fullName = string.Empty;
    private string _password = string.Empty;
    private bool _isActive = true;
    private RoleOption? _selectedRole;
    private bool _isEditingExistingUser;
    private string _passportNumber = string.Empty;
    private DateTime _birthDate = DefaultBirthDate;
    private string _registrationAddress = string.Empty;
    private string _employerName = string.Empty;
    private int _employmentMonths;
    private decimal _monthlyIncome;
    private decimal _monthlyExpenses;

    public UserManagementViewModel(IAdminService adminService, UserSessionHolder userSessionHolder)
    {
        _adminService = adminService;
        _userSessionHolder = userSessionHolder;

        Users = [];

        foreach (var role in (UserRole[])Enum.GetValues(typeof(UserRole)))
        {
            Roles.Add(new RoleOption(role, StatusTextProvider.GetRoleText(role)));
        }

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        RegisterCommand = new AsyncRelayCommand(RegisterAsync, onError: ReportUnexpectedError);
        SaveCommand = new AsyncRelayCommand(SaveAsync, onError: ReportUnexpectedError);
        ClearCommand = new RelayCommand(ClearForm);
    }

    public ObservableCollection<UserDto> Users { get; }

    public ObservableCollection<RoleOption> Roles => _roles;

    public AsyncRelayCommand ReloadCommand { get; }

    public AsyncRelayCommand RegisterCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public RelayCommand ClearCommand { get; }

    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                if (value is not null)
                {
                    FillFormFromUser(value);
                }

                OnPropertyChanged(nameof(FormModeText));
            }
        }
    }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public RoleOption? SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                OnPropertyChanged(nameof(IsClientRoleSelected));
            }
        }
    }

    public bool IsClientRoleSelected => SelectedRole?.Role == UserRole.Client;

    public string PassportNumber
    {
        get => _passportNumber;
        set => SetProperty(ref _passportNumber, value);
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    public string RegistrationAddress
    {
        get => _registrationAddress;
        set => SetProperty(ref _registrationAddress, value);
    }

    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    public int EmploymentMonths
    {
        get => _employmentMonths;
        set => SetProperty(ref _employmentMonths, value);
    }

    public decimal MonthlyIncome
    {
        get => _monthlyIncome;
        set => SetProperty(ref _monthlyIncome, value);
    }

    public decimal MonthlyExpenses
    {
        get => _monthlyExpenses;
        set => SetProperty(ref _monthlyExpenses, value);
    }

    public bool IsEditingExistingUser
    {
        get => _isEditingExistingUser;
        private set
        {
            if (SetProperty(ref _isEditingExistingUser, value))
            {
                OnPropertyChanged(nameof(FormModeText));
            }
        }
    }

    public string FormModeText => IsEditingExistingUser && SelectedUser is not null
        ? $"Редактирование учётной записи «{SelectedUser.Login}»"
        : "Новая учётная запись сотрудника";

    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var users = await _adminService.GetUsersAsync();

            var wasEditing = IsEditingExistingUser;
            var editingId = SelectedUser?.Id;

            Users.Clear();

            foreach (var user in users)
            {
                Users.Add(user);
            }

            SelectedUser = wasEditing && editingId is int id
                ? Users.FirstOrDefault(user => user.Id == id)
                : null;

            StatusMessage = $"Загружено учётных записей: {Users.Count}.";
        });
    }

    private async Task RegisterAsync()
    {
        var userName = FullName.Trim();

        if (SelectedRole is null)
        {
            ShowError("Выберите роль пользователя.");
            return;
        }

        var role = SelectedRole.Role;

        var registration = new StaffRegistrationDto(
            Login.Trim(),
            Password,
            userName,
            role,
            role == UserRole.Client ? BuildProfile() : null);

        var result = await ExecuteGuardedAsync(() => _adminService.RegisterStaffAsync(registration));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        ClearForm();
        await ReloadAsync();

        ShowSuccess($"Пользователь {userName} зарегистрирован.");
    }

    private async Task SaveAsync()
    {
        if (SelectedUser is null)
        {
            ShowError("Выберите пользователя в списке.");
            return;
        }

        var userLogin = SelectedUser.Login;

        if (SelectedRole is null)
        {
            ShowError("Выберите роль пользователя.");
            return;
        }

        var role = SelectedRole.Role;

        var editor = new UserEditorDto(
            SelectedUser.Id,
            Login.Trim(),
            FullName.Trim(),
            role,
            IsActive,
            Password,
            role == UserRole.Client ? BuildProfile() : null);

        var result = await ExecuteGuardedAsync(() =>
            _adminService.UpdateUserAsync(editor, CurrentAdministratorId));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        ClearForm();
        await ReloadAsync();

        ShowSuccess($"Данные пользователя {userLogin} сохранены.");
    }

    private ClientProfileInputDto BuildProfile() => new(
        PassportNumber.Trim(),
        DateOnly.FromDateTime(BirthDate),
        RegistrationAddress.Trim(),
        EmployerName.Trim(),
        EmploymentMonths,
        MonthlyIncome,
        MonthlyExpenses);

    private void FillFormFromUser(UserDto user)
    {
        Login = user.Login;
        FullName = user.FullName;
        Password = string.Empty;
        IsActive = user.IsActive;
        SelectedRole = EnsureRoleIsListed(user.Role);
        FillProfile(user.Profile);
        IsEditingExistingUser = true;
    }

    private RoleOption EnsureRoleIsListed(UserRole role)
    {
        var existing = Roles.FirstOrDefault(option => option.Role == role);

        if (existing is not null)
        {
            return existing;
        }

        var added = new RoleOption(role, StatusTextProvider.GetRoleText(role));
        Roles.Add(added);

        return added;
    }

    private void FillProfile(ClientProfileInputDto? profile)
    {
        PassportNumber = profile?.PassportNumber ?? string.Empty;
        BirthDate = profile?.BirthDate.ToDateTime(TimeOnly.MinValue) ?? DefaultBirthDate;
        RegistrationAddress = profile?.RegistrationAddress ?? string.Empty;
        EmployerName = profile?.EmployerName ?? string.Empty;
        EmploymentMonths = profile?.EmploymentMonths ?? 0;
        MonthlyIncome = profile?.MonthlyIncome ?? 0m;
        MonthlyExpenses = profile?.MonthlyExpenses ?? 0m;
    }

    private void ClearForm()
    {
        SelectedUser = null;
        Login = string.Empty;
        FullName = string.Empty;
        Password = string.Empty;
        IsActive = true;
        SelectedRole = EnsureRoleIsListed(UserRole.CreditSpecialist);
        FillProfile(null);
        IsEditingExistingUser = false;
        StatusMessage = string.Empty;
    }

    private int CurrentAdministratorId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    public async Task LoadAsync()
    {
        ClearForm();
        await ReloadAsync();
    }
}