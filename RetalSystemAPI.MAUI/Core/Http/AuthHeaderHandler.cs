using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Auth;

namespace RetalSystemAPI.MAUI.Core.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthStateService _authStateService;

    public AuthHeaderHandler(IAuthStateService authStateService)
    {
        _authStateService = authStateService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authStateService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
