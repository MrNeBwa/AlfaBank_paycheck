using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

/// <summary>
/// Проверка заявки кредитным специалистом: данные клиента, скоринг и решение.
/// </summary>
public sealed class ApplicationReviewViewModel : ViewModelBase, IPageViewModel
{
    private readonly ICreditSpecialistService _specialistService;
    private readonly UserSessionHolder _userSessionHolder;
    private readonly ApplicationQueueViewModel _queuePage;
    private readonly int _applicationId;

    private ApplicationDetailsDto? _application;
    private string _decisionComment = string.Empty;
    private string _scoreConclusion = string.Empty;
    private string _paymentShareText = string.Empty;
    private string _scorePointsText = string.Empty;

    /// <summary>
    /// Создаёт модель представления проверки заявки.
    /// </summary>
    /// <param name="specialistService">Сервис операций кредитного специалиста.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    /// <param name="queuePage">Страница очереди, обновляемая после решения.</param>
    /// <param name="applicationId">Идентификатор проверяемой заявки.</param>
    public ApplicationReviewViewModel(
        ICreditSpecialistService specialistService,
        UserSessionHolder userSessionHolder,
        ApplicationQueueViewModel queuePage,
        int applicationId)
    {
        _specialistService = specialistService;
        _userSessionHolder = userSessionHolder;
        _queuePage = queuePage;
        _applicationId = applicationId;

        Reasons = [];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        ApproveCommand = new AsyncRelayCommand(ApproveAsync, onError: ReportUnexpectedError);
        RejectCommand = new AsyncRelayCommand(RejectAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Замечания по результатам скоринга.
    /// </summary>
    public ObservableCollection<string> Reasons { get; }

    /// <summary>
    /// Команда загрузки данных заявки и расчёта скоринга.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда одобрения заявки с формированием графика платежей.
    /// </summary>
    public AsyncRelayCommand ApproveCommand { get; }

    /// <summary>
    /// Команда отклонения заявки.
    /// </summary>
    public AsyncRelayCommand RejectCommand { get; }

    /// <summary>
    /// Проверяемая заявка.
    /// </summary>
    public ApplicationDetailsDto? Application
    {
        get => _application;
        private set
        {
            if (SetProperty(ref _application, value))
            {
                OnPropertyChanged(nameof(CanDecide));
                ApproveCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Комментарий специалиста по решению.
    /// </summary>
    public string DecisionComment
    {
        get => _decisionComment;
        set
        {
            if (SetProperty(ref _decisionComment, value))
            {
                OnPropertyChanged(nameof(CanDecide));
                RejectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Балльная оценка заёмщика в текстовом виде.
    /// </summary>
    public string ScorePointsText
    {
        get => _scorePointsText;
        private set => SetProperty(ref _scorePointsText, value);
    }

    /// <summary>
    /// Показатель долговой нагрузки в текстовом виде.
    /// </summary>
    public string PaymentShareText
    {
        get => _paymentShareText;
        private set => SetProperty(ref _paymentShareText, value);
    }

    /// <summary>
    /// Вывод автоматической проверки заёмщика.
    /// </summary>
    public string ScoreConclusion
    {
        get => _scoreConclusion;
        private set => SetProperty(ref _scoreConclusion, value);
    }

    /// <summary>
    /// Признак доступности решения по заявке.
    /// </summary>
    public bool CanDecide => Application?.Status == CreditApplicationStatus.New;

    /// <summary>
    /// Загружает данные заявки и выполняет скоринг заёмщика.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var detailsResult = await _specialistService.GetApplicationDetailsAsync(_applicationId);

            if (!detailsResult.IsSuccess || detailsResult.Value is null)
            {
                StatusMessage = detailsResult.ErrorMessage;
                return;
            }

            Application = detailsResult.Value;
            DecisionComment = Application.DecisionComment;

            var scoringResult = await _specialistService.EvaluateApplicationAsync(_applicationId);

            if (!scoringResult.IsSuccess || scoringResult.Value is null)
            {
                StatusMessage = scoringResult.ErrorMessage;
                return;
            }

            ApplyScoring(scoringResult.Value);
        });
    }

    /// <summary>
    /// Одобряет заявку: система формирует кредит и аннуитетный график платежей.
    /// </summary>
    private async Task ApproveAsync()
    {
        var result = await ExecuteGuardedAsync(() =>
            _specialistService.ApproveApplicationAsync(_applicationId, CurrentSpecialistId, DecisionComment));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        StatusMessage = $"Заявка одобрена. Открыт кредит № {result.Value}, график платежей сформирован.";
        await _queuePage.ReloadAsync();
        await ReloadAsync();
    }

    /// <summary>
    /// Отклоняет заявку с обязательным комментарием специалиста.
    /// </summary>
    private async Task RejectAsync()
    {
        var result = await ExecuteGuardedAsync(() =>
            _specialistService.RejectApplicationAsync(_applicationId, CurrentSpecialistId, DecisionComment));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        StatusMessage = "Заявка отклонена.";
        await _queuePage.ReloadAsync();
        await ReloadAsync();
    }

    /// <summary>
    /// Отображает результаты скоринга.
    /// </summary>
    /// <param name="scoring">Результат оценки заёмщика.</param>
    private void ApplyScoring(ScoringResultDto scoring)
    {
        ScorePointsText = $"Балльная оценка: {scoring.ScorePoints} из 100.";
        PaymentShareText = $"ПДР (доля платежа в доходе): {scoring.PaymentSharePercent:0.##}%.";
        ScoreConclusion = scoring.Conclusion;

        Reasons.Clear();

        foreach (var reason in scoring.Reasons)
        {
            Reasons.Add(reason);
        }
    }

    /// <summary>
    /// Идентификатор кредитного специалиста, работающего с заявкой.
    /// </summary>
    private int CurrentSpecialistId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}