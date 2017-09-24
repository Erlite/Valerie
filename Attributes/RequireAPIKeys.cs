using System;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Handlers;

namespace Valerie.Attributes
{
    public class RequireAPIKeys : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var Keys = (Context as ValerieContext).ValerieConfig.APIKeys;
            string Msg = null;
            bool IsNull = false;

            if (Keys.BingKey == null)
            {
                Msg = "Bot owner has not specified Microsoft Cogitive Services API Key in  config!";
                IsNull = true;
            }
            else if (Keys.CleverBotKey == null)
            {
                Msg = "Bot owner has not specified Cleverbot API Key in  config!";
                IsNull = true;
            }
            else if (Keys.GiphyKey == null)
            {
                Msg = "Bot owner has not specified Giphy API Key in  config!";
                IsNull = true;
            }
            else if (Keys.GoogleKey == null)
            {
                Msg = "Bot owner has not specified Google API Key in  config!";
                IsNull = true;
            }
            else if (Keys.MashapeKey == null)
            {
                Msg = "Bot owner has not specified Mashape API Key in  config!";
                IsNull = true;
            }
            else if (Keys.SearchEngineID == null)
            {
                Msg = "Bot owner has not specified Google Search Engine ID key in config!";
                IsNull = true;
            }
            else if (Keys.SteamKey == null)
            {
                Msg = "Bot owner has not specified Steam API Key in  config!";
                IsNull = true;
            }
            else if (Keys.TwitterKeys.AccessToken == null)
            {
                Msg = "Bot owner has not speficied Twitter's Access Token in config!";
                IsNull = true;
            }
            else if (Keys.TwitterKeys.AccessTokenSecret == null)
            {
                Msg = "Bot owner has not speficied Twitter's Access Token Secert in config!";
                IsNull = true;
            }
            else if (Keys.TwitterKeys.ConsumerKey == null)
            {
                Msg = "Bot owner has not speficied Twitter's Consumer Key in config!";
                IsNull = true;
            }
            else if (Keys.TwitterKeys.ConsumerSecret == null)
            {
                Msg = "Bot owner has not speficied Twitter's Consumer Secert in config!";
                IsNull = true;
            }

            return Task.FromResult(IsNull ? PreconditionResult.FromError(Msg) : PreconditionResult.FromSuccess());
        }
    }
}