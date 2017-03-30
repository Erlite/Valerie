# How to Setup Bot

## Cloning
---
If you cloned the project and used Nuget restore to restore packages in the solution and everything worked fine then there is not much for you to read here.
But if you ran into some problem then read the `New Console Application` part.

## New Console Application
---
I'm assuming you either ran into a problem after cloning the project or you just wanted to start your own project. Whatever the case here are the steps:

Create new Console application targeting .Net Framework 4.6.1. Add this to your Nuget sources [Tools -> Options -> Nuget Package Manager -> Package sources]

?> *https://www.myget.org/F/discord-net/api/v3/index.json*

Install the latest version of the following from Nuget ( Make sure to check off prereleases):
  - Discord.Net 1.0 (This should also install Discord.Net.Commands, Discord.Net.WebSocket automatically. If it doesn't do it manually)
  - Discord.Net.Core
  - Discord.Net.Rest
  - Discord.Addons.InteractiveCommands
  - Discord.Addons.EmojiTools
  - Discord.Addons.WS4NetCompatibility

Once packages are installed, start importing the folders/files.

!>*FOLDERS:* Attributes, Classes, Handlers, Modules, Services

!>*FILES:* Core.cs

A simple copy paste should do the trick:

![Copy](http://vvcap.com/img/9KYJEq6C9qH.png)
![Paste](http://vvcap.com/img/mAnGKIfu39O.png)

If everything is done correctly, you shouldn't run into errors. If you run into an error after importing and the error is regarding some assembly not found then install that package VIA Nuget.

!> DO NOT UPDATE WS4NetCompatibility PACKAGE EVER! IT WILL CAUSE SOME KIND OF PROBLEM THAT I'M NOT AWARE OF!
