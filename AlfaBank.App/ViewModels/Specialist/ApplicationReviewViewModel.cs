using System.Collections.ObjectModel;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

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

    public ObservableCollection<string> Reasons { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public AsyncRelayCommand ApproveCommand { get; }

    public AsyncRelayCommand RejectCommand { get; }

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

    public string ScorePointsText
    {
        get => _scorePointsText;
        private set => SetProperty(ref _scorePointsText, value);
    }

    public string PaymentShareText
    {
        get => _paymentShareText;
        private set => SetProperty(ref _paymentShareText, value);
    }

    public string ScoreConclusion
    {
        get => _scoreConclusion;
        private set => SetProperty(ref _scoreConclusion, value);
    }

    public bool IsScoreApproved
    {
        get => _isScoreApproved;
        private set => SetProperty(ref _isScoreApproved, value);
    }

    public int ScorePoints
    {
        get => _scorePoints;
        private set => SetProperty(ref _scorePoints, value);
    }

    public int ScoreMaximumPoints => BankConstants.ScoreMaximumPoints;

    public decimal PaymentSharePercent
    {
        get => _paymentSharePercent;
        private set
        {
            if (SetProperty(ref _paymentSharePercent, value))
            {
                NotifyPaymentShareChanged();
            }
        }
    }

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

    public bool IsPaymentShareCritical => !_isPaymentShareAcceptable;

    public bool IsPaymentShareHigh =>
        _isPaymentShareAcceptable
        && _paymentSharePercent > BankConstants.PaymentShareWarningPercent + BankConstants.PaymentShareTolerance;

    public bool IsPaymentShareNormal => _isPaymentShareAcceptable && !IsPaymentShareHigh;

    public string PaymentSharePercentText => BankTextFormatter.Percent(_paymentSharePercent);

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

    public decimal DisposableIncome
    {
        get => _disposableIncome;
        private set => SetProperty(ref _disposableIncome, value);
    }

    public decimal MonthlyPayment
    {
        get => _monthlyPayment;
        private set => SetProperty(ref _monthlyPayment, value);
    }

    public bool CanDecide => Application?.Status == CreditApplicationStatus.New;

    public async Task ReloadAsync()
    {
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

    private void NotifyPaymentShareChanged()
    {
        OnPropertyChanged(nameof(IsPaymentShareCritical));
        OnPropertyChanged(nameof(IsPaymentShareHigh));
        OnPropertyChanged(nameof(IsPaymentShareNormal));
        OnPropertyChanged(nameof(PaymentSharePercentText));
        OnPropertyChanged(nameof(PaymentShareSummaryText));
    }

    private int CurrentSpecialistId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    public Task LoadAsync() => ReloadAsync();
}