using System.Net;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Centralized error handling utility for API operations
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Error codes for different types of failures
    /// </summary>
    public static class ErrorCodes
    {
        public const string TIMEOUT = "TIMEOUT";
        public const string NETWORK_ERROR = "NETWORK_ERROR";
        public const string PARSING_ERROR = "PARSING_ERROR";
        public const string HTTP_ERROR = "HTTP_ERROR";
        public const string UNEXPECTED_ERROR = "UNEXPECTED_ERROR";
        public const string EMPTY_RESPONSE = "EMPTY_RESPONSE";
        public const string INVALID_DATA = "INVALID_DATA";
    }

    /// <summary>
    /// Handles timeout exceptions with contextual information
    /// </summary>
    public static ApiResult<T> HandleTimeout<T>(int timeoutSeconds, string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "TimeoutSeconds", timeoutSeconds },
            { "Timestamp", DateTime.UtcNow }
        };
        
        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = operation != null 
            ? $"{operation} timed out after {timeoutSeconds} seconds. Please check your connection and try again."
            : $"Request timed out after {timeoutSeconds} seconds. Please check your connection and try again.";

        return ApiResult<T>.Failure(message, ErrorCodes.TIMEOUT, context);
    }

    /// <summary>
    /// Handles network-related exceptions
    /// </summary>
    public static ApiResult<T> HandleNetworkError<T>(HttpRequestException ex, string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "ExceptionMessage", ex.Message },
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = operation != null
            ? $"Network error during {operation}: {ex.Message}. Please check if the server is running."
            : $"Network error: {ex.Message}. Please check if the server is running.";

        return ApiResult<T>.Failure(message, ErrorCodes.NETWORK_ERROR, context);
    }

    /// <summary>
    /// Handles JSON parsing exceptions
    /// </summary>
    public static ApiResult<T> HandleParsingError<T>(JsonException ex, string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "ExceptionMessage", ex.Message },
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = operation != null
            ? $"Invalid response format received during {operation}. Please try again later."
            : "Invalid response format received from server. Please try again later.";

        return ApiResult<T>.Failure(message, ErrorCodes.PARSING_ERROR, context);
    }

    /// <summary>
    /// Handles HTTP status code errors with specific messages
    /// </summary>
    public static ApiResult<T> HandleHttpStatusError<T>(HttpStatusCode statusCode, string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "StatusCode", (int)statusCode },
            { "StatusName", statusCode.ToString() },
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = statusCode switch
        {
            HttpStatusCode.NotFound => operation != null 
                ? $"{operation} endpoint not found. Please check the server configuration."
                : "Endpoint not found. Please check the server configuration.",
            
            HttpStatusCode.Unauthorized => operation != null
                ? $"Authentication required for {operation}. Please log in and try again."
                : "Authentication required. Please log in and try again.",
                
            HttpStatusCode.Forbidden => operation != null
                ? $"Access denied for {operation}. You don't have permission to perform this action."
                : "Access denied. You don't have permission to perform this action.",
                
            HttpStatusCode.InternalServerError => operation != null
                ? $"Server error occurred during {operation}. Please try again later."
                : "Server error occurred. Please try again later.",
                
            HttpStatusCode.ServiceUnavailable => operation != null
                ? $"Service is temporarily unavailable for {operation}. Please try again later."
                : "Service is temporarily unavailable. Please try again later.",
                
            HttpStatusCode.BadRequest => operation != null
                ? $"Invalid request for {operation}. Please check your input and try again."
                : "Invalid request. Please check your input and try again.",
                
            _ => operation != null
                ? $"Server responded with status {statusCode} for {operation}. Please try again."
                : $"Server responded with status {statusCode}. Please try again."
        };

        return ApiResult<T>.Failure(message, ErrorCodes.HTTP_ERROR, context);
    }

    /// <summary>
    /// Handles unexpected exceptions
    /// </summary>
    public static ApiResult<T> HandleUnexpectedError<T>(Exception ex, string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "ExceptionType", ex.GetType().Name },
            { "ExceptionMessage", ex.Message },
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = operation != null
            ? $"An unexpected error occurred during {operation}. Please try again later."
            : "An unexpected error occurred. Please try again later.";

        return ApiResult<T>.Failure(message, ErrorCodes.UNEXPECTED_ERROR, context);
    }

    /// <summary>
    /// Handles empty or null response scenarios
    /// </summary>
    public static ApiResult<T> HandleEmptyResponse<T>(string? operation = null)
    {
        var context = new Dictionary<string, object>
        {
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);

        var message = operation != null
            ? $"Empty response received from {operation}."
            : "Empty response received from server.";

        return ApiResult<T>.Failure(message, ErrorCodes.EMPTY_RESPONSE, context);
    }

    /// <summary>
    /// Handles invalid data scenarios (e.g., failed deserialization)
    /// </summary>
    public static ApiResult<T> HandleInvalidData<T>(string? operation = null, string? additionalInfo = null)
    {
        var context = new Dictionary<string, object>
        {
            { "Timestamp", DateTime.UtcNow }
        };

        if (!string.IsNullOrEmpty(operation))
            context.Add("Operation", operation);
            
        if (!string.IsNullOrEmpty(additionalInfo))
            context.Add("AdditionalInfo", additionalInfo);

        var message = operation != null
            ? $"Failed to parse response data from {operation}."
            : "Failed to parse server response data.";

        return ApiResult<T>.Failure(message, ErrorCodes.INVALID_DATA, context);
    }
}