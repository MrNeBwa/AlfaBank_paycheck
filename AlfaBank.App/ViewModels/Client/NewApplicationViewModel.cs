using System.Collections.ObjectModel;
using System.Globalization;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

/// <summary>
/// Форма подачи заявки на кредит с предварительным расчётом ежемесячного платежа.
/// </summary>
public sealed class NewApplicationViewModel : ViewModelBase, IPageViewModel
{
    private readonly IClientService _clientService;
    private readonly ICreditCalculator _creditCalculator;
    private readonly UserSessionHolder _userSessionHolder;
    private readonly ClientOverviewViewModel _overviewPage;

    private ProductDto? _selectedProduct;
    private string _amountText = string.Empty;
    private string _termText = string.Empty;
    private decimal _calculatedPayment;
    private string _limitsText = string.Empty;

    /// <summary>
    /// Создаёт модель представления формы подачи заявки.
    /// </summary>
    /// <param name="clientService">Сервис операций клиента.</param>
    /// <param name="creditCalculator">Кредитный калькулятор для предварительного расчёта.</param>
    /// <param name="userSessionHolder">Хранилище данных текущего пользователя.</param>
    /// <param name="overviewPage">Страница со списком заявок, обновляемая после подачи.</param>
    public NewApplicationViewModel(
        IClientService clientService,
        ICreditCalculator creditCalculator,
        UserSessionHolder userSessionHolder,
        ClientOverviewViewModel overviewPage)
    {
        _clientService = clientService;
        _creditCalculator = creditCalculator;
        _userSessionHolder = userSessionHolder;
        _overviewPage = overviewPage;

        Products = [];
        SubmitCommand = new AsyncRelayCommand(SubmitAsync, CanSubmit, onError: ReportUnexpectedError);
        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync, onError: ReportUnexpectedError);
    }

    /// <summary>
    /// Кредитные продукты, доступные для подачи заявки.
    /// </summary>
    public ObservableCollection<ProductDto> Products { get; }

    /// <summary>
    /// Команда загрузки справочника продуктов.
    /// </summary>
    public AsyncRelayCommand LoadProductsCommand { get; }

    /// <summary>
    /// Команда отправки заявки на рассмотрение.
    /// </summary>
    public AsyncRelayCommand SubmitCommand { get; }

    /// <summary>
    /// Выбранный кредитный продукт.
    /// </summary>
    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value))
            {
                UpdateLimitsText();
                RecalculatePayment();
            }
        }
    }

    /// <summary>
    /// Запрашиваемая сумма кредита, введённая пользователем.
    /// </summary>
    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                RecalculatePayment();
            }
        }
    }

    /// <summary>
    /// Запрашиваемый срок кредита в месяцах, введённый пользователем.
    /// </summary>
    public string TermText
    {
        get => _termText;
        set
        {
            if (SetProperty(ref _termText, value))
            {
                RecalculatePayment();
            }
        }
    }

    /// <summary>
    /// Предварительный ежемесячный платёж по аннуитетной схеме.
    /// </summary>
    public decimal CalculatedPayment
    {
        get => _calculatedPayment;
        private set => SetProperty(ref _calculatedPayment, value);
    }

    /// <summary>
    /// Напоминание о лимитах выбранного продукта.
    /// </summary>
    public string LimitsText
    {
        get => _limitsText;
        private set => SetProperty(ref _limitsText, value);
    }

    /// <summary>
    /// Загружает справочник кредитных продуктов.
    /// </summary>
    public async Task LoadProductsAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var products = await _clientService.GetAvailableProductsAsync();

            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }

            if (SelectedProduct is null)
            {
                SelectedProduct = Products.FirstOrDefault();
            }

            SubmitCommand.RaiseCanExecuteChanged();
        });
    }

    /// <summary>
    /// Отправляет заявку на рассмотрение кредитному специалисту.
    /// </summary>
    private async Task SubmitAsync()
    {
        var clientProfileId = _userSessionHolder.CurrentUser?.ClientProfileId;

        if (!clientProfileId.HasValue || SelectedProduct is null)
        {
            return;
        }

        var submission = new ApplicationSubmissionDto(
            SelectedProduct.Id,
            ParseAmount(),
            ParseTerm());

        var result = await ExecuteGuardedAsync(() =>
            _clientService.SubmitApplicationAsync(clientProfileId.Value, submission));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            ShowError(result.ErrorMessage);
            return;
        }

        ShowSuccess("Заявка отправлена на рассмотрение кредитному специалисту.");
        AmountText = string.Empty;
        TermText = string.Empty;

        await _overviewPage.ReloadAsync();
    }

    /// <summary>
    /// Пересчитывает предварительный ежемесячный платёж по введённым данным.
    /// </summary>
    private void RecalculatePayment()
    {
        CalculatedPayment = SelectedProduct is null || !TryParseAmount() || !TryParseTerm()
            ? 0m
            : _creditCalculator.CalculateMonthlyPayment(ParseAmount(), SelectedProduct.AnnualInterestRate, ParseTerm());

        SubmitCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Определяет, можно ли отправить заявку.
    /// Кнопка отправки блокируется, пока продукт не выбран и данные не введены
    /// в пределах лимитов продукта: раньше она выглядела доступной всегда,
    /// но нажатие без данных не давало никакого результата.
    /// </summary>
    /// <returns>Значение, если заявка готова к отправке.</returns>
    private bool CanSubmit()
    {
        if (SelectedProduct is null || !TryParseAmount() || !TryParseTerm())
        {
            return false;
        }

        var amount = ParseAmount();
        var term = ParseTerm();

        return amount >= SelectedProduct.MinAmount
               && amount <= SelectedProduct.MaxAmount
               && term >= SelectedProduct.MinTermMonths
               && term <= SelectedProduct.MaxTermMonths;
    }

    /// <summary>
    /// Формирует текст с лимитами выбранного продукта.
    /// </summary>
    private void UpdateLimitsText() =>
        LimitsText = SelectedProduct is null
            ? "Выберите кредитный продукт."
            : $"Сумма от {SelectedProduct.MinAmount:0.##} до {SelectedProduct.MaxAmount:0.##} руб., " +
              $"срок от {SelectedProduct.MinTermMonths} до {SelectedProduct.MaxTermMonths} мес., " +
              $"ставка {SelectedProduct.AnnualInterestRate:0.##}% годовых. {SelectedProduct.Description}";

    /// <summary>
    /// Разбирает введённую сумму кредита.
    /// </summary>
    /// <returns>Сумма кредита в рублях.</returns>
    private decimal ParseAmount() =>
        decimal.TryParse(AmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount) ? amount : 0m;

    /// <summary>
    /// Разбирает введённый срок кредита.
    /// </summary>
    /// <returns>Срок кредита в месяцах.</returns>
    private int ParseTerm() =>
        int.TryParse(TermText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var term) ? term : 0;

    /// <summary>
    /// Проверяет корректность введённой суммы.
    /// </summary>
    /// <returns>Значение, если сумма введена.</returns>
    private bool TryParseAmount() =>
        decimal.TryParse(AmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount) && amount > 0m;

    /// <summary>
    /// Проверяет корректность введённого срока.
    /// </summary>
    /// <returns>Значение, если срок введён.</returns>
    private bool TryParseTerm() =>
        int.TryParse(TermText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var term) && term > 0;

    /// <inheritdoc />
    public Task LoadAsync() => LoadProductsAsync();
}
