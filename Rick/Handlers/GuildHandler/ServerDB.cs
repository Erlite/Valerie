﻿using System.Threading.Tasks;
using System;
using System.Linq;
using Raven.Client;
using Rick.Handlers.GuildHandler.Models;
using Rick.Services.Logger;
using Rick.Services.Logger.Enums;
using Rick.Handlers.GuildHandler.Enum;

namespace Rick.Handlers.GuildHandler
{
    public class ServerDB
    {
        public static GuildModel GuildConfig(ulong GuildId)
        {
            using (IDocumentSession Session = MainHandler.Store.OpenSession())
            {
                return Session.Load<GuildModel>($"{$"{GuildId}"}");
            }
        }

        public static async Task LoadGuildConfigsAsync(ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Load = await Session.LoadAsync<GuildModel>($"{GuildId}");
                if (Load == null)
                {
                    await Session.StoreAsync(new GuildModel
                    {
                        Id = $"{GuildId}"
                    });
                    Log.Write(Status.KAY, Source.ServerDatabase, $"Database added with ID: {$"{GuildId}"}");
                }
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task DeleteGuildConfigAsync(ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var GetGuild = await Session.LoadAsync<GuildModel>($"{GuildId}");
                Session.Delete(GetGuild);
                Log.Write(Status.WRN, Source.ServerDatabase, $"Database removed with ID: {$"{GuildId}"}");
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task UpdateConfigAsync(ulong GuildId, ModelEnum ValueType, string Value = null)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                switch (ValueType)
                {
                    case ModelEnum.RolesAdd: Config.AssignableRoles.Add(Convert.ToUInt64(Value)); break;
                    case ModelEnum.RolesRemove: Config.AssignableRoles.Remove(Convert.ToUInt64(Value)); break;
                    case ModelEnum.KarmaEnabled: Config.IsKarmaEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.LeaveAdd: Config.LeaveMessages.Add(Value); break;
                    case ModelEnum.LeaveRemove: Config.LeaveMessages.Remove(Value); break;
                    case ModelEnum.ModCases: Config.ModCases += 1; break;
                    case ModelEnum.MuteId: Config.MuteRoleID = Convert.ToUInt64(Value); break;
                    case ModelEnum.AntiAdvertisement: Config.AntiAdvertisement = Convert.ToBoolean(Value); break;
                    case ModelEnum.Prefix: Config.Prefix = Value; break;
                    case ModelEnum.WelcomeAdd: Config.WelcomeMessages.Add(Value); break;
                    case ModelEnum.WelcomeRemove: Config.WelcomeMessages.Remove(Value); break;
                    case ModelEnum.CBChannel: Config.Chatterbot.TextChannel = Convert.ToUInt64(Value); break;
                    case ModelEnum.CBEnabled: Config.Chatterbot.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.JoinChannel: Config.JoinEvent.TextChannel = Convert.ToUInt64(Value); break;
                    case ModelEnum.JoinEnabled: Config.JoinEvent.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.LeaveChannel: Config.LeaveEvent.TextChannel = Convert.ToUInt64(Value); break;
                    case ModelEnum.LeaveEnabled: Config.LeaveEvent.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.ModChannel: Config.ModLog.TextChannel = Convert.ToUInt64(Value); break;
                    case ModelEnum.ModEnabled: Config.ModLog.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.StarChannel: Config.Starboard.TextChannel = Convert.ToUInt64(Value); break;
                    case ModelEnum.StarEnabled: Config.Starboard.IsEnabled = Convert.ToBoolean(Value); break;

                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task TagsHandlerAsync(ulong GuildId, ModelEnum ValueType, string Name = null, string Response = null, ulong Owner = 0, string Date = null)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                switch (ValueType)
                {
                    case ModelEnum.TagAdd:
                        var NewTag = new TagsModel
                        {
                            Name = Name,
                            Response = Response,
                            Owner = Owner,
                            CreationDate = Date
                        };
                        Config.TagsList.Add(NewTag);
                        break;
                    case ModelEnum.TagRemove:
                        var TagToRemove = Config.TagsList.FirstOrDefault(x => x.Name == Name);
                        Config.TagsList.Remove(TagToRemove);
                        break;
                    case ModelEnum.TagModify:
                        var GetTag = Config.TagsList.FirstOrDefault(x => x.Name == Name);
                        GetTag.Response = Response;
                        break;
                    case ModelEnum.TagUpdate:
                        var TagToUpdate = Config.TagsList.FirstOrDefault(x => x.Name == Name);
                        TagToUpdate.Uses += 1;
                        break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task AFKHandlerAsync(ulong GuildId, ModelEnum ValueType, ulong Id, string Message = null)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                switch (ValueType)
                {
                    case ModelEnum.AFKAdd: Config.AFKList.Add(Id, Message); break;
                    case ModelEnum.AFKRemove: Config.AFKList.Remove(Id); break;
                    case ModelEnum.AFKModify: Config.AFKList[Id] = Message; break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task KarmaHandlerAsync(ulong GuildId, ModelEnum ValueType, ulong Id, int Value)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                switch (ValueType)
                {
                    case ModelEnum.KarmaAdd: Config.KarmaList.Add(Id, Value); break;
                    case ModelEnum.KarmaDelete: Config.KarmaList.Remove(Id); break;
                    case ModelEnum.KarmaUpdate: Config.KarmaList[Id] += Value; break;
                    case ModelEnum.KarmaSubtract: Config.KarmaList[Id] -= Value; break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }
    }
}