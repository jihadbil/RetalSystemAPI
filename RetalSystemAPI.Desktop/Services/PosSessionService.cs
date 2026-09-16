using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RetalSystemAPI.Desktop.Core.Auth;
using RetalSystemAPI.Desktop.Models.Pos;

namespace RetalSystemAPI.Desktop.Services;

public interface IPosSessionService
{
    string? ScopeKey { get; }
    PosSessionState Load();
    void Save(string scopeKey, PosSessionState state);
}

public sealed class PosSessionService(AuthStateService auth, string? directory = null) : IPosSessionService
{
    private readonly string _directory = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetalSystem", "PosDrafts");
    public string? ScopeKey => auth.IsAuthenticated && !string.IsNullOrWhiteSpace(auth.CurrentUserId)
        ? $"{auth.CurrentTenantId}:{auth.CurrentUserId}" : null;

    private string FileName(string scope) => Path.Combine(_directory, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scope))) + ".json");

    public PosSessionState Load()
    {
        if (ScopeKey is not { } scope || !File.Exists(FileName(scope))) return new();
        return JsonSerializer.Deserialize<PosSessionState>(File.ReadAllText(FileName(scope)))
            ?? throw new InvalidDataException("تعذر قراءة مسودة البيع المحفوظة.");
    }

    public void Save(string scopeKey, PosSessionState state)
    {
        // An old screen must never write its work into the next signed-in user's session.
        if (ScopeKey != scopeKey) return;
        Directory.CreateDirectory(_directory);
        var path = FileName(scopeKey);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(state));
        File.Move(temporary, path, overwrite: true);
    }
}
