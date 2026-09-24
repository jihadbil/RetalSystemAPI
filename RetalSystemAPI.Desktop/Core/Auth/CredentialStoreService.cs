using System;
using System.Net;
using AdysTech.CredentialManager;

namespace RetalSystemAPI.Desktop.Core.Auth;

public class CredentialStoreService
{
    private const string TargetName = "RetalSystemAPI_Desktop_JWT";
    private const string PosDeviceTargetName = "RetalSystemAPI_Desktop_POS_DEVICE";

    public void SaveToken(string token)
    {
        try
        {
            var cred = new NetworkCredential("jwt", token);
            CredentialManager.SaveCredentials(TargetName, cred);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save credential: {ex.Message}");
        }
    }

    public string? LoadToken()
    {
        try
        {
            var cred = CredentialManager.GetCredentials(TargetName);
            return cred?.Password;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load credential: {ex.Message}");
            return null;
        }
    }

    public void ClearToken()
    {
        try
        {
            CredentialManager.RemoveCredentials(TargetName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear credential: {ex.Message}");
        }
    }

    public void SavePosDeviceToken(string token)
    {
        try
        {
            CredentialManager.SaveCredentials(PosDeviceTargetName, new NetworkCredential("pos-device", token));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save POS device credential: {ex.Message}");
        }
    }

    public string? LoadPosDeviceToken()
    {
        try
        {
            return CredentialManager.GetCredentials(PosDeviceTargetName)?.Password;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load POS device credential: {ex.Message}");
            return null;
        }
    }

    public void ClearPosDeviceToken()
    {
        try
        {
            CredentialManager.RemoveCredentials(PosDeviceTargetName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear POS device credential: {ex.Message}");
        }
    }
}
