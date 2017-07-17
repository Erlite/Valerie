using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Tweetinvi;
using Rick.Extensions;
using Rick.Attributes;
using Rick.Enums;
using Rick.Handlers;
using Tweetinvi.Parameters;
using Tweetinvi.Models;
using Discord;

namespace Rick.Modules
{
    [CheckBlacklist, APICheck, RequireBotPermission(GuildPermission.SendMessages)]
    public class TwitterModule : ModuleBase
    {
        [Command("Tweet"), Summary("Tweets from @Vuxey account!"), Cooldown(30)]
        public async Task TweetAsync([Remainder] string TweetMessage)
        {
            if (TweetMessage.Length >= 120 || TweetMessage.Length <= 25)
            {
                await ReplyAsync("Tweet can't be longer than 120 characters and can't be shorter than 25 characters!");
                return;
            }

            var Filter = Functions.Function.Censor(TweetMessage);
            var Publish = $"{Filter} - {Context.User.Username}";
            if (Publish.Length > 140)
            {
                await ReplyAsync("Tweet's total length is greater than 140!");
                return;
            }

            var UserTweet = Tweet.PublishTweet(Filter);
            string ThumbImage = null;

            if (!string.IsNullOrWhiteSpace(User.GetAuthenticatedUser().ProfileImageUrlFullSize))
                ThumbImage = User.GetAuthenticatedUser().ProfileImageUrlFullSize;
            else
                ThumbImage = Context.Client.CurrentUser.GetAvatarUrl();

            var embed = EmbedExtension.Embed(EmbedColors.Green,
                $"{Context.User.Username} posted a tweet!", Context.User.GetAvatarUrl(), Description:
                $"**Tweet:** {TweetMessage}\n" +
                $"**Tweet ID:** {UserTweet.Id}\n" +
                $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})", ThumbUrl: ThumbImage);
            await ReplyAsync("", embed: embed);
        }

        [Command("TweetMedia"), Summary("Tweets with media from @Vuxey account!"),
            Remarks("TweetMedia \"https://Foo.com/Foo.png\"\"Tweet Message much wow\""), Cooldown(30)]
        public async Task MediaAsync(string URL, [Remainder] string TweetMessage)
        {
            string FileName = ConfigHandler.CacheFolder + "/" + Context.User.Username + $"{new Random().Next(1, 9999)}.png";
            await new HttpClient().DownloadAsync(new Uri(URL), FileName);

            if (TweetMessage.Length >= 120 || TweetMessage.Length <= 25)
            {
                await ReplyAsync("Tweet can't be longer than 120 characters and can't be shorter than 25 characters!");
                return;
            }

            var Filter = Functions.Function.Censor(TweetMessage);
            var Publish = $"{Filter} - {Context.User.Username}";
            if (Publish.Length > 140)
            {
                await ReplyAsync("Tweet's total length is greater than 140!");
                return;
            }

            string ThumbImage = null;

            if (!string.IsNullOrWhiteSpace(User.GetAuthenticatedUser().ProfileImageUrlFullSize))
                ThumbImage = User.GetAuthenticatedUser().ProfileImageUrlFullSize;
            else
                ThumbImage = Context.Client.CurrentUser.GetAvatarUrl();

            byte[] ImageFile = File.ReadAllBytes(FileName);
            var TweetMedia = Upload.UploadImage(ImageFile);

            var tweet = Tweet.PublishTweet(Filter, new PublishTweetOptionalParameters
            {
                Medias = new List<IMedia> { TweetMedia }
            });

            var embed = EmbedExtension.Embed(EmbedColors.Green,
                $"{Context.User.Username} posted a tweet!", Context.User.GetAvatarUrl(), Description:
                $"**Tweet:** {TweetMessage}\n" +
                $"**Tweet ID:** {tweet.Id}\n" +
                $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({tweet.Url})", ThumbUrl: ThumbImage);
            await ReplyAsync("", embed: embed);
        }

        [Command("Reply"), Summary("Replies back to a tweet!"), Cooldown(10)]
        public async Task ReplyAsync(long ID, [Remainder] string TweetMessage)
        {
            var ReplyTo = Tweet.GetTweet(ID);

            if (ReplyTo.IsTweetPublished)
            {
                if (TweetMessage.Length >= 120 || TweetMessage.Length <= 25)
                {
                    await ReplyAsync("Tweet can't be longer than 120 characters and can't be shorter than 25 characters!");
                    return;
                }

                var Filter = Functions.Function.Censor(TweetMessage);
                var Publish = $"{Filter} - {Context.User.Username}";
                if (Publish.Length > 140)
                {
                    await ReplyAsync("Tweet's total length is greater than 140!");
                    return;
                }

                var UserTweet = Tweet.PublishTweetInReplyTo(Publish, ReplyTo);
                string ThumbImage = null;

                if (!string.IsNullOrWhiteSpace(User.GetAuthenticatedUser().ProfileImageUrlFullSize))
                    ThumbImage = User.GetAuthenticatedUser().ProfileImageUrlFullSize;
                else
                    ThumbImage = Context.Client.CurrentUser.GetAvatarUrl();

                var embed = EmbedExtension.Embed(EmbedColors.Green,
                    $"{Context.User.Username} replied to a tweet!", Context.User.GetAvatarUrl(), Description:
                    $"**Original Tweet:** {ReplyTo.FullText}\n" +
                    $"**Reply:** {TweetMessage}\n" +
                    $"Tweet ID: {UserTweet.Id}\n" +
                    $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})", ThumbUrl: ThumbImage);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("DeleteTweet"), Summary("Deletes a specified tweet!")]
        public async Task DeleteTweetAsync(long ID)
        {
            var GetTweet = Tweet.GetTweet(ID);
            if (GetTweet.IsTweetPublished)
            {
                var Success = Tweet.DestroyTweet(GetTweet);
                await ReplyAsync($"Tweet with {ID} ID has been deleted!");
            }
        }
    }
}
