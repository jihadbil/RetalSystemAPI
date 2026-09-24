using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Pos;

namespace RetalSystemAPI.Desktop.Services.Accounting;

public interface IPosCashApiService
{
    Task<ApiResponse<ClaimPosDeviceResponse>> ClaimAsync(Guid tenantId, string pairingCode, CancellationToken ct = default);
    Task<ApiResponse<PosDeviceContextInfo>> GetContextAsync(CancellationToken ct = default);
    Task<ApiResponse<CashSessionInfo>> OpenAsync(decimal openingFloat, CancellationToken ct = default);
    Task<ApiResponse<CashSessionInfo>> CloseAsync(decimal countedBalance, string? notes = null, CancellationToken ct = default);
}

public sealed class PosCashApiService(ApiClient apiClient, CredentialStoreService credentialStore) : IPosCashApiService
{
    public async Task<ApiResponse<ClaimPosDeviceResponse>> ClaimAsync(Guid tenantId, string pairingCode, CancellationToken ct = default)
    {
        var result = await apiClient.PostAsync<ClaimPosDeviceResponse>("pos-device/claim", new { tenantId, pairingCode }, ct);
        if (result.Success && result.Data != null) credentialStore.SavePosDeviceToken(result.Data.DeviceToken);
        return result;
    }

    public Task<ApiResponse<PosDeviceContextInfo>> GetContextAsync(CancellationToken ct = default) => apiClient.GetAsync<PosDeviceContextInfo>("pos-device/context", ct);
    public Task<ApiResponse<CashSessionInfo>> OpenAsync(decimal openingFloat, CancellationToken ct = default) => apiClient.PostAsync<CashSessionInfo>("pos-device/sessions/open", new { openingFloat }, ct);
    public Task<ApiResponse<CashSessionInfo>> CloseAsync(decimal countedBalance, string? notes = null, CancellationToken ct = default) => apiClient.PostAsync<CashSessionInfo>("pos-device/sessions/close", new { countedClosingBalance = countedBalance, notes }, ct);
}
