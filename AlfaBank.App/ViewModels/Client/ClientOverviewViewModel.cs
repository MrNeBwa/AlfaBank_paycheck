using System.Collections.ObjectModel;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

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

    public ClientOverviewViewModel(IClientService clientService, UserSessionHolder userSessionHolder)
    {
        _clientService = clientService;
        _userSessionHolder = userSessionHolder;

        Applications = [];
        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
    }

    public ObservableCollection<ApplicationSummaryDto> Applications { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public int ApplicationsTotalCount
    {
        get => _applicationsTotalCount;
        private set => SetProperty(ref _applicationsTotalCount, value);
    }

    public int ApplicationsNewCount
    {
        get => _applicationsNewCount;
        private set => SetProperty(ref _applicationsNewCount, value);
    }

    public int ApplicationsApprovedCount
    {
        get => _applicationsApprovedCount;
        private set => SetProperty(ref _applicationsApprovedCount, value);
    }

    public int ActiveCreditsCount
    {
        get => _activeCreditsCount;
        private set => SetProperty(ref _activeCreditsCount, value);
    }

    public decimal RemainingDebtTotal
    {
        get => _remainingDebtTotal;
        private set => SetProperty(ref _remainingDebtTotal, value);
    }

    public decimal PaidTotal
    {
        get => _paidTotal;
        private set => SetProperty(ref _paidTotal, value);
    }

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

    public Task LoadAsync() => ReloadAsync();
}
