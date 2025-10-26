using System.Net;
using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

/// <summary>
/// Centralized API service for making HTTP requests with consistent error handling
/// </summary>
public interface IApiService
{
    Task<ApiResult<T>> GetAsync<T>(string endpoint, string? operationName = null, int timeoutSeconds = 30);
    Task<ApiResult<T>> PostAsync<T>(string endpoint, object? data = null, string? operationName = null, int timeoutSeconds = 30);
    Task<ApiResult<T>> PutAsync<T>(string endpoint, object? data = null, string? operationName = null, int timeoutSeconds = 30);
    Task<ApiResult<T>> DeleteAsync<T>(string endpoint, string? operationName = null, int timeoutSeconds = 30);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private const string DEFAULT_BASE_URL = "http://localhost:5258";

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(DEFAULT_BASE_URL);
        }
    }

    public async Task<ApiResult<T>> GetAsync<T>(string endpoint, string? operationName = null, int timeoutSeconds = 30)
    {
        return await ExecuteRequestAsync<T>(
            () => _httpClient.GetAsync(endpoint),
            operationName ?? $"GET {endpoint}",
            timeoutSeconds
        );
    }

    public async Task<ApiResult<T>> PostAsync<T>(string endpoint, object? data = null, string? operationName = null, int timeoutSeconds = 30)
    {
        return await ExecuteRequestAsync<T>(
            async () =>
            {
                var json = data != null ? JsonSerializer.Serialize(data) : string.Empty;
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PostAsync(endpoint, content);
            },
            operationName ?? $"POST {endpoint}",
            timeoutSeconds
        );
    }

    public async Task<ApiResult<T>> PutAsync<T>(string endpoint, object? data = null, string? operationName = null, int timeoutSeconds = 30)
    {
        return await ExecuteRequestAsync<T>(
            async () =>
            {
                var json = data != null ? JsonSerializer.Serialize(data) : string.Empty;
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                return await _httpClient.PutAsync(endpoint, content);
            },
            operationName ?? $"PUT {endpoint}",
            timeoutSeconds
        );
    }

    public async Task<ApiResult<T>> DeleteAsync<T>(string endpoint, string? operationName = null, int timeoutSeconds = 30)
    {
        return await ExecuteRequestAsync<T>(
            () => _httpClient.DeleteAsync(endpoint),
            operationName ?? $"DELETE {endpoint}",
            timeoutSeconds
        );
    }

    private async Task<ApiResult<T>> ExecuteRequestAsync<T>(
        Func<Task<HttpResponseMessage>> requestFunc, 
        string operationName, 
        int timeoutSeconds)
    {
        try
        {
            _logger.LogInformation("Starting {Operation}", operationName);

            // Configure timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            
            // Execute the request with timeout
            var response = await ExecuteWithTimeout(requestFunc, cts.Token);
            
            // Handle the response
            var result = await ProcessResponse<T>(response, operationName);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully completed {Operation}", operationName);
            }
            else
            {
                _logger.LogWarning("Failed {Operation}: {ErrorMessage}", operationName, result.ErrorMessage);
            }
            
            return result;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Timeout occurred for {Operation} after {TimeoutSeconds} seconds", operationName, timeoutSeconds);
            return ErrorHandler.HandleTimeout<T>(timeoutSeconds, operationName);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred for {Operation}", operationName);
            return ErrorHandler.HandleNetworkError<T>(ex, operationName);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error occurred for {Operation}", operationName);
            return ErrorHandler.HandleParsingError<T>(ex, operationName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred for {Operation}", operationName);
            return ErrorHandler.HandleUnexpectedError<T>(ex, operationName);
        }
    }

    private async Task<HttpResponseMessage> ExecuteWithTimeout(Func<Task<HttpResponseMessage>> requestFunc, CancellationToken cancellationToken)
    {
        try
        {
            return await requestFunc().WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new TaskCanceledException("The request was canceled due to timeout.");
        }
    }

    private async Task<ApiResult<T>> ProcessResponse<T>(HttpResponseMessage response, string operationName)
    {
        // Check if the response indicates success
        if (response.IsSuccessStatusCode)
        {
            return await ProcessSuccessResponse<T>(response, operationName);
        }
        else
        {
            return ErrorHandler.HandleHttpStatusError<T>(response.StatusCode, operationName);
        }
    }

    private async Task<ApiResult<T>> ProcessSuccessResponse<T>(HttpResponseMessage response, string operationName)
    {
        try
        {
            // Read the response content
            var jsonContent = await response.Content.ReadAsStringAsync();
            
            // Validate that we received content
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return ErrorHandler.HandleEmptyResponse<T>(operationName);
            }
            
            // Deserialize the response
            var data = JsonSerializer.Deserialize<T>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            // Validate deserialized data
            if (data == null)
            {
                return ErrorHandler.HandleInvalidData<T>(operationName, "Deserialization returned null");
            }
            
            return ApiResult<T>.Success(data);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response for {Operation}", operationName);
            return ErrorHandler.HandleParsingError<T>(ex, operationName);
        }
    }
}