using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

public sealed class ApplicationQueueViewModel : ViewModelBase, IPageViewModel
{
    private readonly ICreditSpecialistService _specialistService;
    private readonly UserSessionHolder _userSessionHolder;
    private readonly NavigationService _navigationService;

    private ApplicationSummaryDto? _selectedApplication;
    private ApplicationStatus _selectedStatus = ApplicationStatus.All;

    public ApplicationQueueViewModel(
        ICreditSpecialistService specialistService,
        UserSessionHolder userSessionHolder,
        NavigationService navigationService)
    {
        _specialistService = specialistService;
        _userSessionHolder = userSessionHolder;
        _navigationService = navigationService;

        Applications = [];
        Statuses = [ApplicationStatus.All];

        foreach (var status in Enum.GetValues<CreditApplicationStatus>())
        {
            Statuses.Add(new ApplicationStatus(status, StatusTextProvider.GetApplicationStatusText(status)));
        }

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        OpenReviewCommand = new RelayCommand(OpenReview, CanOpenReview);
    }

    public ObservableCollection<ApplicationSummaryDto> Applications { get; }

    public ObservableCollection<ApplicationStatus> Statuses { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public RelayCommand OpenReviewCommand { get; }

    public ApplicationStatus SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                ReloadCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ApplicationSummaryDto? SelectedApplication
    {
        get => _selectedApplication;
        set
        {
            if (SetProperty(ref _selectedApplication, value))
            {
                OpenReviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task ReloadAsync()
    {
        var statusFilter = SelectedStatus.DomainStatus;

        await ExecuteGuardedAsync(async () =>
        {
            var applications = await _specialistService.GetQueueAsync(statusFilter);

            Applications.Clear();

            foreach (var application in applications)
            {
                Applications.Add(application);
            }

            StatusMessage = Applications.Count == 0
                ? "В очереди нет заявок с выбранным статусом."
                : $"В очереди {Applications.Count} заявок.";
        });
    }

    private void OpenReview()
    {
        if (SelectedApplication is null)
        {
            return;
        }

        _navigationService.Navigate(new ApplicationReviewViewModel(
            _specialistService,
            _userSessionHolder,
            this,
            SelectedApplication.Id));
    }

    private bool CanOpenReview() => SelectedApplication is not null;

    public Task LoadAsync() => ReloadAsync();
}
