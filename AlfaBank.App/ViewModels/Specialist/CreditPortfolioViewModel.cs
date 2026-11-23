using System.Collections.ObjectModel;
using System.Globalization;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

public sealed class CreditPortfolioViewModel : ViewModelBase, IPageViewModel
{
    private const string PaymentDateFormat = "dd.MM.yyyy";

    private readonly ICreditSpecialistService _specialistService;
    private readonly UserSessionHolder _userSessionHolder;

    private CreditSummaryDto? _selectedCredit;
    private ScheduleRowDto? _selectedScheduleItem;
    private string _searchText = string.Empty;
    private string _paymentDateText = string.Empty;
    private string _paymentAmountText = string.Empty;
    private string _paymentComment = string.Empty;

    public CreditPortfolioViewModel(
        ICreditSpecialistService specialistService,
        UserSessionHolder userSessionHolder)
    {
        _specialistService = specialistService;
        _userSessionHolder = userSessionHolder;

        Credits = [];
        ScheduleItems = [];
        Payments = [];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        SelectCreditCommand = new AsyncRelayCommand(SelectCreditAsync, CanSelectCredit, onError: ReportUnexpectedError);
        RegisterPaymentCommand = new AsyncRelayCommand(RegisterPaymentAsync, CanRegisterPayment, onError: ReportUnexpectedError);
    }

    public ObservableCollection<CreditSummaryDto> Credits { get; }

    public ObservableCollection<ScheduleRowDto> ScheduleItems { get; }

    public ObservableCollection<PaymentDto> Payments { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public AsyncRelayCommand SelectCreditCommand { get; }

    public AsyncRelayCommand RegisterPaymentCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public CreditSummaryDto? SelectedCredit
    {
        get => _selectedCredit;
        set
        {
            if (SetProperty(ref _selectedCredit, value))
            {
                SelectCreditCommand.RaiseCanExecuteChanged();
                RegisterPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ScheduleRowDto? SelectedScheduleItem
    {
        get => _selectedScheduleItem;
        set
        {
            if (SetProperty(ref _selectedScheduleItem, value))
            {
                if (value is not null)
                {
                    PaymentAmountText = value.PaymentAmount.ToString("0.00");
                    PaymentDateText = DateTime.Today.ToString(PaymentDateFormat, CultureInfo.InvariantCulture);
                }

                RegisterPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PaymentDateText
    {
        get => _paymentDateText;
        set
        {
            if (SetProperty(ref _paymentDateText, value))
            {
                RegisterPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PaymentAmountText
    {
        get => _paymentAmountText;
        set
        {
            if (SetProperty(ref _paymentAmountText, value))
            {
                RegisterPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PaymentComment
    {
        get => _paymentComment;
        set => SetProperty(ref _paymentComment, value);
    }

    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var credits = await _specialistService.GetCreditsAsync(SearchText.Trim());

            var previousCreditId = SelectedCredit?.Id;

            Credits.Clear();

            foreach (var credit in credits)
            {
                Credits.Add(credit);
            }

            SelectedCredit = previousCreditId is null
                ? null
                : Credits.FirstOrDefault(credit => credit.Id == previousCreditId);

            StatusMessage = $"Загружено кредитов: {Credits.Count}.";
        });
    }

    private async Task SelectCreditAsync()
    {
        if (SelectedCredit is null)
        {
            return;
        }

        await ExecuteGuardedAsync(async () =>
        {
            var scheduleResult = await _specialistService.GetScheduleAsync(SelectedCredit.Id);
            var payments = await _specialistService.GetPaymentsAsync(SelectedCredit.Id);

            ScheduleItems.Clear();
            Payments.Clear();

            if (!scheduleResult.IsSuccess || scheduleResult.Value is null)
            {
                ShowError(scheduleResult.ErrorMessage);
            }
            else
            {
                foreach (var item in scheduleResult.Value)
                {
                    ScheduleItems.Add(item);
                }
            }

            foreach (var payment in payments)
            {
                Payments.Add(payment);
            }

            SelectedScheduleItem = null;
        });
    }

    private async Task RegisterPaymentAsync()
    {
        if (SelectedCredit is null || SelectedScheduleItem is null)
        {
            return;
        }

        var registration = new PaymentRegistrationDto(
            SelectedCredit.Id,
            SelectedScheduleItem.Id,
            ParsePaymentDate(),
            ParsePaymentAmount(),
            PaymentComment);

        var result = await ExecuteGuardedAsync(() =>
            _specialistService.RegisterPaymentAsync(registration, CurrentSpecialistId));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        ShowSuccess($"Платёж по плану № {SelectedScheduleItem.Number} зарегистрирован.");
        PaymentComment = string.Empty;

        await ReloadAsync();
        await SelectCreditAsync();
    }

    private bool CanSelectCredit() => SelectedCredit is not null;

    private bool CanRegisterPayment() =>
        SelectedCredit is not null
        && SelectedScheduleItem is not null
        && SelectedScheduleItem.Status != ScheduleItemStatus.Paid
        && ParsePaymentAmount() > 0m
        && TryParsePaymentDate()
        && ParsePaymentAmount() >= 0.01m;

    private DateOnly ParsePaymentDate() =>
        TryParsePaymentDate()
            ? DateOnly.ParseExact(PaymentDateText, PaymentDateFormat)
            : DateOnly.FromDateTime(DateTime.Today);

    private decimal ParsePaymentAmount() =>
        decimal.TryParse(PaymentAmountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : decimal.TryParse(PaymentAmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out amount)
                ? amount
                : 0m;

    private bool TryParsePaymentDate() =>
        DateOnly.TryParseExact(PaymentDateText, PaymentDateFormat, out _);

    private int CurrentSpecialistId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    public Task LoadAsync() => ReloadAsync();
}