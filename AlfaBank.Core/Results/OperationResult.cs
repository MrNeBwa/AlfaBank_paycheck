namespace AlfaBank.Core.Results;

public class OperationResult
{
    public bool IsSuccess { get; }

    public string ErrorMessage { get; }

    protected OperationResult(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static OperationResult Success() => new(true, string.Empty);

    public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
}
