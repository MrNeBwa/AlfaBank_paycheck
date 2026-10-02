using System.Collections.ObjectModel;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Admin;

/// <summary>
/// Управление справочником кредитных продуктов.
/// </summary>
public sealed class ProductManagementViewModel : ViewModelBase, IPageViewModel
{
    private readonly IAdminService _adminService;

    private ProductDto? _selectedProduct;
    private string _name = string.Empty;
    private decimal _annualInterestRate;
    private decimal _minAmount;
    private decimal _maxAmount;
    private int _minTermMonths;
    private int _maxTermMonths;
    private string _description = string.Empty;
    private bool _isActive = true;
    private bool _isEditingExistingProduct;

    /// <summary>
    /// Создаёт модель представления справочника кредитных продуктов.
    /// </summary>
    /// <param name="adminService">Сервис операций администратора.</param>
    public ProductManagementViewModel(IAdminService adminService)
    {
        _adminService = adminService;

        Products = [];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        SaveCommand = new AsyncRelayCommand(SaveAsync, onError: ReportUnexpectedError);
        ClearCommand = new RelayCommand(ClearForm);
    }

    /// <summary>
    /// Кредитные продукты справочника.
    /// </summary>
    public ObservableCollection<ProductDto> Products { get; }

    /// <summary>
    /// Команда обновления справочника.
    /// </summary>
    public AsyncRelayCommand ReloadCommand { get; }

    /// <summary>
    /// Команда сохранения продукта.
    /// </summary>
    public AsyncRelayCommand SaveCommand { get; }

    /// <summary>
    /// Команда очистки формы.
    /// </summary>
    public RelayCommand ClearCommand { get; }

    /// <summary>
    /// Выбранный продукт справочника.
    /// </summary>
    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (SetProperty(ref _selectedProduct, value) && value is not null)
            {
                FillFormFromProduct(value);
            }
        }
    }

    /// <summary>
    /// Наименование кредитного продукта.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Годовая процентная ставка.
    /// </summary>
    public decimal AnnualInterestRate
    {
        get => _annualInterestRate;
        set => SetProperty(ref _annualInterestRate, value);
    }

    /// <summary>
    /// Минимальная сумма кредита.
    /// </summary>
    public decimal MinAmount
    {
        get => _minAmount;
        set => SetProperty(ref _minAmount, value);
    }

    /// <summary>
    /// Максимальная сумма кредита.
    /// </summary>
    public decimal MaxAmount
    {
        get => _maxAmount;
        set => SetProperty(ref _maxAmount, value);
    }

    /// <summary>
    /// Минимальный срок кредита в месяцах.
    /// </summary>
    public int MinTermMonths
    {
        get => _minTermMonths;
        set => SetProperty(ref _minTermMonths, value);
    }

    /// <summary>
    /// Максимальный срок кредита в месяцах.
    /// </summary>
    public int MaxTermMonths
    {
        get => _maxTermMonths;
        set => SetProperty(ref _maxTermMonths, value);
    }

    /// <summary>
    /// Пояснение к продукту.
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Признак доступности продукта для новых заявок.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    /// <summary>
    /// Признак режима редактирования существующего продукта.
    /// </summary>
    public bool IsEditingExistingProduct
    {
        get => _isEditingExistingProduct;
        private set => SetProperty(ref _isEditingExistingProduct, value);
    }

    /// <summary>
    /// Обновляет справочник кредитных продуктов.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var products = await _adminService.GetProductsAsync(onlyActive: false);

            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }

            StatusMessage = $"Загружено продуктов: {Products.Count}.";
        });
    }

    /// <summary>
    /// Сохраняет новый или изменённый кредитный продукт.
    /// </summary>
    private async Task SaveAsync()
    {
        var editor = new ProductEditorDto(
            SelectedProduct?.Id ?? 0,
            Name.Trim(),
            AnnualInterestRate,
            MinAmount,
            MaxAmount,
            MinTermMonths,
            MaxTermMonths,
            Description.Trim(),
            IsActive);

        var result = await ExecuteGuardedAsync(() => _adminService.SaveProductAsync(editor));

        if (result is null)
        {
            return;
        }

        if (!result.IsSuccess)
        {
            StatusMessage = result.ErrorMessage;
            return;
        }

        StatusMessage = $"Продукт «{Name}» сохранён.";
        ClearForm();
        await ReloadAsync();
    }

    /// <summary>
    /// Заполняет форму данными выбранного продукта.
    /// </summary>
    /// <param name="product">Выбранный продукт.</param>
    private void FillFormFromProduct(ProductDto product)
    {
        Name = product.Name;
        AnnualInterestRate = product.AnnualInterestRate;
        MinAmount = product.MinAmount;
        MaxAmount = product.MaxAmount;
        MinTermMonths = product.MinTermMonths;
        MaxTermMonths = product.MaxTermMonths;
        Description = product.Description;
        IsActive = product.IsActive;
        IsEditingExistingProduct = true;
    }

    /// <summary>
    /// Очищает форму и переводит её в режим создания нового продукта.
    /// </summary>
    private void ClearForm()
    {
        SelectedProduct = null;
        Name = string.Empty;
        AnnualInterestRate = 0m;
        MinAmount = 0m;
        MaxAmount = 0m;
        MinTermMonths = 0;
        MaxTermMonths = 0;
        Description = string.Empty;
        IsActive = true;
        IsEditingExistingProduct = false;
        StatusMessage = string.Empty;
    }

    /// <inheritdoc />
    public Task LoadAsync() => ReloadAsync();
}