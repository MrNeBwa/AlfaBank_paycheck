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
    private bool _isPaymentShareAcceptable = true;
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
    /// Максимальный балл шкалы скоринга: подпись под крупным числом не должна
    /// содержать число, расходящееся с правилами банка.
    /// </summary>
    public int ScoreMaximumPoints => BankConstants.ScoreMaximumPoints;

    /// <summary>
    /// Показатель долговой нагрузки (ПДН) в процентах.
    /// </summary>
    public decimal PaymentSharePercent
    {
        get => _paymentSharePercent;
        private set
        {
            if (SetProperty(ref _paymentSharePercent, value))
            {
                // Производные свойства вычисляются, поэтому WPF не обновляет их
                // сами: без явного уведомления на экране оставалось «0,0 %»
                // и подпись «в пределах нормы» от первоначального значения.
                NotifyPaymentShareChanged();
            }
        }
    }

    /// <summary>
    /// Признак того, что долговая нагрузка укладывается в предел банка.
    /// Берётся из результата скоринга: цвет подписи и решение заявки
    /// должны зависеть от одного и того же флага.
    /// </summary>
    public bool IsPaymentShareAcceptable
    {
        get => _isPaymentShareAcceptable;
        private set
        {
            if (SetProperty(ref _isPaymentShareAcceptable, value))
            {
                NotifyPaymentShareChanged();
            }
        }
    }

    /// <summary>
    /// Признак того, что долговая нагрузка превышает предельную.
    /// </summary>
    public bool IsPaymentShareCritical => !_isPaymentShareAcceptable;

    /// <summary>
    /// Признак того, что долговая нагрузка близка к предельной.
    /// </summary>
    public bool IsPaymentShareHigh =>
        _isPaymentShareAcceptable
        && _paymentSharePercent > BankConstants.PaymentShareWarningPercent + BankConstants.PaymentShareTolerance;

    /// <summary>
    /// Признак того, что долговая нагрузка в пределах нормы.
    /// </summary>
    public bool IsPaymentShareNormal => _isPaymentShareAcceptable && !IsPaymentShareHigh;

    /// <summary>
    /// Показатель долговой нагрузки в виде процента для крупной подписи.
    /// </summary>
    public string PaymentSharePercentText => BankTextFormatter.Percent(_paymentSharePercent);

    /// <summary>
    /// Короткая подпись к показателю долговой нагрузки: зелёная, жёлтая или красная.
    /// Текст зависит от того же флага, что и решение по заявке.
    /// </summary>
    public string PaymentShareSummaryText
    {
        get
        {
            var limit = BankTextFormatter.Percent(BankConstants.PaymentShareMaximumPercent, digits: 0);

            if (IsPaymentShareCritical)
            {
                return $"выше нормы (предел {limit}) — заявка будет отклонена";
            }

            if (IsPaymentShareHigh)
            {
                return $"в пределах нормы, но близко к пределу ({limit}) — требуется внимание специалиста";
            }

            return $"в пределах нормы (не более {limit})";
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
        // Сообщение внизу страницы относится только к действию специалиста
        // («Одобрение невозможно…» появляется после нажатия «Одобрить»).
        // При открытии заявки оно должно быть пустым, иначе результат
        // предыдущего решения остаётся на экране красной строкой.
        ResetStatusMessage();

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
    /// Сбрасывает результаты проверки: после решения заявка уходит из очереди,
    /// и на странице не должны оставаться цифры прошлой проверки.
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
        IsPaymentShareAcceptable = true;
        Reasons.Clear();
        OnPropertyChanged(nameof(CanDecide));
        ApproveCommand.RaiseCanExecuteChanged();
        RejectCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Отображает результаты скоринга.
    /// </summary>
    /// <param name="scoring">Результат оценки заёмщика.</param>
    private void ApplyScoring(ScoringResultDto scoring)
    {
        ScorePoints = scoring.ScorePoints;
        PaymentSharePercent = scoring.PaymentSharePercent;
        DisposableIncome = scoring.DisposableIncome;
        MonthlyPayment = scoring.MonthlyPayment;
        IsPaymentShareAcceptable = scoring.IsPaymentShareAcceptable;
        ScorePointsText =
            $"Балльная оценка: {scoring.ScorePoints} из {BankConstants.ScoreMaximumPoints} " +
            $"(для одобрения нужно не менее {BankConstants.ScoreMinimumToApprove}).";

        // Формула ПДН показана явно: платёж делится на свободные средства,
        // то есть на те же суммы, что используются в замечаниях скоринга.
        PaymentShareText =
            $"ПДН = платёж / свободные средства (доход − расходы): " +
            $"{BankTextFormatter.Rub(scoring.MonthlyPayment)} / " +
            $"{BankTextFormatter.Rub(scoring.DisposableIncome)} = " +
            $"{BankTextFormatter.Percent(scoring.PaymentSharePercent)} " +
            $"при пределе {BankTextFormatter.Percent(BankConstants.PaymentShareMaximumPercent, digits: 0)}.";
        ScoreConclusion = scoring.Conclusion;
        IsScoreApproved = scoring.IsApproved;

        Reasons.Clear();

        foreach (var reason in scoring.Reasons)
        {
            Reasons.Add(reason);
        }
    }

    /// <summary>
    /// Оповещает интерфейс об изменении показателя долговой нагрузки и всех
    /// производных от него подписей и цветовых меток.
    /// </summary>
    private void NotifyPaymentShareChanged()
    {
        OnPropertyChanged(nameof(IsPaymentShareCritical));
        OnPropertyChanged(nameof(IsPaymentShareHigh));
        OnPropertyChanged(nameof(IsPaymentShareNormal));
        OnPropertyChanged(nameof(PaymentSharePercentText));
        OnPropertyChanged(nameof(PaymentShareSummaryText));
    }

    /// <summary>
    /// Идентификатор кредитного специалиста, работающего с заявкой.
    /// </summary>
    private int CurrentSpecialistId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}