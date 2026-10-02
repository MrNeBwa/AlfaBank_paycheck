using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Admin;

/// <summary>
/// Управление пользователями: просмотр, регистрация сотрудников и изменение ролей.
/// </summary>
public sealed class UserManagementViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAdminService _adminService;
    private readonly UserSessionHolder _userSessionHolder;

    private UserDto? _selectedUser;
    private string _login = string.Empty;
    private string _fullName = string.Empty;
    private string _password = string.Empty;
    private bool _isActive = true;
    private RoleOption? _selectedRole;
    private bool _isEditingExistingUser;

    /// <summary>
    /// Создаёт модель представления управления пользователями.
    /// </summary>
    /// <param name="adminService">Сервис операций администратора.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    public UserManagementViewModel(IAdminService adminService, UserSessionHolder userSessionHolder)
    {
        _adminService = adminService;
        _userSessionHolder = userSessionHolder;

        Users = [];
        Roles =
        [
            new RoleOption(UserRole.CreditSpecialist, StatusTextProvider.GetRoleText(UserRole.CreditSpecialist)),
            new RoleOption(UserRole.Administrator, StatusTextProvider.GetRoleText(UserRole.Administrator))
        ];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        RegisterCommand = new AsyncRelayCommand(RegisterAsync, onError: ReportUnexpectedError);
        SaveCommand = new AsyncRelayCommand(SaveAsync, onError: ReportUnexpectedError);
        ClearCommand = new RelayCommand(ClearForm);
    }

    /// <summary>
    /// Пользователи системы.
    /// </summary>
    public ObservableCollection<UserDto> Users { get; }

    /// <summary>
    /// Роли, доступные для назначения сотруднику.
    /// </summary>
    public IReadOnlyList<RoleOption> Roles { get; }

    /// <summary>
    /// Команда обновления списка пользователей.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда регистрации нового сотрудника.
    /// </summary>
    public AsyncRelayCommand RegisterCommand { get; }

    /// <summary>
    /// Команда сохранения изменений в учётной записи.
    /// </summary>
    public AsyncRelayCommand SaveCommand { get; }

    /// <summary>
    /// Команда очистки формы.
    /// </summary>
    public RelayCommand ClearCommand { get; }

    /// <summary>
    /// Выбранный пользователь из списка.
    /// </summary>
    public UserDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value) && value is not null)
            {
                FillFormFromUser(value);
            }
        }
    }

    /// <summary>
    /// Логин нового сотрудника.
    /// </summary>
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    /// <summary>
    /// Полное имя пользователя.
    /// </summary>
    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    /// <summary>
    /// Новый пароль. Пустое значение означает, что пароль не изменяется.
    /// </summary>
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Признак активной учётной записи.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    /// <summary>
    /// Выбранная роль сотрудника.
    /// </summary>
    public RoleOption? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    /// <summary>
    /// Признак режима редактирования существующей учётной записи.
    /// </summary>
    public bool IsEditingExistingUser
    {
        get => _isEditingExistingUser;
        private set => SetProperty(ref _isEditingExistingUser, value);
    }

    /// <summary>
    /// Обновляет список пользователей.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var users = await _adminService.GetUsersAsync();

            Users.Clear();

            foreach (var user in users)
            {
                Users.Add(user);
            }

            StatusMessage = $"Загружено учётных записей: {Users.Count}.";
        });
    }

    /// <summary>
    /// Регистрирует нового сотрудника банка.
    /// </summary>
    private async Task RegisterAsync()
    {
        var registration = new StaffRegistrationDto(Login.Trim(), Password, FullName.Trim(), SelectedRole?.Role ?? UserRole.CreditSpecialist);

        var result = await ExecuteGuardedAsync(() => _adminService.RegisterStaffAsync(registration));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        StatusMessage = $"Сотрудник {FullName} зарегистрирован.";
        ClearForm();
        await ReloadAsync();
    }

    /// <summary>
    /// Сохраняет изменения в выбранной учётной записи.
    /// </summary>
    private async Task SaveAsync()
    {
        if (SelectedUser is null)
        {
            StatusMessage = "Выберите пользователя в списке.";
            return;
        }

        var editor = new UserEditorDto(
            SelectedUser.Id,
            FullName.Trim(),
            SelectedRole?.Role ?? UserRole.CreditSpecialist,
            IsActive,
            Password);

        var result = await ExecuteGuardedAsync(() =>
            _adminService.UpdateUserAsync(editor, CurrentAdministratorId));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        StatusMessage = $"Данные пользователя {SelectedUser.Login} сохранены.";
        ClearForm();
        await ReloadAsync();
    }

    /// <summary>
    /// Заполняет форму данными выбранного пользователя.
    /// </summary>
    /// <param name="user">Выбранный пользователь.</param>
    private void FillFormFromUser(UserDto user)
    {
        Login = user.Login;
        FullName = user.FullName;
        Password = string.Empty;
        IsActive = user.IsActive;
        SelectedRole = Roles.FirstOrDefault(role => role.Role == user.Role) ?? Roles[0];
        IsEditingExistingUser = true;
    }

    /// <summary>
    /// Очищает форму и переводит её в режим регистрации нового сотрудника.
    /// </summary>
    private void ClearForm()
    {
        SelectedUser = null;
        Login = string.Empty;
        FullName = string.Empty;
        Password = string.Empty;
        IsActive = true;
        SelectedRole = Roles.Count > 0 ? Roles[0] : null;
        IsEditingExistingUser = false;
        StatusMessage = string.Empty;
    }

    /// <summary>
    /// Идентификатор администратора, выполняющего изменения.
    /// </summary>
    private int CurrentAdministratorId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}