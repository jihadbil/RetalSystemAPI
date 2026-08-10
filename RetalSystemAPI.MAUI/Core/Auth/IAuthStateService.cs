using System.Threading.Tasks;

namespace RetalSystemAPI.MAUI.Core.Auth;

public interface IAuthStateService
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task ClearTokenAsync();
    Task<bool> IsAuthenticatedAsync();
}
