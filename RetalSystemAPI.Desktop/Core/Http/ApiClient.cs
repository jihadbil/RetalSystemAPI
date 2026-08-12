using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Auth;

namespace RetalSystemAPI.Desktop.Core.Http;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthStateService _authState;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, AuthStateService authState)
    {
        _httpClient = httpClient;
        _authState = authState;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (content != null)
        {
            request.Content = content;
        }

        if (_authState.IsAuthenticated && !string.IsNullOrEmpty(_authState.JwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.JwtToken);
        }

        return request;
    }

    public async Task<byte[]?> DownloadFileAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Get, endpoint);
            using var response = await _httpClient.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync(ct);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<T>> PostFileAsync<T>(string endpoint, string filePath, CancellationToken ct = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", Path.GetFileName(filePath));

            using var request = CreateRequest(HttpMethod.Post, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Get, endpoint);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<T> { Success = false, Message = "تعذر الاتصال بالخادم (API). يرجى التأكد من تشغيل خادم RetalSystemAPI على العنوان https://localhost:7226", ErrorCode = "NETWORK_ERROR" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Post, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (HttpRequestException)
        {
            return new ApiResponse<T> { Success = false, Message = "تعذر الاتصال بالخادم (API). يرجى التأكد من تشغيل خادم RetalSystemAPI على العنوان https://localhost:7226", ErrorCode = "NETWORK_ERROR" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse> PostAsync(string endpoint, object body, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Post, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleVoidResponseAsync(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Put, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Delete, endpoint);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleVoidResponseAsync(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Patch, endpoint);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object body, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = CreateRequest(HttpMethod.Patch, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse> PatchAsync(string endpoint, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Patch, endpoint);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleVoidResponseAsync(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    public async Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default)
    {
        try
        {
            using var request = CreateRequest(HttpMethod.Post, endpoint, content);
            using var response = await _httpClient.SendAsync(request, ct);
            return await HandleResponseAsync<T>(response, ct);
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message, ErrorCode = "NETWORK_ERROR" };
        }
    }

    private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _authState.ClearToken();
            return new ApiResponse<T> { Success = false, Message = "انتهت الجلسة أو غير مصرح به", ErrorCode = "UNAUTHORIZED" };
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ApiResponse<T> { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
        }

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
            if (result != null)
            {
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                }

                if (!result.Success && string.IsNullOrWhiteSpace(result.Message))
                {
                    result.Message = ExtractValidationErrorMessage(json) ?? response.ReasonPhrase ?? "خطأ في تنفيذ الطلب";
                }
                return result;
            }
        }
        catch
        {
            // تجاهل خطأ التحليل المباشر لتجربة استخراج الأخطاء من الـ JSON
        }

        var fallbackMsg = ExtractValidationErrorMessage(json) ?? response.ReasonPhrase ?? "فشل في معالجة الاستجابة";
        return new ApiResponse<T> { Success = response.IsSuccessStatusCode, Message = fallbackMsg };
    }

    private async Task<ApiResponse> HandleVoidResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _authState.ClearToken();
            return new ApiResponse { Success = false, Message = "انتهت الجلسة أو غير مصرح به", ErrorCode = "UNAUTHORIZED" };
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ApiResponse { Success = response.IsSuccessStatusCode, Message = response.ReasonPhrase };
        }

        try
        {
            var result = JsonSerializer.Deserialize<ApiResponse>(json, _jsonOptions);
            if (result != null)
            {
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                }

                if (!result.Success && string.IsNullOrWhiteSpace(result.Message))
                {
                    result.Message = ExtractValidationErrorMessage(json) ?? response.ReasonPhrase ?? "خطأ في تنفيذ الطلب";
                }
                return result;
            }
        }
        catch
        {
            // تجاهل خطأ التحليل المباشر
        }

        var fallbackMsg = ExtractValidationErrorMessage(json) ?? response.ReasonPhrase ?? "فشل في معالجة الاستجابة";
        return new ApiResponse { Success = response.IsSuccessStatusCode, Message = fallbackMsg };
    }

    private string? ExtractValidationErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
            {
                var errorMessages = new System.Collections.Generic.List<string>();
                foreach (var prop in errorsProp.EnumerateObject())
                {
                    foreach (var err in prop.Value.EnumerateArray())
                    {
                        var str = err.GetString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            errorMessages.Add(str);
                        }
                    }
                }
                if (errorMessages.Count > 0)
                {
                    return string.Join(" | ", errorMessages);
                }
            }

            if (root.TryGetProperty("title", out var titleProp))
            {
                return titleProp.GetString();
            }
        }
        catch
        {
            // Ignore JSON parsing errors
        }

        return null;
    }
}
