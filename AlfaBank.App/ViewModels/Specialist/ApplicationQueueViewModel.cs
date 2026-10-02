using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

/// <summary>
/// Очередь заявок кредитного специалиста с фильтром по статусу.
/// </summary>
public sealed class ApplicationQueueViewModel : ViewModelBase, IPageViewModel
{
    private readonly ICreditSpecialistService _specialistService;
    private readonly UserSessionHolder _userSessionHolder;
    private readonly NavigationService _navigationService;

    private ApplicationSummaryDto? _selectedApplication;
    private ApplicationStatus _selectedStatus = ApplicationStatus.All;

    /// <summary>
    /// Создаёт модель представления очереди заявок.
    /// </summary>
    /// <param name="specialistService">Сервис операций кредитного специалиста.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    /// <param name="navigationService">Служба навигации между страницами.</param>
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

    /// <summary>
    /// Заявки, попавшие в очередь.
    /// </summary>
    public ObservableCollection<ApplicationSummaryDto> Applications { get; }

    /// <summary>
    /// Варианты фильтра по статусу заявки.
    /// </summary>
    public ObservableCollection<ApplicationStatus> Statuses { get; }

    /// <summary>
    /// Команда обновления очереди заявок.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда перехода к проверке выбранной заявки.
    /// </summary>
    public RelayCommand OpenReviewCommand { get; }

    /// <summary>
    /// Выбранный фильтр по статусу заявки.
    /// </summary>
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

    /// <summary>
    /// Выбранная заявка из очереди.
    /// </summary>
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

    /// <summary>
    /// Обновляет очередь заявок с учётом выбранного статуса.
    /// </summary>
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

    /// <summary>
    /// Открывает страницу проверки выбранной заявки.
    /// </summary>
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

    /// <summary>
    /// Определяет доступность перехода к проверке заявки.
    /// </summary>
    /// <returns>Значение, если заявка выбрана.</returns>
    private bool CanOpenReview() => SelectedApplication is not null;

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}
