using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;

namespace RetalSystemAPI.MAUI.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string uri, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
                return errorResult ?? new ApiResponse<T> { Success = false, Message = $"خطأ في الاتصال بالسيرفر ({response.StatusCode})" };
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = "استجابة غير معروفة" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string uri, object body, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(uri, body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = $"خطأ في استجابة السيرفر ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse> PostAsync(string uri, object body, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(uri, body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse>(JsonOptions, ct);
            return result ?? new ApiResponse { Success = false, Message = $"خطأ في استجابة السيرفر ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string uri, object body, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(uri, body, JsonOptions, ct);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = $"خطأ في استجابة السيرفر ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string uri, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(uri, ct);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = $"خطأ في استجابة السيرفر ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string uri, object? body = null, CancellationToken ct = default)
    {
        try
        {
            HttpResponseMessage response;
            if (body != null)
            {
                response = await _httpClient.PatchAsJsonAsync(uri, body, JsonOptions, ct);
            }
            else
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, uri);
                response = await _httpClient.SendAsync(request, ct);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = $"خطأ في استجابة السيرفر ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }

    public async Task<byte[]?> GetByteArrayAsync(string uri, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(uri, ct);
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

    public async Task<ApiResponse<T>> PostMultipartAsync<T>(string uri, MultipartFormDataContent content, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync(uri, content, ct);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions, ct);
            return result ?? new ApiResponse<T> { Success = false, Message = $"خطأ في الرفع ({response.StatusCode})" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Message = ex.Message };
        }
    }
}
