using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Core.Attributes.Registration;

using SteamAccess.Configs;
using SteamAccess.Services;

namespace SteamAccess;

[MinimumApiVersion(354)]
public class SteamAccess : BasePlugin, IPluginConfig<BaseConfigs>
{
    public override string ModuleName => "SteamAccess";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "luca.uy";
    public override string ModuleDescription => "Rewards Steam group members with flags and command access";

    public BaseConfigs Config { get; set; } = new();
    private PlayerAccessService? _playerAccessService;
    private CommandAccessService? _commandAccessService;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _periodicCheckTimer;

    public void OnConfigParsed(BaseConfigs config)
    {
        if (config.GroupID == 0)
        {
            throw new Exception("Invalid value has been set for config value `GroupID`");
        }

        Config = config;
        Utils.Logger.Config = config;

        Utils.Logger.LogInfo("Config", "Configuration loaded successfully");
    }

    public override void Load(bool hotReload)
    {
        _playerAccessService = new PlayerAccessService(Config);
        _commandAccessService = new CommandAccessService(Config, _playerAccessService, this, Localizer);

        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        if (Config.EnableCommandAccess && Config.RestrictedCommands.Length > 0)
        {
            _commandAccessService.RegisterCommandListeners();
        }

        if (hotReload)
        {
            OnMapStartHandler(string.Empty);
            CheckAllConnectedPlayers();
        }

        StartPeriodicCheck();
        Utils.Logger.LogInfo("Load", "Plugin loaded successfully");
    }

    private void StartPeriodicCheck()
    {
        float checkInterval = Config.CheckInterval;
        if (checkInterval <= 0)
        {
            Utils.Logger.LogError("PeriodicCheck", $"Invalid CheckInterval: {checkInterval}. Must be greater than 0. Periodic check will not start.");
            return;
        }

        _periodicCheckTimer = AddTimer(checkInterval, () =>
        {
            Utils.Logger.LogInfo("PeriodicCheck", "Timer callback executed - running periodic check");
            Utils.Logger.LogDebug("PeriodicCheck", "Running periodic group membership check for connected players...");
            CheckAllConnectedPlayers();
        }, TimerFlags.REPEAT);

        if (_periodicCheckTimer == null)
        {
            Utils.Logger.LogError("PeriodicCheck", "Failed to create periodic check timer");
            return;
        }

        Utils.Logger.LogInfo("PeriodicCheck", $"Periodic check started - verifying every {checkInterval} seconds");
    }

    private void CheckAllConnectedPlayers()
    {
        var players = Utilities.GetPlayers();
        int checkedCount = 0;

        foreach (var player in players)
        {
            if (player == null || !player.IsValid || player.IsBot)
                continue;

            if (!SteamID.TryParse(player.SteamID.ToString(), out var steamId) || steamId == null)
                continue;

            _ = _playerAccessService!.CheckAndUpdatePlayerAccessAsync(steamId, player.PlayerName);
            checkedCount++;
        }

        Utils.Logger.LogDebug("PeriodicCheck", $"Checked {checkedCount} connected players");
    }

    private void OnMapStartHandler(string mapName)
    {
        AddTimer(2.0f, CheckAllConnectedPlayers);
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        if (!SteamID.TryParse(player.SteamID.ToString(), out var steamId) || steamId == null)
        {
            Utils.Logger.LogWarning("PlayerConnect", $"Failed to parse SteamID for player {player.PlayerName}");
            return HookResult.Continue;
        }

        var playerName = player.PlayerName;
        AddTimer(1.5f, async () =>
        {
            await _playerAccessService!.CheckAndUpdatePlayerAccessAsync(steamId, playerName);
        });

        Utils.Logger.LogInfo("PlayerConnect", $"Player {playerName} connected - will verify group membership");
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot)
            return HookResult.Continue;

        if (!SteamID.TryParse(player.SteamID.ToString(), out var steamId) || steamId == null)
            return HookResult.Continue;

        _playerAccessService?.RemovePlayer(steamId);
        Utils.Logger.LogDebug("PlayerDisconnect", $"Player {player.PlayerName} disconnected - removed from tracking");

        return HookResult.Continue;
    }

    [ConsoleCommand("css_steamaccess_reload")]
    [RequiresPermissions("@css/root")]
    public void OnReloadCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (_playerAccessService == null)
        {
            command.ReplyToCommand($"{Localizer["prefix"]} Error: Service not initialized");
            Utils.Logger.LogError("Command", "Attempted to reload but _playerAccessService is null");
            return;
        }

        _playerAccessService.ClearAll();

        AddTimer(1.0f, CheckAllConnectedPlayers);

        command.ReplyToCommand($"{Localizer["prefix"]} {Localizer["reload_success"]}");
        Utils.Logger.LogInfo("Command", "Player access reloaded - rechecking all connected players");
    }

    public override void Unload(bool hotReload)
    {
        _periodicCheckTimer?.Kill();

        _commandAccessService?.UnregisterCommandListeners();
        _playerAccessService?.ClearAll();

        Utils.Logger.LogInfo("Unload", "Plugin unloaded successfully");
    }
}