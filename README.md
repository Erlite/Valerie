# DiscordExampleBot
This is a readme. Read it carefully.

---
### What you need:

- Visual Studio 2017
- Discord.Net 1.0 (Latest version please)
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
- If you had to create a new project then import the following:
  - Attributes (Folder)
  - Handlers (Folder)
  - Modules (Folder)
  - Services (Folder)
  - Core.cs
- If "paste" option is disabled make sure you are on core.cs file then right click on project name and paste.
![Copy](http://vvcap.com/img/9KYJEq6C9qH.png)
![Paste](http://vvcap.com/img/mAnGKIfu39O.png)
- Once imported you shouldn't be running into errors. Shall you encounter any error after importing then the problem is on your end.

---

### Problems you might run into:
- Response.json not found (Create one manually)
--- 
 ### Functions of this bot:
 
- [x] Uses Json for config, logs, points and responses
- [x] Uses embeds
- [x] Admin Commands (Kick, Ban, Serverlist (Requires Owner), Leave (Requires Owner), Delete
- [x] Bot Commands [Group Name = Set & Requires Owner] (Username, Nickname, Avatar, Game, Status)
- [x] General Commands (Guildinfo, Gif, Urban, Ping, Gift (Admin), Gift, Top, Roleinfo)
- [x] Enable/Disable certain loggings [Group Name = Log] (ModChannel, ServerChannel, Actions, Joins, Leaves, NameChange, NickChange, Banlog, Latency, Autorespond)
- [ ] Google Commands
- [x] Github Command/s => (Uses Fox's example) ##{Issue Number} pulls issue from my github
- [x] Autorespond json
- [x] Command Cooldown
- [x] Beautiful Console
- [ ] Bot's own website
- [x] Interactive Command Module (Some commands have been turned into interactive commands such as: Kick, Response)
- [ ] Option to use emoji as Command prefix
- [x] Easy to understand code? (I kinda accomplish that. Most of it is pretty straight forward)
- [x] No longer requires WS4NET. Already installed it and added it in `Core.cs`
