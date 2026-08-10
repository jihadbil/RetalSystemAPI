using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Auth;

namespace RetalSystemAPI.Desktop.Services;

public interface IAuthApiService
{
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<ApiResponse> LogoutAsync(CancellationToken ct = default);
}

public class AuthApiService : IAuthApiService
{
    private readonly ApiClient _apiClient;

    public AuthApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<AuthResponseDto>("auth/login", request, ct);
    }

    public Task<ApiResponse> LogoutAsync(CancellationToken ct = default)
    {
        return _apiClient.PostAsync("auth/logout", new { }, ct);
    }
}
