namespace MapShared.Dto;

public class ApiError
{
    public int Code { get; set; }
    public string? Message { get; set; }

    public ApiError()
    {
        
    }
    
    public ApiError(int code, string message)
    {
        Code = code;
        Message = message;
    }

    public const int UndefinedError = -2;

    public static ApiError Undefined(string message) => new ApiError(UndefinedError, message);
}

