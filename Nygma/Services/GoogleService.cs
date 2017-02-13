using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using System.Text.RegularExpressions;
using Nygma.Handlers;
using Google.Apis.Customsearch.v1.Data;
using Google.Apis.Customsearch.v1;
using Google.Apis.Urlshortener.v1;
using Google.Apis.Urlshortener.v1.Data;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace Nygma.Services
{
    public class GoogleService
    {
        const string search_engine_id = "018084019232060951019:hs5piey28-e";
        private  YouTubeService yt;
        private CustomsearchService cs;
        private ConfigHandler config;
        private readonly Regex YtVideoIdRegex = new Regex(@"(?:youtube\.com\/\S*(?:(?:\/e(?:mbed))?\/|watch\?(?:\S*?&?v\=))|youtu\.be\/)(?<id>[a-zA-Z0-9_-]{6,11})", RegexOptions.Compiled);
        public GoogleService(ConfigHandler con)
        {
            var bcs = new BaseClientService.Initializer
            {
                ApplicationName = config.BotName,
                ApiKey = config.GAPI
            };
            config = con;
            yt = new YouTubeService(bcs);
            cs = new CustomsearchService(bcs);
        }

        public async Task<IEnumerable<string>> GetVideosByKeywordsAsync(string keywords, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                throw new ArgumentNullException(nameof(keywords));

            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            string id = "";
            var match = YtVideoIdRegex.Match(keywords);
            if (match.Length > 1)
            {
                id = match.Groups["id"].Value;
            }
            if (!string.IsNullOrWhiteSpace(id))
            {
                return new[] { "http://www.youtube.com/watch?v=" + id };
            }
            var query = yt.Search.List("snippet");
            query.MaxResults = count;
            query.Q = keywords;
            query.Type = "video";
            return (await query.ExecuteAsync()).Items.Select(i => "http://www.youtube.com/watch?v=" + i.Id.VideoId);
        }

        public async Task<ImageResult> GetImageAsync(string query, int start = 1)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            var req = cs.Cse.List(query);
            req.Cx = search_engine_id;
            req.Num = 1;
            req.Fields = "items(image(contextLink,thumbnailLink),link)";
            req.SearchType = CseResource.ListRequest.SearchTypeEnum.Image;
            req.Start = start;

            var search = await req.ExecuteAsync().ConfigureAwait(false);

            return new ImageResult(search.Items[0].Image, search.Items[0].Link);
        }

        public async Task<GoogleShortenUrlResponse> ShortenUrl(string url)
        {
            string json = null;
            using (var client = new HttpClient())
            {
                var message = new HttpRequestMessage();
                message.Method = HttpMethod.Post;
                message.Content = new StringContent("{\"longUrl\": \"" + url + "\"}", Encoding.UTF8, "application/json");
                message.RequestUri = new Uri("https://www.googleapis.com/urlshortener/v1/url?key=" + config.GAPI);
                json = await client.SendAsync(message).Result.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<GoogleShortenUrlResponse>(json);
        }

    }

    public struct ImageResult
    {
        public Result.ImageData Image { get; }
        public string Link { get; }

        public ImageResult(Result.ImageData image, string link)
        {
            this.Image = image;
            this.Link = link;
        }
    }
    public struct GoogleSearchResult
    {
        public string Title { get; }
        public string Link { get; }
        public string Text { get; }

        public GoogleSearchResult(string title, string link, string text)
        {
            this.Title = title;
            this.Link = link;
            this.Text = text;
        }
    }
    public class GoogleShortenUrlResponse
    {
        [JsonProperty("id")]
        public Uri ShortenedUrl { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("longUrl")]
        public Uri LongUrl { get; set; }
    }
}
