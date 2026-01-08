using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;

namespace SteamAccess.Utils;

public static class PermissionManager
{
    public static void GrantPermissions(SteamID steamId, string[] flags)
    {
        if (flags == null || flags.Length == 0)
            return;

        foreach (string flag in flags)
        {
            if (string.IsNullOrWhiteSpace(flag))
                continue;

            if (flag.StartsWith("#"))
            {
                AdminManager.AddPlayerToGroup(steamId, flag);
                Logger.LogDebug("Permissions", $"Granted group {flag} to SteamID: {steamId}");
            }
            else if (flag.StartsWith("@"))
            {
                AdminManager.AddPlayerPermissions(steamId, flag);
                Logger.LogDebug("Permissions", $"Granted permission {flag} to SteamID: {steamId}");
            }
        }
    }

    public static void RevokePermissions(SteamID steamId, string[] flags)
    {
        if (flags == null || flags.Length == 0)
            return;

        foreach (string flag in flags)
        {
            if (string.IsNullOrWhiteSpace(flag))
                continue;

            if (flag.StartsWith("#"))
            {
                AdminManager.RemovePlayerFromGroup(steamId, removeInheritedFlags: true, flag);
                Logger.LogDebug("Permissions", $"Revoked group {flag} from SteamID: {steamId}");
            }
            else if (flag.StartsWith("@"))
            {
                AdminManager.ClearPlayerPermissions(steamId);
                Logger.LogDebug("Permissions", $"Revoked permissions from SteamID: {steamId}");
            }
        }
    }
}