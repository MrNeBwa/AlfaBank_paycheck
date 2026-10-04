using System.Collections.ObjectModel;
using System.Globalization;
using AlfaBank.Core.Models.Enums;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Specialist;

/// <summary>
/// Портфель выданных кредитов: график платежей и регистрация платежей клиентов.
/// </summary>
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

    /// <summary>
    /// Создаёт модель представления портфеля кредитов.
    /// </summary>
    /// <param name="specialistService">Сервис операций кредитного специалиста.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
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

    /// <summary>
    /// Кредиты клиентов банка.
    /// </summary>
    public ObservableCollection<CreditSummaryDto> Credits { get; }

    /// <summary>
    /// График платежей по выбранному кредиту.
    /// </summary>
    public ObservableCollection<ScheduleRowDto> ScheduleItems { get; }

    /// <summary>
    /// Платёжи по выбранному кредиту.
    /// </summary>
    public ObservableCollection<PaymentDto> Payments { get; }

    /// <summary>
    /// Команда обновления портфеля кредитов.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда открытия графика платежей по выбранному кредиту.
    /// </summary>
    public AsyncRelayCommand SelectCreditCommand { get; }

    /// <summary>
    /// Команда регистрации платежа клиента.
    /// </summary>
    public AsyncRelayCommand RegisterPaymentCommand { get; }

    /// <summary>
    /// Поисковый запрос: фрагмент ФИО клиента или названия продукта.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

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
                SelectCreditCommand.RaiseCanExecuteChanged();
                RegisterPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Выбранный платёж графика, предназначенный для оплаты.
    /// </summary>
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

    /// <summary>
    /// Дата платежа клиента, введённая специалистом.
    /// </summary>
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

    /// <summary>
    /// Сумма платежа клиента, введённая специалистом.
    /// </summary>
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

    /// <summary>
    /// Комментарий специалиста к платежу.
    /// </summary>
    public string PaymentComment
    {
        get => _paymentComment;
        set => SetProperty(ref _paymentComment, value);
    }

    /// <summary>
    /// Обновляет портфель кредитов с учётом поискового запроса.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var credits = await _specialistService.GetCreditsAsync(SearchText.Trim());

            // Выбранный кредит запоминается по номеру: после перезагрузки списка
            // он должен остаться выделенным, иначе после регистрации платежа
            // специалист терял открытый график.
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

    /// <summary>
    /// Загружает график платежей и историю платежей по выбранному кредиту.
    /// </summary>
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

    /// <summary>
    /// Регистрирует платёж клиента по выбранному плану графика.
    /// </summary>
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

    /// <summary>
    /// Определяет доступность загрузки графика по выбранному кредиту.
    /// </summary>
    /// <returns>Значение, если кредит выбран.</returns>
    private bool CanSelectCredit() => SelectedCredit is not null;

    /// <summary>
    /// Определяет доступность регистрации платежа.
    /// </summary>
    /// <returns>Значение, если выбран непогашенный план графика и заполнены реквизиты платежа.</returns>
    private bool CanRegisterPayment() =>
        SelectedCredit is not null
        && SelectedScheduleItem is not null
        && SelectedScheduleItem.Status != ScheduleItemStatus.Paid
        && ParsePaymentAmount() > 0m
        && TryParsePaymentDate()
        && ParsePaymentAmount() >= 0.01m;

    /// <summary>
    /// Разбирает введённую дату платежа.
    /// </summary>
    /// <returns>Дата платежа. При ошибке ввода возвращается текущая дата.</returns>
    private DateOnly ParsePaymentDate() =>
        TryParsePaymentDate()
            ? DateOnly.ParseExact(PaymentDateText, PaymentDateFormat)
            : DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Разбирает введённую сумму платежа.
    /// </summary>
    /// <returns>Сумма платежа в рублях.</returns>
    private decimal ParsePaymentAmount() =>
        decimal.TryParse(PaymentAmountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : decimal.TryParse(PaymentAmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out amount)
                ? amount
                : 0m;

    /// <summary>
    /// Проверяет корректность введённой даты платежа.
    /// </summary>
    /// <returns>Значение, если дата введена верно.</returns>
    private bool TryParsePaymentDate() =>
        DateOnly.TryParseExact(PaymentDateText, PaymentDateFormat, out _);

    /// <summary>
    /// Идентификатор кредитного специалиста, работающего с кредитом.
    /// </summary>
    private int CurrentSpecialistId => _userSessionHolder.CurrentUser?.UserId ?? 0;

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}