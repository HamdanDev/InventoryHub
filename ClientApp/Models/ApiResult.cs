namespace ClientApp.Models;

/// <summary>
/// Generic wrapper for API responses that encapsulates success/failure state
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class ApiResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? ErrorContext { get; set; }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static ApiResult<T> Failure(string errorMessage, string? errorCode = null, Dictionary<string, object>? context = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ErrorContext = context
        };
    }
}

/// <summary>
/// Non-generic version for operations that don't return data
/// </summary>
public class ApiResult : ApiResult<object>
{
    public static new ApiResult Success()
    {
        return new ApiResult
        {
            IsSuccess = true
        };
    }

    public static new ApiResult Failure(string errorMessage, string? errorCode = null, Dictionary<string, object>? context = null)
    {
        return new ApiResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            ErrorContext = context
        };
    }
}