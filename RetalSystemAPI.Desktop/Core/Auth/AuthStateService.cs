using System;
using System.Text;
using System.Text.Json;

namespace RetalSystemAPI.Desktop.Core.Auth;

public class AuthStateService
{
    private readonly CredentialStoreService _credentialStore;

    public string? JwtToken { get; private set; }
    public string? CurrentUserId { get; private set; }
    public Guid? CurrentTenantId { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

    public event EventHandler? AuthStateChanged;

    public AuthStateService(CredentialStoreService credentialStore)
    {
        _credentialStore = credentialStore;
        // تحميل التوكن المحفوظ فقط إذا كان متوفراً والتحقق من صحته
        var savedToken = _credentialStore.LoadToken();
        if (!string.IsNullOrEmpty(savedToken))
        {
            ProcessToken(savedToken, saveToStore: false);
        }
    }

    public void SetToken(string token, string? userId = null, Guid? tenantId = null, bool rememberMe = false)
    {
        ProcessToken(token, saveToStore: rememberMe, fallbackUserId: userId, fallbackTenantId: tenantId);
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearToken()
    {
        JwtToken = null;
        CurrentUserId = null;
        CurrentTenantId = null;
        _credentialStore.ClearToken();
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ProcessToken(string token, bool saveToStore, string? fallbackUserId = null, Guid? fallbackTenantId = null)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                ClearToken();
                return;
            }

            string payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);
            var root = doc.RootElement;

            // 1. فحص تاريخ انتهاء التوكن (exp)
            if (root.TryGetProperty("exp", out var expProp) && expProp.TryGetInt64(out var expUnix))
            {
                var expTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                if (expTime <= DateTime.UtcNow)
                {
                    ClearToken();
                    return;
                }
            }

            // 2. استخراج معرف المستخدم (UserId)
            if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var subProp) ||
                root.TryGetProperty("sub", out subProp))
            {
                CurrentUserId = subProp.GetString();
            }
            else
            {
                CurrentUserId = fallbackUserId;
            }

            // 3. استخراج معرف المستأجر (TenantId)
            if (root.TryGetProperty("TenantId", out var tenantProp) && Guid.TryParse(tenantProp.GetString(), out var tenantId))
            {
                CurrentTenantId = tenantId;
            }
            else
            {
                CurrentTenantId = fallbackTenantId;
            }

            JwtToken = token;
            if (saveToStore)
            {
                _credentialStore.SaveToken(token);
            }
            else
            {
                // إذا لم يتم تحديد "تذكرني"، تأكد من إزالة أي توكن قديم محفوظ من أجهزة التخزين
                _credentialStore.ClearToken();
            }
        }
        catch
        {
            ClearToken();
        }
    }
}
