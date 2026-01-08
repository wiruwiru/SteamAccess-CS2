using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace SteamAccess.Configs;

public class BaseConfigs : BasePluginConfig
{
    [JsonPropertyName("GroupID")]
    public long GroupID { get; set; } = 0;

    [JsonPropertyName("CheckInterval")]
    public float CheckInterval { get; set; } = 60.0f;

    [JsonPropertyName("EnableGiveFlags")]
    public bool EnableGiveFlags { get; set; } = true;

    [JsonPropertyName("GiveFlags")]
    public string[] GiveFlags { get; set; } = { "@steamaccess/member" };

    [JsonPropertyName("EnableCommandAccess")]
    public bool EnableCommandAccess { get; set; } = true;

    [JsonPropertyName("RestrictedCommands")]
    public string[] RestrictedCommands { get; set; } =
    {
        "css_yourcommand",
        "css_morecommands"
    };

    [JsonPropertyName("EnableDebug")]
    public bool EnableDebug { get; set; } = false;

    [JsonPropertyName("ConfigVersion")]
    public override int Version { get; set; } = 1;
}