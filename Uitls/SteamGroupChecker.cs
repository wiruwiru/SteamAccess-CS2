using CounterStrikeSharp.API.Modules.Entities;
using System.Xml;

namespace SteamAccess.Utils;

public static class SteamGroupChecker
{
    public static async Task<bool> IsPlayerInGroupAsync(SteamID steamId, long groupId)
    {
        try
        {
            string url = $"http://steamcommunity.com/gid/{groupId}/memberslistxml/?xml=1";
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);

            HttpResponseMessage response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning("SteamCheck", $"Failed to fetch group info. Status: {response.StatusCode}");
                return false;
            }

            string groupInfo = await response.Content.ReadAsStringAsync();
            return CheckMembershipInXml(groupInfo, steamId);
        }
        catch (Exception ex)
        {
            Logger.LogError("SteamCheck", $"Error checking group membership: {ex.Message}");
            return false;
        }
    }

    private static bool CheckMembershipInXml(string xmlContent, SteamID steamId)
    {
        try
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlContent);

            XmlNodeList? steamIDNodes = xmlDoc.SelectNodes("//members/steamID64");
            if (steamIDNodes == null)
                return false;

            string steamId64String = steamId.SteamId64.ToString();
            foreach (XmlNode node in steamIDNodes)
            {
                if (node.InnerText == steamId64String)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError("SteamCheck", $"Error parsing XML: {ex.Message}");
            return false;
        }
    }
}