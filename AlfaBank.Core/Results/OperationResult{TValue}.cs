namespace AlfaBank.Core.Results;

/// <summary>
/// Результат операции, возвращающий значение при успешном выполнении.
/// </summary>
/// <typeparam name="TValue">Тип возвращаемого значения.</typeparam>
public sealed class OperationResult<TValue> : OperationResult
{
    /// <summary>
    /// Полученное значение операции.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Создаёт результат операции со значением.
    /// </summary>
    /// <param name="isSuccess">Признак успеха.</param>
    /// <param name="value">Возвращаемое значение.</param>
    /// <param name="errorMessage">Текст ошибки.</param>
    private OperationResult(bool isSuccess, TValue? value, string errorMessage)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    /// <summary>
    /// Создаёт успешный результат со значением.
    /// </summary>
    /// <param name="value">Возвращаемое значение.</param>
    /// <returns>Успешный результат.</returns>
    public static OperationResult<TValue> Success(TValue value) => new(true, value, string.Empty);

    /// <summary>
    /// Создаёт успешный результат без значения.
    /// </summary>
    /// <returns>Успешный результат.</returns>
    public static new OperationResult<TValue> Success() => new(true, default, string.Empty);

    /// <summary>
    /// Создаёт результат с ошибкой.
    /// </summary>
    /// <param name="errorMessage">Текст ошибки на русском языке.</param>
    /// <returns>Результат с ошибкой.</returns>
    public static new OperationResult<TValue> Failure(string errorMessage) => new(false, default, errorMessage);
}
