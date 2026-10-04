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
    /// Идентификатор редактируемого продукта. Значение 0 означает создание нового продукта.
    /// </summary>
    private int _editingProductId;

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
            if (!SetProperty(ref _selectedProduct, value))
            {
                return;
            }

            if (value is not null)
            {
                FillFormFromProduct(value);
            }

            // Заголовок формы зависит от выбранной строки, поэтому обновляется
            // при каждом выборе. Раньше уведомление слалось только при смене
            // признака режима, и при переходе с продукта № 1 на продукт № 2
            // заголовок оставался «Редактирование продукта № 1».
            OnPropertyChanged(nameof(FormModeText));
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
        private set
        {
            if (SetProperty(ref _isEditingExistingProduct, value))
            {
                OnPropertyChanged(nameof(FormModeText));
            }
        }
    }

    /// <summary>
    /// Заголовок формы: показывает, создаётся продукт или редактируется существующий.
    /// </summary>
    public string FormModeText => IsEditingExistingProduct && _editingProductId > 0
        ? $"Редактирование кредитного продукта № {_editingProductId}"
        : "Новый кредитный продукт";

    /// <summary>
    /// Обновляет справочник кредитных продуктов.
    /// </summary>
    public async Task ReloadAsync()
    {
        await ExecuteGuardedAsync(async () =>
        {
            var products = await _adminService.GetProductsAsync(onlyActive: false);

            // Режим формы запоминается до перезагрузки: таблица при заполнении
            // может выделить строку по собственному усмотрению, и тогда форма
            // незаметно перешла бы в режим правки, хотя пользователь ничего
            // не выбирал, а новый продукт было бы уже не создать.
            var wasEditing = IsEditingExistingProduct;
            var editingId = _editingProductId;

            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }

            // После перезагрузки все элементы коллекции новые, поэтому в режиме
            // правки выделенная строка и заполненная форма соответствуют
            // прежнему продукту, а не устаревшему объекту из прошлой загрузки.
            SelectedProduct = wasEditing && editingId > 0
                ? Products.FirstOrDefault(product => product.Id == editingId)
                : null;

            StatusMessage = $"Загружено продуктов: {Products.Count}.";
        });
    }

    /// <summary>
    /// Сохраняет новый или изменённый кредитный продукт.
    /// </summary>
    private async Task SaveAsync()
    {
        // Идентификатор берётся из режима формы, а не из выделения в списке:
        // иначе сохранение после выбора строки молча изменяло бы существующий продукт.
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

        // Форма очищается до вывода результата, иначе очистка стёрла бы сообщение.
        ClearForm();
        await ReloadAsync();

        ShowSuccess(wasCreating
            ? $"Продукт «{savedName}» создан и добавлен в справочник."
            : $"Продукт «{savedName}» сохранён.");
    }

    /// <summary>
    /// Заполняет форму данными выбранного продукта.
    /// </summary>
    /// <param name="product">Выбранный продукт.</param>
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

    /// <summary>
    /// Очищает форму и переводит её в режим создания нового продукта.
    /// </summary>
    private void ClearForm()
    {
        // Сначала сбрасывается режим, затем снимается выделение:
        // при обратном порядке заголовок формы на мгновение оставался бы
        // с номером предыдущего продукта.
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

    /// <summary>
    /// Открывает страницу управления продуктами.
    /// Модель представления переиспользуется всю сессию, поэтому при каждом открытии
    /// страницы форма возвращается в режим создания: иначе она осталась бы
    /// в режиме правки выбранного продукта и новый продукт было бы не создать.
    /// </summary>
    public async Task LoadAsync()
    {
        ClearForm();
        await ReloadAsync();
    }
}