namespace AlfaBank.Core.Results;

/// <summary>
/// Результат операции, возвращающий признак успеха и текст ошибки для пользователя.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Признак успешного выполнения операции.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Сообщение об ошибке в понятной пользователю формулировке.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Создаёт результат операции.
    /// </summary>
    /// <param name="isSuccess">Признак успеха.</param>
    /// <param name="errorMessage">Текст ошибки.</param>
    protected OperationResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Создаёт успешный результат операции.
    /// </summary>
    /// <returns>Успешный результат.</returns>
    public static OperationResult Success() => new(true, string.Empty);

    /// <summary>
    /// Создаёт результат с ошибкой.
    /// </summary>
    /// <param name="errorMessage">Текст ошибки на русском языке.</param>
    /// <returns>Результат с ошибкой.</returns>
    public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
}
