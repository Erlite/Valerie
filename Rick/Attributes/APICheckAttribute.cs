using System;
using Discord.Commands;
using Rick.Handlers;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class APICheckAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Command, IServiceProvider Provider)
        {
            var Keys = ConfigHandler.IConfig.APIKeys;
            string Msg = null;

            bool Empty = false;

            if (Keys.BingKey == null)
            {
                Msg = "Bot owner has not specified Microsoft Cogitive Services API Key in  config!";
                Empty = true;
            }
            else if (Keys.CleverBotKey == null)
            {
                Msg = "Bot owner has not specified Cleverbot API Key in  config!";
                Empty = true;
            }
            else if (Keys.GiphyKey == null)
            {
                Msg = "Bot owner has not specified Giphy API Key in  config!";
                Empty = true;
            }
            else if (Keys.GoogleKey == null)
            {
                Msg = "Bot owner has not specified Google API Key in  config!";
                Empty = true;
            }
            else if (Keys.MashapeKey == null)
            {
                Msg = "Bot owner has not specified Mashape API Key in  config!";
                Empty = true;
            }
            else if (Keys.SearchEngineID == null)
            {
                Msg = "Bot owner has not specified Google Search Engine ID key in config!";
                Empty = true;
            }
            else if (Keys.SteamKey == null)
            {
                Msg = "Bot owner has not specified Steam API Key in  config!";
                Empty = true;
            }
            else if (Keys.TwitterKeys.AccessToken == null)
            {
                Msg = "Bot owner has not speficied Twitter's Access Token in config!";
                Empty = true;
            }
            else if (Keys.TwitterKeys.AccessTokenSecret == null)
            {
                Msg = "Bot owner has not speficied Twitter's Access Token Secert in config!";
                Empty = true;
            }
            else if (Keys.TwitterKeys.ConsumerKey == null)
            {
                Msg = "Bot owner has not speficied Twitter's Consumer Key in config!";
                Empty = true;
            }
            else if (Keys.TwitterKeys.ConsumerSecret == null)
            {
                Msg = "Bot owner has not speficied Twitter's Consumer Secert in config!";
                Empty = true;
            }

            return await Task.FromResult(Empty) ? PreconditionResult.FromError(Msg) : PreconditionResult.FromSuccess();
        }
    }
}
