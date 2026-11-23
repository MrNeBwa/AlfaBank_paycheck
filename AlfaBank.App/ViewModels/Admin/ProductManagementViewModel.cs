using System.Collections.ObjectModel;
using AlfaBank.Core.Services.Contracts;
using AlfaBank.Core.Services.Dtos;
using AlfaBank.App.Infrastructure;

namespace AlfaBank.App.ViewModels.Admin;

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

    private int _editingProductId;

    public ProductManagementViewModel(IAdminService adminService)
    {
        _adminService = adminService;

        Products = [];

        ReloadCommand = new AsyncRelayCommand(ReloadAsync, onError: ReportUnexpectedError);
        SaveCommand = new AsyncRelayCommand(SaveAsync, onError: ReportUnexpectedError);
        ClearCommand = new RelayCommand(ClearForm);
    }

    public ObservableCollection<ProductDto> Products { get; }

    public AsyncRelayCommand ReloadCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public RelayCommand ClearCommand { get; }

    public ProductDto? SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (!SetProperty(ref _selectedProduct, value))
            {
                return;
            }

            if (value is not null)
            {
                FillFormFromProduct(value);
            }

            OnPropertyChanged(nameof(FormModeText));
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public decimal AnnualInterestRate
    {
        get => _annualInterestRate;
        set => SetProperty(ref _annualInterestRate, value);
    }

    public decimal MinAmount
    {
        get => _minAmount;
        set => SetProperty(ref _minAmount, value);
    }

    public decimal MaxAmount
    {
        get => _maxAmount;
        set => SetProperty(ref _maxAmount, value);
    }

    public int MinTermMonths
    {
        get => _minTermMonths;
        set => SetProperty(ref _minTermMonths, value);
    }

    public int MaxTermMonths
    {
        get => _maxTermMonths;
        set => SetProperty(ref _maxTermMonths, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public bool IsEditingExistingProduct
    {
        get => _isEditingExistingProduct;
        private set
        {
            if (SetProperty(ref _isEditingExistingProduct, value))
            {
                OnPropertyChanged(nameof(FormModeText));
            }
        }
    }

    public string FormModeText => IsEditingExistingProduct && _editingProductId > 0
        ? $"Редактирование кредитного продукта № {_editingProductId}"
        : "Новый кредитный продукт";

    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var products = await _adminService.GetProductsAsync(onlyActive: false);

            var wasEditing = IsEditingExistingProduct;
            var editingId = _editingProductId;

            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }

            SelectedProduct = wasEditing && editingId > 0
                ? Products.FirstOrDefault(product => product.Id == editingId)
                : null;

            StatusMessage = $"Загружено продуктов: {Products.Count}.";
        });
    }

    private async Task SaveAsync()
    {
        var wasCreating = _editingProductId == 0;
        var savedName = Name.Trim();

        var editor = new ProductEditorDto(
            _editingProductId,
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
            ShowError(result.ErrorMessage);
            return;
        }

        ClearForm();
        await ReloadAsync();

        ShowSuccess(wasCreating
            ? $"Продукт «{savedName}» создан и добавлен в справочник."
            : $"Продукт «{savedName}» сохранён.");
    }

    private void FillFormFromProduct(ProductDto product)
    {
        _editingProductId = product.Id;
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

    private void ClearForm()
    {
        _editingProductId = 0;
        IsEditingExistingProduct = false;
        SelectedProduct = null;

        Name = string.Empty;
        AnnualInterestRate = 0m;
        MinAmount = 0m;
        MaxAmount = 0m;
        MinTermMonths = 0;
        MaxTermMonths = 0;
        Description = string.Empty;
        IsActive = true;
        StatusMessage = string.Empty;
    }

    public async Task LoadAsync()
    {
        ClearForm();
        await ReloadAsync();
    }
}