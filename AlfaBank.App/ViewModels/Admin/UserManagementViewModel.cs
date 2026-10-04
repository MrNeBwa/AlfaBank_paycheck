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
    /// <summary>
    /// Дата рождения по умолчанию: возраст 30 лет, то есть заведомо допустимый.
    /// </summary>
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

        foreach (var role in (UserRole[])Enum.GetValues(typeof(UserRole)))
        {
            Roles.Add(new RoleOption(role, StatusTextProvider.GetRoleText(role)));
        }

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
    /// Список строится по всем значениям перечисления и дополняется на лету,
    /// поэтому роль существующего пользователя всегда есть в списке и не может
    /// молча подмениться другой при сохранении.
    /// </summary>
    public ObservableCollection<RoleOption> Roles => _roles;

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
        set
        {
            if (SetProperty(ref _selectedRole, value))
            {
                // Поля анкеты показываются только для роли «Клиент»:
                // у сотрудников банка анкеты нет, и она им не нужна.
                OnPropertyChanged(nameof(IsClientRoleSelected));
            }
        }
    }

    /// <summary>
    /// Признак того, что выбрана роль клиента и требуется заполнить анкету.
    /// </summary>
    public bool IsClientRoleSelected => SelectedRole?.Role == UserRole.Client;

    /// <summary>
    /// Номер и серия паспорта клиента.
    /// </summary>
    public string PassportNumber
    {
        get => _passportNumber;
        set => SetProperty(ref _passportNumber, value);
    }

    /// <summary>
    /// Дата рождения клиента.
    /// </summary>
    public DateTime BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    /// <summary>
    /// Адрес регистрации клиента.
    /// </summary>
    public string RegistrationAddress
    {
        get => _registrationAddress;
        set => SetProperty(ref _registrationAddress, value);
    }

    /// <summary>
    /// Место работы клиента.
    /// </summary>
    public string EmployerName
    {
        get => _employerName;
        set => SetProperty(ref _employerName, value);
    }

    /// <summary>
    /// Стаж работы клиента на текущем месте в месяцах.
    /// </summary>
    public int EmploymentMonths
    {
        get => _employmentMonths;
        set => SetProperty(ref _employmentMonths, value);
    }

    /// <summary>
    /// Ежемесячный доход клиента в рублях.
    /// </summary>
    public decimal MonthlyIncome
    {
        get => _monthlyIncome;
        set => SetProperty(ref _monthlyIncome, value);
    }

    /// <summary>
    /// Ежемесячные расходы клиента в рублях.
    /// </summary>
    public decimal MonthlyExpenses
    {
        get => _monthlyExpenses;
        set => SetProperty(ref _monthlyExpenses, value);
    }

    /// <summary>
    /// Признак режима редактирования существующей учётной записи.
    /// </summary>
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

    /// <summary>
    /// Заголовок формы: показывает, создаётся учётная запись или редактируется существующая.
    /// </summary>
    public string FormModeText => IsEditingExistingUser && SelectedUser is not null
        ? $"Редактирование учётной записи «{SelectedUser.Login}»"
        : "Новая учётная запись сотрудника";

    /// <summary>
    /// Обновляет список пользователей.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var users = await _adminService.GetUsersAsync();

            // Режим формы запоминается до перезагрузки: таблица при заполнении
            // может выделить строку по собственному усмотрению, и тогда форма
            // перешла бы в режим правки без всякого выбора пользователя.
            var wasEditing = IsEditingExistingUser;
            var editingId = SelectedUser?.Id;

            Users.Clear();

            foreach (var user in users)
            {
                Users.Add(user);
            }

            // После перезагрузки все элементы коллекции новые, поэтому в режиме
            // правки выделенная строка и форма соответствуют прежнему сотруднику.
            SelectedUser = wasEditing && editingId is int id
                ? Users.FirstOrDefault(user => user.Id == id)
                : null;

            StatusMessage = $"Загружено учётных записей: {Users.Count}.";
        });
    }

    /// <summary>
    /// Регистрирует нового пользователя банка.
    /// </summary>
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

        // Форма очищается до вывода результата, иначе очистка стёрла бы сообщение.
        ClearForm();
        await ReloadAsync();

        ShowSuccess($"Пользователь {userName} зарегистрирован.");
    }

    /// <summary>
    /// Сохраняет изменения в выбранной учётной записи.
    /// </summary>
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

        // Форма очищается до вывода результата, иначе очистка стёрла бы сообщение.
        ClearForm();
        await ReloadAsync();

        ShowSuccess($"Данные пользователя {userLogin} сохранены.");
    }

    /// <summary>
    /// Собирает анкету клиента из полей формы.
    /// </summary>
    /// <returns>Данные анкеты клиента.</returns>
    private ClientProfileInputDto BuildProfile() => new(
        PassportNumber.Trim(),
        DateOnly.FromDateTime(BirthDate),
        RegistrationAddress.Trim(),
        EmployerName.Trim(),
        EmploymentMonths,
        MonthlyIncome,
        MonthlyExpenses);

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
        SelectedRole = EnsureRoleIsListed(user.Role);
        FillProfile(user.Profile);
        IsEditingExistingUser = true;
    }

    /// <summary>
    /// Возвращает роль из списка, при необходимости добавляя её в список.
    /// Раньше здесь применялась подстановка первого элемента списка, и роль
    /// пользователя менялась без предупреждения: клиент сохранялся как
    /// кредитный специалист, пока варианта «Клиент» в списке не было.
    /// </summary>
    /// <param name="role">Роль пользователя.</param>
    /// <returns>Элемент списка ролей с этой ролью.</returns>
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

    /// <summary>
    /// Заполняет поля анкеты клиента.
    /// </summary>
    /// <param name="profile">Анкета выбранного пользователя либо null для сотрудников банка.</param>
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

    /// <summary>
    /// Очищает форму и переводит её в режим регистрации новой учётной записи.
    /// </summary>
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

    /// <summary>
    /// Идентификатор администратора, выполняющего изменения.
    /// </summary>
    private int CurrentAdministratorId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    /// <summary>
    /// Открывает страницу управления сотрудниками.
    /// Модель представления переиспользуется всю сессию, поэтому при каждом открытии
    /// страницы форма возвращается в режим регистрации: иначе она осталась бы
    /// в режиме правки выбранного сотрудника, и поле логин было бы заблокировано.
    /// </summary>
    public async Task LoadAsync()
    {
        ClearForm();
        await ReloadAsync();
    }
}