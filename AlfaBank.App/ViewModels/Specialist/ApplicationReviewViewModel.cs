using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services;
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
    private bool _isScoreApproved;
    private int _scorePoints;
    private decimal _paymentSharePercent;
    private decimal _disposableIncome;
    private decimal _monthlyPayment;

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
    /// Признак того, что заёмщик прошёл автоматическую проверку.
    /// Определяет цвет блока с результатами скоринга: зелёный при успехе, красный при отказе.
    /// </summary>
    public bool IsScoreApproved
    {
        get => _isScoreApproved;
        private set => SetProperty(ref _isScoreApproved, value);
    }

    /// <summary>
    /// Балльная оценка заёмщика числом: показывается крупно, как в личном кабинете банка.
    /// </summary>
    public int ScorePoints
    {
        get => _scorePoints;
        private set => SetProperty(ref _scorePoints, value);
    }

    /// <summary>
    /// Показатель долговой нагрузки в процентах.
    /// </summary>
    public decimal PaymentSharePercent
    {
        get => _paymentSharePercent;
        private set
        {
            if (SetProperty(ref _paymentSharePercent, value))
            {
                OnPropertyChanged(nameof(IsPaymentShareHigh));
                OnPropertyChanged(nameof(IsPaymentShareCritical));
            }
        }
    }

    /// <summary>
    /// Признак того, что долговая нагрузка близка к предельной.
    /// </summary>
    public bool IsPaymentShareHigh =>
        _paymentSharePercent > BankConstants.PaymentShareWarningPercent + 0.0001m
        && _paymentSharePercent <= BankConstants.PaymentShareMaximumPercent + 0.0001m;

    /// <summary>
    /// Признак того, что долговая нагрузка превышает предельную.
    /// </summary>
    public bool IsPaymentShareCritical =>
        _paymentSharePercent > BankConstants.PaymentShareMaximumPercent + 0.0001m;

    /// <summary>
    /// Признак того, что долговая нагрузка в пределах нормы.
    /// </summary>
    public bool IsPaymentShareNormal =>
        _paymentSharePercent <= BankConstants.PaymentShareWarningPercent + 0.0001m;

    /// <summary>
    /// Показатель долговой нагрузки в виде процента для крупной подписи.
    /// </summary>
    public string PaymentSharePercentText => $"{_paymentSharePercent:F1}%";

    /// <summary>
    /// Короткая подпись к показателю долговой нагрузки: зелёная, жёлтая или красная.
    /// </summary>
    public string PaymentShareSummaryText
    {
        get
        {
            if (IsPaymentShareCritical)
            {
                return $"выше предельных {BankConstants.PaymentShareMaximumPercent:F0}% — заявка будет отклонена";
            }

            if (IsPaymentShareHigh)
            {
                return "нагрузка близка к предельной, требуется внимание специалиста";
            }

            return $"в пределах нормы (не более {BankConstants.PaymentShareMaximumPercent:F0}%)";
        }
    }

    /// <summary>
    /// Свободные средства заёмщика после вычета расходов.
    /// </summary>
    public decimal DisposableIncome
    {
        get => _disposableIncome;
        private set => SetProperty(ref _disposableIncome, value);
    }

    /// <summary>
    /// Расчётный ежемесячный платёж по кредиту.
    /// </summary>
    public decimal MonthlyPayment
    {
        get => _monthlyPayment;
        private set => SetProperty(ref _monthlyPayment, value);
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
                ShowError(detailsResult.ErrorMessage);
                return;
            }

            Application = detailsResult.Value;
            DecisionComment = Application.DecisionComment;

            var scoringResult = await _specialistService.EvaluateApplicationAsync(_applicationId);

            if (!scoringResult.IsSuccess || scoringResult.Value is null)
            {
                ShowError(scoringResult.ErrorMessage);
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
            ShowError(result.ErrorMessage);
            return;
        }

        ShowSuccess($"Заявка одобрена. Открыт кредит № {result.Value}, график платежей сформирован.");
        await _queuePage.ReloadAsync();
        ClearScoringState();
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
            ShowError(result.ErrorMessage);
            return;
        }

        ShowError("Заявка отклонена.");
        await _queuePage.ReloadAsync();
        ClearScoringState();
    }

    /// <summary>
    /// Отображает результаты скоринга.
    /// </summary>
    private void ClearScoringState()
    {
        Application = null;
        DecisionComment = string.Empty;
        ScorePoints = 0;
        PaymentSharePercent = 0m;
        DisposableIncome = 0m;
        MonthlyPayment = 0m;
        ScorePointsText = string.Empty;
        PaymentShareText = string.Empty;
        ScoreConclusion = string.Empty;
        IsScoreApproved = false;
        Reasons.Clear();
        OnPropertyChanged(nameof(CanDecide));
        ApproveCommand.RaiseCanExecuteChanged();
        RejectCommand.RaiseCanExecuteChanged();
    }

    private void ApplyScoring(ScoringResultDto scoring)
    {
        ScorePoints = scoring.ScorePoints;
        PaymentSharePercent = scoring.PaymentSharePercent;
        DisposableIncome = scoring.DisposableIncome;
        MonthlyPayment = scoring.MonthlyPayment;
        ScorePointsText =
            $"Балльная оценка: {scoring.ScorePoints} из 100 " +
            $"(для одобрения нужно не менее {BankConstants.ScoreMinimumToApprove}).";
        PaymentShareText =
            $"Платёж по кредиту {scoring.MonthlyPayment:F2} руб. — это " +
            $"{scoring.PaymentSharePercent:F1}% свободных средств заёмщика " +
            $"({scoring.DisposableIncome:F2} руб. в месяц после расходов). " +
            $"Предельная доля — {BankConstants.PaymentShareMaximumPercent:F0}%.";
        ScoreConclusion = scoring.Conclusion;
        IsScoreApproved = scoring.IsApproved;

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