using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Auth;

namespace RetalSystemAPI.MAUI.Services;

public interface IAuthApiService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<ApiResponse> LogoutAsync(CancellationToken ct = default);
}

public class AuthApiService : IAuthApiService
{
    private readonly ApiClient _apiClient;

    public AuthApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<AuthResponseDto>("api/auth/login", dto, ct);
    }

    public Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<AuthResponseDto>("api/auth/register", dto, ct);
    }

    public Task<ApiResponse> LogoutAsync(CancellationToken ct = default)
    {
        return _apiClient.PostAsync("api/auth/logout", new { }, ct);
    }
}
