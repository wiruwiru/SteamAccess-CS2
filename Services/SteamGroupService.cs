using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using System.Xml;

using SteamAccess.Utils;
using SteamAccess.Configs;

namespace SteamAccess.Services;

public class SteamGroupService
{
    private readonly BaseConfigs _config;
    private readonly string _steamGroupInfoUrl;
    private readonly HashSet<SteamID> _groupMembersCache = new();

    public SteamGroupService(BaseConfigs config)
    {
        _config = config;
        _steamGroupInfoUrl = $"http://steamcommunity.com/gid/{config.GroupID}/memberslistxml/?xml=1";
        Logger.LogInfo("Service", $"Steam Group URL initialized: {_steamGroupInfoUrl}");
    }

    public bool IsPlayerInGroup(SteamID steamId)
    {
        return _groupMembersCache.Contains(steamId);
    }

    public void ClearCache()
    {
        _groupMembersCache.Clear();
    }

    public void ClearAllPermissions()
    {
        foreach (SteamID steamId in _groupMembersCache)
        {
            if (_config.EnableGiveFlags)
            {
                foreach (string flag in _config.GiveFlags)
                {
                    if (flag.StartsWith("#"))
                    {
                        AdminManager.RemovePlayerFromGroup(steamId, removeInheritedFlags: true, flag);
                    }
                    else if (flag.StartsWith("@"))
                    {
                        AdminManager.ClearPlayerPermissions(steamId);
                    }
                }
            }
        }
    }

    public async Task FetchGroupMembersAsync()
    {
        try
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(_steamGroupInfoUrl);

            if (response.IsSuccessStatusCode)
            {
                string groupInfo = await response.Content.ReadAsStringAsync();
                ParseGroupMembers(groupInfo);
                Logger.LogInfo("Fetch", $"Successfully fetched group members. Total: {_groupMembersCache.Count}");
            }
            else
            {
                Logger.LogError("Fetch", $"Unable to fetch group info! Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Fetch", $"Error fetching group info: {ex.Message}");
        }
    }

    private void ParseGroupMembers(string groupInfo)
    {
        try
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(groupInfo);

            XmlNodeList? steamIDNodes = xmlDoc.SelectNodes("//members/steamID64");

            if (steamIDNodes == null)
            {
                Logger.LogWarning("Parse", "No members found in the Steam group");
                return;
            }

            int addedCount = 0;
            foreach (XmlNode node in steamIDNodes)
            {
                string steamID64 = node.InnerText;
                if (string.IsNullOrEmpty(steamID64)) continue;

                if (SteamID.TryParse(steamID64, out var steamId) && steamId != null)
                {
                    if (_config.EnableGiveFlags && _config.GiveFlags.Length > 0)
                    {
                        foreach (string flag in _config.GiveFlags)
                        {
                            if (flag.StartsWith("#"))
                            {
                                AdminManager.AddPlayerToGroup(steamId, flag);
                                Logger.LogDebug("Parse", $"Added group {flag} to SteamID: {steamId}");
                            }
                            else if (flag.StartsWith("@"))
                            {
                                AdminManager.AddPlayerPermissions(steamId, flag);
                                Logger.LogDebug("Parse", $"Added permission {flag} to SteamID: {steamId}");
                            }
                        }
                    }

                    if (!_groupMembersCache.Contains(steamId))
                    {
                        _groupMembersCache.Add(steamId);
                        addedCount++;
                    }
                }
            }

            Logger.LogInfo("Parse", $"Parsed {addedCount} new members from Steam group");
        }
        catch (Exception ex)
        {
            Logger.LogError("Parse", $"Unable to parse Steam group members: {ex.Message}");
        }
    }
}