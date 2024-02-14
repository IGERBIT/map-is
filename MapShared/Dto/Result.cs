namespace MapShared.Dto;

public struct Result<TValue,TError>
{
    public TValue? Value { get; set; }
    public TError? Error { get; set; }

    public bool IsSuccess => Error is null;
    public bool IsFail => Error is not null;

    public Result(TValue? value)
    {
        Value = value;
    }

    public Result(TError? error)
    {
        Error = error;
    }

    public static implicit operator Result<TValue, TError>(TValue? value)
    {
        return new Result<TValue, TError>(value);
    }
    
    public static implicit operator Result<TValue, TError>(TError? error)
    {
        return new Result<TValue, TError>(error);
    }
    
}

public static class ResultExtensions
{
    public static T ValueOrThrow<T, TException>(this Result<T, TException> result) where TException : Exception
    {
        if (result.IsFail) throw result.Error;
        return result.Value;
    }

}

