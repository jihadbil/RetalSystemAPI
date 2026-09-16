using System;
using System.Text;
using System.Text.Json;

namespace RetalSystemAPI.Desktop.Core.Auth;

public class AuthStateService
{
    private readonly CredentialStoreService _credentialStore;

    public string? JwtToken { get; private set; }
    public string? CurrentUserId { get; private set; }
    public string? CurrentUserName { get; private set; }
    public Guid? CurrentTenantId { get; private set; }
    public System.Collections.Generic.List<string> CurrentRoles { get; private set; } = new();
    public System.Collections.Generic.HashSet<string> CurrentPermissions { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);
    public bool IsCashier => CurrentRoles.Any(r => r.Equals("Cashier", StringComparison.OrdinalIgnoreCase) || r.Equals("كاشير", StringComparison.OrdinalIgnoreCase)) && !IsAdmin;
    public bool IsAdmin => CurrentRoles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) || r.Equals("مدير", StringComparison.OrdinalIgnoreCase));

    public bool HasPermission(string permission)
    {
        if (IsAdmin) return true;
        if (string.IsNullOrWhiteSpace(permission)) return true;
        return CurrentPermissions.Contains(permission);
    }

    public bool CanAccessScreen(string screenPermission)
    {
        return HasPermission(screenPermission);
    }

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

    public void SetToken(
        string token, 
        string? userId = null, 
        Guid? tenantId = null, 
        bool rememberMe = false, 
        System.Collections.Generic.List<string>? roles = null, 
        string? userName = null,
        System.Collections.Generic.IEnumerable<string>? permissions = null)
    {
        ProcessToken(token, saveToStore: rememberMe, fallbackUserId: userId, fallbackTenantId: tenantId, fallbackRoles: roles, fallbackUserName: userName, fallbackPermissions: permissions);
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearToken()
    {
        JwtToken = null;
        CurrentUserId = null;
        CurrentUserName = null;
        CurrentTenantId = null;
        CurrentRoles.Clear();
        CurrentPermissions.Clear();
        _credentialStore.ClearToken();
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ProcessToken(
        string token, 
        bool saveToStore, 
        string? fallbackUserId = null, 
        Guid? fallbackTenantId = null, 
        System.Collections.Generic.List<string>? fallbackRoles = null, 
        string? fallbackUserName = null,
        System.Collections.Generic.IEnumerable<string>? fallbackPermissions = null)
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

            // 4. استخراج اسم المستخدم (UserName)
            if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", out var nameProp) ||
                root.TryGetProperty("name", out nameProp) ||
                root.TryGetProperty("unique_name", out nameProp))
            {
                CurrentUserName = nameProp.GetString();
            }
            else
            {
                CurrentUserName = fallbackUserName;
            }

            // 5. استخراج الأدوار (Roles)
            CurrentRoles.Clear();
            if (root.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleProp) ||
                root.TryGetProperty("role", out roleProp))
            {
                if (roleProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleProp.EnumerateArray())
                    {
                        var s = r.GetString();
                        if (!string.IsNullOrWhiteSpace(s)) CurrentRoles.Add(s);
                    }
                }
                else if (roleProp.ValueKind == JsonValueKind.String)
                {
                    var s = roleProp.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) CurrentRoles.Add(s);
                }
            }
            else if (fallbackRoles != null && fallbackRoles.Count > 0)
            {
                CurrentRoles.AddRange(fallbackRoles);
            }

            // 6. استخراج الصلاحيات (Permissions)
            CurrentPermissions.Clear();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.NameEquals("Permission") || prop.NameEquals("permission"))
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var elem in prop.Value.EnumerateArray())
                        {
                            var perm = elem.GetString();
                            if (!string.IsNullOrWhiteSpace(perm))
                            {
                                CurrentPermissions.Add(perm);
                            }
                        }
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        var perm = prop.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(perm))
                        {
                            CurrentPermissions.Add(perm);
                        }
                    }
                }
            }

            if (fallbackPermissions != null)
            {
                foreach (var perm in fallbackPermissions)
                {
                    if (!string.IsNullOrWhiteSpace(perm))
                    {
                        CurrentPermissions.Add(perm);
                    }
                }
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
