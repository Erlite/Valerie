# DiscordExampleBot
This is a readme. Read it carefully.

---
### What you need:

- Visual Studio 2017
- Discord.Net 1.0 (Latest version please)
- Maybe Discord.Net.Providers.WS4Net =>
```cs
_client = new DiscordSocketClient(new DiscordSocketConfig { 
    WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance
 });
 ```
 - Latest .Net Framework (4.6 +)
 
 That's pretty much it.

---
### How to get started:
- Clone this repo
- Either open the project and let nuget restore the packages or Create a new Console Application targetting .Net Framework 4.6.1 and install the latest versions of the following: 
  - Discord.Net (1.0)
  - Discord.Addons.InteractiveCommands
  - Discord.Net.Core
  - Discord.Net.Rest
  
  Rest of them should install automatically. Such as: Discord.Net.Commands, Discord.Net.WebSocket
---
 
 ### Functions of this bot:
 
- [x] Uses Json for config, logs and points
- [x] Uses embeds
- [x] Admin Commands (Kick, Ban, Serverlist (Requires Owner), Leave (Requires Owner), Delete
- [x] Bot Commands [Group Name = Set & Requires Owner] (Username, Nickname, Avatar, Game, Status)
- [x] General Commands (Guildinfo, Gif, Urban, Ping, Gift (Admin), Gift, Top, Roleinfo)
- [x] Enable/Disable certain loggings [Group Name = Log] (ModChannel, ServerChannel, Actions, Joins, Leaves, NameChange, NickChange, Banlog, Latency, Autorespond)
- [ ] Google Commands
- [ ] Github Commands
- [ ] Autorespond json
- [x] Command Cooldown
- [x] Beautiful Console
- [ ] Bot's own website
- [ ] Interactive Command Module
- [ ] Option to use emoji as Command prefix
- [x] Easy to understand code? (I kinda accomplish that. Most of it is pretty straight forward)
