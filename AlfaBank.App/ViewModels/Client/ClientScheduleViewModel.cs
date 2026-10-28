using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

public sealed class ClientScheduleViewModel : ViewModelBase, IPageViewModel
{
    private const string ProfileMissingMessage = "Анкета клиента не найдена. Обратитесь к администратору.";

    private readonly IClientService _clientService;
    private readonly UserSessionHolder _userSessionHolder;

    private CreditSummaryDto? _selectedCredit;
    private ScheduleRowDto? _selectedScheduleItem;

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

    public ObservableCollection<CreditSummaryDto> Credits { get; }

    public ObservableCollection<ScheduleRowDto> ScheduleItems { get; }

    public ObservableCollection<PaymentDto> Payments { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public AsyncRelayCommand ReloadScheduleCommand { get; }

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

    public ScheduleRowDto? SelectedScheduleItem
    {
        get => _selectedScheduleItem;
        set => SetProperty(ref _selectedScheduleItem, value);
    }

    public int PaidCount =>
        ScheduleItems.Count(item => item.Status == ScheduleItemStatus.Paid);

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

    public Task LoadAsync() => ReloadAsync();
}
