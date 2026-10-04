using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

/// <summary>
/// Страница клиента с графиком платежей по кредитам и внесёнными платежами.
/// </summary>
public sealed class ClientScheduleViewModel : ViewModelBase, IPageViewModel
{
    private const string ProfileMissingMessage = "Анкета клиента не найдена. Обратитесь к администратору.";

    private readonly IClientService _clientService;
    private readonly UserSessionHolder _userSessionHolder;

    private CreditSummaryDto? _selectedCredit;
    private ScheduleRowDto? _selectedScheduleItem;

    /// <summary>
    /// Создаёт модель представления страницы графиков платежей.
    /// </summary>
    /// <param name="clientService">Сервис операций клиента.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    public ClientScheduleViewModel(IClientService clientService, UserSessionHolder userSessionHolder)
    {
        _clientService = clientService;
        _userSessionHolder = userSessionHolder;

        Credits = [];
        ScheduleItems = [];
        Payments = [];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        ReloadScheduleCommand = new AsyncRelayCommand(ReloadScheduleAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Кредиты клиента.
    /// </summary>
    public ObservableCollection<CreditSummaryDto> Credits { get; }

    /// <summary>
    /// Платежи по выбранному кредиту.
    /// </summary>
    public ObservableCollection<ScheduleRowDto> ScheduleItems { get; }

    /// <summary>
    /// Все внесённые клиентом платежи.
    /// </summary>
    public ObservableCollection<PaymentDto> Payments { get; }

    /// <summary>
    /// Команда обновления данных страницы.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда обновления графика по выбранному кредиту.
    /// </summary>
    public AsyncRelayCommand ReloadScheduleCommand { get; }

    /// <summary>
    /// Выбранный кредит.
    /// </summary>
    public CreditSummaryDto? SelectedCredit
    {
        get => _selectedCredit;
        set
        {
            if (SetProperty(ref _selectedCredit, value))
            {
                ScheduleItems.Clear();
                ReloadScheduleCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Выбранный платёж графика.
    /// </summary>
    public ScheduleRowDto? SelectedScheduleItem
    {
        get => _selectedScheduleItem;
        set => SetProperty(ref _selectedScheduleItem, value);
    }

    /// <summary>
    /// Количество оплаченных платежей по выбранному кредиту.
    /// </summary>
    public int PaidCount =>
        ScheduleItems.Count(item => item.Status == ScheduleItemStatus.Paid);

    /// <summary>
    /// Обновляет список кредитов и платежей клиента.
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
            var credits = await _clientService.GetMyCreditsAsync(clientProfileId.Value);
            var payments = await _clientService.GetMyPaymentsAsync(clientProfileId.Value);

            Credits.Clear();

            foreach (var credit in credits)
            {
                Credits.Add(credit);
            }

            Payments.Clear();

            foreach (var payment in payments)
            {
                Payments.Add(payment);
            }

            SelectedCredit ??= Credits.FirstOrDefault();
        });

        if (SelectedCredit is not null)
        {
            await ReloadScheduleAsync();
        }
    }

    /// <summary>
    /// Обновляет график платежей по выбранному кредиту.
    /// </summary>
    private async Task ReloadScheduleAsync()
    {
        var clientProfileId = _userSessionHolder.CurrentUser?.ClientProfileId;

        if (!clientProfileId.HasValue || SelectedCredit is null)
        {
            return;
        }

        await ExecuteGuardedAsync(async () =>
        {
            var result = await _clientService.GetMyScheduleAsync(clientProfileId.Value, SelectedCredit.Id);

            ScheduleItems.Clear();

            if (!result.IsSuccess || result.Value is null)
            {
                ShowError(result.ErrorMessage);
                return;
            }

            foreach (var item in result.Value)
            {
                ScheduleItems.Add(item);
            }

            OnPropertyChanged(nameof(PaidCount));
            ReloadScheduleCommand.RaiseCanExecuteChanged();
        });
    }

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}
