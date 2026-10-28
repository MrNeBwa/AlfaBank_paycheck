namespace AlfaBank.Core.Results;

public sealed class OperationResult<TValue> : OperationResult
{
    public TValue? Value { get; }

    private OperationResult(bool isSuccess, TValue? value, string errorMessage)
        : base(isSuccess, errorMessage)
    {
        Value = value;
    }

    public static OperationResult<TValue> Success(TValue value) => new(true, value, string.Empty);

    public static new OperationResult<TValue> Success() => new(true, default, string.Empty);

    public static new OperationResult<TValue> Failure(string errorMessage) => new(false, default, errorMessage);
}
