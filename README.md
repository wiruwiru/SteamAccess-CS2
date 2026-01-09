# SteamAccess
CounterStrikeSharp plugin that rewards Steam group members with automatic permissions and access to commands within your server.

## 🚀 Installation
1. Install [CounterStrike Sharp](https://github.com/roflmuffin/CounterStrikeSharp) and [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master)
2. Download [SteamAccess.zip](https://github.com/wiruwiru/SteamAccess-CS2/releases/latest) from releases
3. Extract and upload to your game server: `csgo/addons/counterstrikesharp/plugins/SteamAccess/`
4. Start server and configure the generated config file at `csgo/addons/counterstrikesharp/configs/plugins/SteamAccess/`

## 📋 Configuration parameters
| Parameter | Description |
|-----------|-------------|
| `GroupID` | Your Steam group's unique ID (found in the group URL). (**Default**: `0`) |
| `CheckInterval` | How often (in seconds) to verify player group membership. (**Default**: `60.0`) |
| `EnableGiveFlags` | Whether to automatically grant flags to group members. (**Default**: `true`) |
| `GiveFlags` | Array of permissions/groups to grant to members. Supports flags (`@css/vip`) or groups (`#css/vips`). (**Default**: `["@steamaccess/member"]`) |
| `EnableCommandAccess` | Enable command restriction system. (**Default**: `true`) |
| `RestrictedCommands` | List of commands that require group membership. (**Default**: `["css_yourcommand", "css_morecommands"]`) |
| `EnableDebug` | Enable detailed debug logging. (**Default**: `false`) |

## 🎮 Commands
| Command | Permission | Description |
|---------|------------|-------------|
| `css_steamaccess_reload` | `@css/root` | Force verification of all connected players |

## ❓ How to Find Your Steam Group ID
Simply go to your group's URL in your browser and add `/memberslistxml/?xml=1` to the end of the URL.
1. Navigate to your Steam group page (e.g., `steamcommunity.com/groups/yourgroup`)
2. Add `/memberslistxml/?xml=1` to the end of the URL
3. Copy the numbers inside `<groupID64>`. Example: `<groupID64>YOUR_ID</groupID64>`
---

## 📊 Support
For issues, questions, or feature requests, please visit our [GitHub Issues](https://github.com/wiruwiru/SteamAccess-CS2/issues) page.