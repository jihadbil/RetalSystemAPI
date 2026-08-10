using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace RetalSystemAPI.MAUI.Core.Auth;

public class AuthStateService : IAuthStateService
{
    private const string TokenKey = "auth_token";

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public async Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        await Task.CompletedTask;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token);
    }
}
