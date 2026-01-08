using CounterStrikeSharp.API.Modules.Entities;

using SteamAccess.Utils;
using SteamAccess.Configs;

namespace SteamAccess.Services;

public class PlayerAccessService
{
    private readonly BaseConfigs _config;
    private readonly Dictionary<SteamID, bool> _playerAccessStatus = new();
    private readonly object _lockObject = new();

    public PlayerAccessService(BaseConfigs config)
    {
        _config = config;
    }

    public bool HasAccess(SteamID steamId)
    {
        lock (_lockObject)
        {
            return _playerAccessStatus.TryGetValue(steamId, out bool hasAccess) && hasAccess;
        }
    }

    public async Task CheckAndUpdatePlayerAccessAsync(SteamID steamId, string playerName)
    {
        bool isInGroup = await SteamGroupChecker.IsPlayerInGroupAsync(steamId, _config.GroupID);
        lock (_lockObject)
        {
            bool previousStatus = _playerAccessStatus.TryGetValue(steamId, out bool status) && status;
            if (isInGroup && !previousStatus)
            {
                _playerAccessStatus[steamId] = true;

                if (_config.EnableGiveFlags)
                {
                    PermissionManager.GrantPermissions(steamId, _config.GiveFlags);
                }

                Logger.LogInfo("Access", $"Player {playerName} (SteamID: {steamId}) granted access - joined Steam group");
            }
            else if (!isInGroup && previousStatus)
            {
                _playerAccessStatus[steamId] = false;

                if (_config.EnableGiveFlags)
                {
                    PermissionManager.RevokePermissions(steamId, _config.GiveFlags);
                }

                Logger.LogInfo("Access", $"Player {playerName} (SteamID: {steamId}) access revoked - left Steam group");
            }
            else if (isInGroup)
            {
                _playerAccessStatus[steamId] = true;
                Logger.LogDebug("Access", $"Player {playerName} (SteamID: {steamId}) access confirmed - still in group");
            }
            else
            {
                _playerAccessStatus[steamId] = false;
            }
        }
    }

    public void RemovePlayer(SteamID steamId)
    {
        lock (_lockObject)
        {
            if (_playerAccessStatus.Remove(steamId))
            {
                Logger.LogDebug("Access", $"Removed player {steamId} from tracking");
            }
        }
    }

    public void ClearAll()
    {
        lock (_lockObject)
        {
            foreach (var kvp in _playerAccessStatus)
            {
                if (kvp.Value && _config.EnableGiveFlags)
                {
                    PermissionManager.RevokePermissions(kvp.Key, _config.GiveFlags);
                }
            }
            _playerAccessStatus.Clear();
        }
    }
}