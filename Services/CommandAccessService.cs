using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using Microsoft.Extensions.Localization;

using SteamAccess.Utils;
using SteamAccess.Configs;

namespace SteamAccess.Services;

public class CommandAccessService
{
    private readonly BaseConfigs _config;
    private readonly PlayerAccessService _playerAccessService;
    private readonly BasePlugin _plugin;
    private readonly IStringLocalizer Localizer;
    private readonly Dictionary<string, CommandInfo.CommandListenerCallback> _commandListeners = new();

    public CommandAccessService(BaseConfigs config, PlayerAccessService playerAccessService, BasePlugin plugin, IStringLocalizer localizer)
    {
        _config = config;
        _playerAccessService = playerAccessService;
        _plugin = plugin;
        Localizer = localizer;
    }

    public void RegisterCommandListeners()
    {
        foreach (string cmd in _config.RestrictedCommands)
        {
            if (string.IsNullOrWhiteSpace(cmd)) continue;

            CommandInfo.CommandListenerCallback listener = (CCSPlayerController? player, CommandInfo info) =>
            {
                return OnRestrictedCommand(player, info, cmd);
            };

            _commandListeners[cmd] = listener;
            _plugin.AddCommandListener(cmd, listener, HookMode.Pre);
            Logger.LogDebug("Commands", $"Registered listener for command: {cmd}");
        }
    }

    public void UnregisterCommandListeners()
    {
        foreach (var kvp in _commandListeners)
        {
            _plugin.RemoveCommandListener(kvp.Key, kvp.Value, HookMode.Pre);
            Logger.LogDebug("Commands", $"Unregistered listener for command: {kvp.Key}");
        }
        _commandListeners.Clear();
    }

    private HookResult OnRestrictedCommand(CCSPlayerController? player, CommandInfo info, string command)
    {
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        if (!SteamID.TryParse(player.SteamID.ToString(), out var steamId) || steamId == null)
            return HookResult.Continue;

        if (!_playerAccessService.HasAccess(steamId))
        {
            player.PrintToChat($"{Localizer["prefix"]} {Localizer["no_permissions", command]}");
            Logger.LogDebug("Commands", $"Player {player.PlayerName} (SteamID: {steamId}) denied access to command: {command}");
            return HookResult.Handled;
        }

        Logger.LogDebug("Commands", $"Player {player.PlayerName} (SteamID: {steamId}) allowed to use command: {command}");
        return HookResult.Continue;
    }
}