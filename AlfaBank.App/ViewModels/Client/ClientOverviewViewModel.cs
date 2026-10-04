using System.Collections.ObjectModel;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

/// <summary>
/// Главная страница клиента: сводка по кредитному портфелю и список заявок.
/// </summary>
public sealed class ClientOverviewViewModel : ViewModelBase, IPageViewModel
{
    private const string ProfileMissingMessage = "Анкета клиента не найдена. Обратитесь к администратору.";

    private readonly IClientService _clientService;
    private readonly UserSessionHolder _userSessionHolder;

    private int _applicationsTotalCount;
    private int _applicationsNewCount;
    private int _applicationsApprovedCount;
    private int _activeCreditsCount;
    private decimal _remainingDebtTotal;
    private decimal _paidTotal;

    /// <summary>
    /// Создаёт модель представления главной страницы клиента.
    /// </summary>
    /// <param name="clientService">Сервис операций клиента.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    public ClientOverviewViewModel(IClientService clientService, UserSessionHolder userSessionHolder)
    {
        _clientService = clientService;
        _userSessionHolder = userSessionHolder;

        Applications = [];
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Заявки клиента с их статусами.
    /// </summary>
    public ObservableCollection<ApplicationSummaryDto> Applications { get; }

    /// <summary>
    /// Команда обновления данных страницы.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Общее количество заявок клиента.
    /// </summary>
    public int ApplicationsTotalCount
    {
        get => _applicationsTotalCount;
        private set => SetProperty(ref _applicationsTotalCount, value);
    }

    /// <summary>
    /// Количество заявок, ожидающих рассмотрения.
    /// </summary>
    public int ApplicationsNewCount
    {
        get => _applicationsNewCount;
        private set => SetProperty(ref _applicationsNewCount, value);
    }

    /// <summary>
    /// Количество одобренных заявок.
    /// </summary>
    public int ApplicationsApprovedCount
    {
        get => _applicationsApprovedCount;
        private set => SetProperty(ref _applicationsApprovedCount, value);
    }

    /// <summary>
    /// Количество действующих кредитов.
    /// </summary>
    public int ActiveCreditsCount
    {
        get => _activeCreditsCount;
        private set => SetProperty(ref _activeCreditsCount, value);
    }

    /// <summary>
    /// Суммарный остаток задолженности клиента.
    /// </summary>
    public decimal RemainingDebtTotal
    {
        get => _remainingDebtTotal;
        private set => SetProperty(ref _remainingDebtTotal, value);
    }

    /// <summary>
    /// Суммарная сумма внесённых платежей.
    /// </summary>
    public decimal PaidTotal
    {
        get => _paidTotal;
        private set => SetProperty(ref _paidTotal, value);
    }

    /// <summary>
    /// Обновляет сводку и список заявок клиента.
    /// </summary>
    public async Task ReloadAsync()
    {
        var clientProfileId = _userSessionHolder.CurrentUser?.ClientProfileId;

        if (!clientProfileId.HasValue)
        {
            StatusMessage = ProfileMissingMessage;
            return;
        }

        await ExecuteGuardedAsync(async () =>
        {
            var applications = await _clientService.GetMyApplicationsAsync(clientProfileId.Value);
            var overview = await _clientService.GetOverviewAsync(clientProfileId.Value);

            Applications.Clear();

            foreach (var application in applications)
            {
                Applications.Add(application);
            }

            ApplyOverview(overview);
        });
    }

    /// <summary>
    /// Применяет сводные показатели к элементам интерфейса.
    /// </summary>
    /// <param name="overviewResult">Результат получения сводки.</param>
    private void ApplyOverview(Core.Results.OperationResult<ClientOverviewDto> overviewResult)
    {
        if (!overviewResult.IsSuccess || overviewResult.Value is null)
        {
            ShowError(overviewResult.ErrorMessage);
            return;
        }

        var overview = overviewResult.Value;

        ApplicationsTotalCount = overview.ApplicationsTotalCount;
        ApplicationsNewCount = overview.ApplicationsNewCount;
        ApplicationsApprovedCount = overview.ApplicationsApprovedCount;
        ActiveCreditsCount = overview.ActiveCreditsCount;
        RemainingDebtTotal = overview.RemainingDebtTotal;
        PaidTotal = overview.PaidTotal;
        StatusMessage = string.Empty;
    }

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}
