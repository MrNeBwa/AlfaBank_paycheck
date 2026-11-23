using System.Collections.ObjectModel;
using System.Globalization;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Client;

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

    public ObservableCollection<ProductDto> Products { get; }

    public AsyncRelayCommand LoadProductsCommand { get; }

    public AsyncRelayCommand SubmitCommand { get; }

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

    public decimal CalculatedPayment
    {
        get => _calculatedPayment;
        private set => SetProperty(ref _calculatedPayment, value);
    }

    public string LimitsText
    {
        get => _limitsText;
        private set => SetProperty(ref _limitsText, value);
    }

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

    private void RecalculatePayment()
    {
        CalculatedPayment = SelectedProduct is null || !TryParseAmount() || !TryParseTerm()
            ? 0m
            : _creditCalculator.CalculateMonthlyPayment(ParseAmount(), SelectedProduct.AnnualInterestRate, ParseTerm());

        SubmitCommand.RaiseCanExecuteChanged();
    }

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

    private void UpdateLimitsText() =>
        LimitsText = SelectedProduct is null
            ? "Выберите кредитный продукт."
            : $"Сумма от {SelectedProduct.MinAmount:0.##} до {SelectedProduct.MaxAmount:0.##} руб., " +
              $"срок от {SelectedProduct.MinTermMonths} до {SelectedProduct.MaxTermMonths} мес., " +
              $"ставка {SelectedProduct.AnnualInterestRate:0.##}% годовых. {SelectedProduct.Description}";

    private decimal ParseAmount() =>
        decimal.TryParse(AmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount) ? amount : 0m;

    private int ParseTerm() =>
        int.TryParse(TermText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var term) ? term : 0;

    private bool TryParseAmount() =>
        decimal.TryParse(AmountText, NumberStyles.Any, CultureInfo.CurrentCulture, out var amount) && amount > 0m;

    private bool TryParseTerm() =>
        int.TryParse(TermText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var term) && term > 0;

    public Task LoadAsync() => LoadProductsAsync();
}
