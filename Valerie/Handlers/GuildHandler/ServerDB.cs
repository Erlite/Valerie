using System.Threading.Tasks;
using System;
using System.Linq;
using Valerie.Handlers.GuildHandler.Models;
using Valerie.Services.Logger;
using Valerie.Services.Logger.Enums;
using Valerie.Handlers.GuildHandler.Enum;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers.GuildHandler
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
                    case ModelEnum.RolesAdd: Config.AssignableRoles.Add(Value); break;
                    case ModelEnum.RolesRemove: Config.AssignableRoles.Remove(Value); break;
                    case ModelEnum.KarmaEnabled: Config.KarmaHandler.IsKarmaEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.LeaveAdd: Config.LeaveMessages.Add(Value); break;
                    case ModelEnum.LeaveRemove: Config.LeaveMessages.Remove(Value); break;
                    case ModelEnum.ModCases: Config.ModCases += 1; break;
                    case ModelEnum.MuteId: Config.MuteRoleID = Convert.ToUInt64(Value); break;
                    case ModelEnum.AntiAdvertisement: Config.AntiAdvertisement = Convert.ToBoolean(Value); break;
                    case ModelEnum.Prefix: Config.Prefix = Value; break;
                    case ModelEnum.WelcomeAdd: Config.WelcomeMessages.Add(Value); break;
                    case ModelEnum.WelcomeRemove: Config.WelcomeMessages.Remove(Value); break;
                    case ModelEnum.CBChannel: Config.Chatterbot.TextChannel = Value; break;
                    case ModelEnum.CBEnabled: Config.Chatterbot.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.JoinChannel: Config.JoinEvent.TextChannel = Value; break;
                    case ModelEnum.JoinEnabled: Config.JoinEvent.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.LeaveChannel: Config.LeaveEvent.TextChannel = Value; break;
                    case ModelEnum.LeaveEnabled: Config.LeaveEvent.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.ModChannel: Config.ModLog.TextChannel = Value; break;
                    case ModelEnum.ModEnabled: Config.ModLog.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.StarChannel: Config.Starboard.TextChannel = Value; break;
                    case ModelEnum.StarEnabled: Config.Starboard.IsEnabled = Convert.ToBoolean(Value); break;
                    case ModelEnum.KarmaMaxRoleLevel: Config.KarmaHandler.MaxRolesLevel = int.Parse(Value); break;

                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task TagsHandlerAsync(ulong GuildId, ModelEnum ValueType, string Name = null, string Response = null, string Owner = null, string Date = null)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                var GetTag = Config.TagsList.FirstOrDefault(x => x.Name == Name);
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
                    case ModelEnum.TagRemove: Config.TagsList.Remove(GetTag); break;
                    case ModelEnum.TagModify: GetTag.Response = Response; break;
                    case ModelEnum.TagUpdate: GetTag.Uses += 1; break;
                    case ModelEnum.TagPurge:
                        Config.TagsList.RemoveAll(x => x.Owner == Owner);
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

        public static async Task KarmaHandlerAsync(ulong GuildId, ModelEnum ValueType, ulong Id, int Value = 0)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                switch (ValueType)
                {
                    case ModelEnum.KarmaNew: Config.KarmaHandler.UsersList.Add(Id, Value); break;
                    case ModelEnum.KarmaDelete: Config.KarmaHandler.UsersList.Remove(Id); break;
                    case ModelEnum.KarmaUpdate: Config.KarmaHandler.UsersList[Id] += Value; break;
                    case ModelEnum.KarmaSubtract: Config.KarmaHandler.UsersList[Id] -= Value; break;
                    case ModelEnum.KarmaBLAdd: Config.KarmaHandler.BlacklistRoles.Add(Id.ToString()); break;
                    case ModelEnum.KarmaBLRemove: Config.KarmaHandler.BlacklistRoles.Remove(Id.ToString()); break;
                    case ModelEnum.KarmaRoleAdd: Config.KarmaHandler.LevelUpRoles.Add(Id, Value); break;
                    case ModelEnum.KarmaRoleRemove: Config.KarmaHandler.LevelUpRoles.Remove(Id); break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }

        public static async Task StarboardHandlerAsync(ulong GuildId, ModelEnum ValueType, ulong MsgId, ulong ChannelId, ulong SMsgId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Config = await Session.LoadAsync<GuildModel>($"{GuildId}");
                var GetMessage = Config.StarredMessages.FirstOrDefault(x => x.MessageId == MsgId.ToString());
                switch (ValueType)
                {
                    case ModelEnum.StarNew:
                        Config.StarredMessages.Add(new Starboard
                        {
                            ChannelId = ChannelId.ToString(),
                            MessageId = MsgId.ToString(),
                            StarboardMessageId = SMsgId.ToString(),
                            Stars = 1
                        });
                        break;
                    case ModelEnum.StarAdd: GetMessage.Stars += 1; break;
                    case ModelEnum.StarDelete: Config.StarredMessages.Remove(GetMessage); break;
                    case ModelEnum.StarSubtract: GetMessage.Stars -= 1; break;
                }
                await Session.StoreAsync(Config);
                await Session.SaveChangesAsync();
                Session.Dispose();
            }
        }
    }
}